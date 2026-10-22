using BusinessLayer.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using ModelLayer.Models.Note;
using RepositoryLayer.Interface;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace BusinessLayer.Service
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepo;
        private readonly IUserRepository _userRepo;
        private readonly IDistributedCache _cache;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<NoteService> _logger;
        public NoteService(INoteRepository noteRepo, IUserRepository userRepo, IDistributedCache cache, IEncryptionService encryptionService, ILogger<NoteService> logger)
        {
            _cache = cache;
            _noteRepo = noteRepo;
            _userRepo = userRepo;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        private async Task InvalidateNoteCache(int userId, int? noteId = null)
        {
            await _cache.RemoveAsync($"notes:active:{userId}");
            await _cache.RemoveAsync($"notes:archived:{userId}");

            if (noteId.HasValue)
            {
                await _cache.RemoveAsync($"notes:detail:{userId}:{noteId.Value}");
            }
        }

        private NoteEntity DecryptNote(NoteEntity note)
        {
            note.Title = _encryptionService.Decrypt(note.Title);
            if(note.Description != null ) note.Description = _encryptionService.Decrypt(note.Description);

            return note;
        }

        private NoteEntity CreateDecryptedCopy(NoteEntity encryptedNote)
        {
            var copy = JsonSerializer.Deserialize<NoteEntity>(JsonSerializer.Serialize(encryptedNote));

            if (copy == null) throw new InvalidOperationException("Could not create note response.");

            return DecryptNote(copy);
        }

        public async Task<IEnumerable<NoteEntity>> GetAllNotesByUserId(int userId)
        {
            string cacheKey = $"notes:active:{userId}";

            var cachedNotes = await _cache.GetStringAsync(cacheKey);

            if(cachedNotes != null)
            {
                try
                {
                    var listOfNotes = JsonSerializer.Deserialize<List<NoteEntity>>(cachedNotes);
                    if (listOfNotes != null)
                    {
                        foreach (var note in listOfNotes)
                        {
                            DecryptNote(note);
                        }
                        return listOfNotes;
                    }
                }
                catch (JsonException ex)
                {
                    // Invalid cache data; fetch from database.
                    _logger.LogWarning(ex, "Invalid cached notes for user {UserId}", userId);
                }
                catch (CryptographicException ex)
                {
                    // Cached ciphertext is invalid or cannot be decrypted.
                    _logger.LogWarning(ex, "Unable to decrypt cached notes for user {UserId}", userId);
                }
                await _cache.RemoveAsync(cacheKey);
            }

            // Get notes from DB -> Serialize it to put it into redis -> then decrypt the notes we got from DB
            var notes = await _noteRepo.GetAllNotesByUserId(userId);

            var serializedNotes = JsonSerializer.Serialize(notes);
            await _cache.SetStringAsync(cacheKey, serializedNotes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            //foreach (var note in notes)
            //{
            //    DecryptNote(note);
            //}
            //return notes;
            return notes.Select(note => CreateDecryptedCopy(note)).ToList();
        }

        public async Task<NoteEntity> CreateNote(CreateNoteModel model, int userId)
        {
            var note = new NoteEntity()
            {
                UserId = userId,
                Title = _encryptionService.Encrypt(model.Title),
                Description = model.Description == null ? null : _encryptionService.Encrypt(model.Description),
                CreatedOn = DateTime.UtcNow,
            };

            var createdNote = await _noteRepo.AddNote(note);

            // -- to prevent retrieving stale cache -- 
            await InvalidateNoteCache(userId);  

            return CreateDecryptedCopy(createdNote);
        }

        public async Task<NoteEntity?> UpdateNote(UpdateNoteModel model, int noteId, int userId)
        {
            var note = await _noteRepo.GetNoteByUserId(noteId, userId);

            if (note == null) return null;

            if (!string.IsNullOrWhiteSpace(model.Title)) note.Title = _encryptionService.Encrypt(model.Title);
            if (model.ClearDescription) note.Description = null;
            if (model.Description != null) note.Description = _encryptionService.Encrypt(model.Description);
            
            note.UpdatedOn = DateTime.UtcNow;
            
            var updatedNote = await _noteRepo.UpdateNote(note);
            await InvalidateNoteCache(userId, noteId);

            return CreateDecryptedCopy(updatedNote);
        }

        public async Task<bool> DeleteNote(int noteId, int userId)
        {
            var result = await _noteRepo.DeleteNote(noteId, userId);
            
            if(result) await InvalidateNoteCache(userId, noteId);
            
            return result;
        }

        public async Task<NoteEntity?> GetNoteById(int noteId, int userId)
        {
            string cacheKey = $"notes:detail:{userId}:{noteId}";

            var cachedNote = await _cache.GetStringAsync(cacheKey);

            if (cachedNote != null)
            {
                try
                {
                    var cachedEntity = JsonSerializer.Deserialize<NoteEntity>(cachedNote);
                    if(cachedEntity != null) return DecryptNote(cachedEntity);
                }catch(JsonException ex)
                {
                    _logger.LogWarning(ex, "Invalid cached note for user {UserId} and note {NoteId}", userId, noteId);
                }
                catch(CryptographicException ex)
                {
                    _logger.LogWarning(ex, "Unable to decrypt cached notes for user {UserId} and note {NoteId}", userId, noteId);
                }
                await _cache.RemoveAsync(cacheKey);
            }

            //--
            var note = await _noteRepo.GetNoteById(noteId, userId);
            if (note == null) return null;

            var serializedNote = JsonSerializer.Serialize(note);
            await _cache.SetStringAsync(cacheKey, serializedNote,new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return CreateDecryptedCopy(note);
        }

        public async Task<IEnumerable<NoteEntity>> GetArchivedNotes(int userId)
        {
            string cacheKey = $"notes:archived:{userId}";

            var cachedNotes = await _cache.GetStringAsync(cacheKey);
            if (cachedNotes != null)
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<NoteEntity>>(cachedNotes);
                    if(list != null)
                    {
                        foreach(var note in list)
                        {
                            DecryptNote(note);
                        }
                        return list;
                    }
                }
                catch (JsonException ex)
                {
                    // Invalid cache data; fetch from database.
                    _logger.LogWarning(ex, "Invalid cached notes for user {UserId}", userId);
                }
                catch (CryptographicException ex)
                {
                    // Cached ciphertext is invalid or cannot be decrypted.
                    _logger.LogWarning(ex, "Unable to decrypt cached notes for user {UserId}", userId);
                }
                await _cache.RemoveAsync(cacheKey);
            }


            // if cached notes from redis are null, then we retrieve notes from DB -> Serialize it(to put into redis) -> after redis, return og notes
            // that we got from DB, and decrypting each note
            var notes = await _noteRepo.GetArchivedNotes(userId);
            var serializedNotes = JsonSerializer.Serialize(notes);
            await _cache.SetStringAsync(cacheKey, serializedNotes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            //foreach(var note in notes)
            //{
            //    DecryptNote(note);
            //}
            //return notes;
            return notes.Select(note => CreateDecryptedCopy(note)).ToList();
        }

        public async Task<NoteEntity?> ArchiveNote(int noteId, int userId)
        {
            var archivedNote = await _noteRepo.ArchiveNote(noteId, userId);
            if (archivedNote == null) return null; 

            await InvalidateNoteCache(userId, noteId);

            return CreateDecryptedCopy(archivedNote);
        }

        public async Task<NoteEntity?> UnarchiveNote(int noteId, int userId)
        {
            var unarchivedNote = await _noteRepo.UnarchiveNote(noteId, userId);
            if (unarchivedNote == null) return null;

            await InvalidateNoteCache(userId, noteId);

            return CreateDecryptedCopy(unarchivedNote);
        }
    }
}
