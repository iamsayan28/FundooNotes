using ModelLayer.Entity;
using RepositoryLayer.Interface;
using BusinessLayer.Interface;
using System.Security.Claims;
using ModelLayer.Models.Note;

namespace BusinessLayer.Service
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepo;
        private readonly IUserRepository _userRepo;
        public NoteService(INoteRepository noteRepo, IUserRepository userRepo)
        {
            _noteRepo = noteRepo;
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<NoteEntity>> GetAllNotesByUserId(int id)
        {
            return await _noteRepo.GetAllNotesByUserId(id);
        }

        public async Task<NoteEntity> CreateNote(CreateNoteModel model, int userId)
        {
            var noteEntity = new NoteEntity()
            {
                UserId = userId,
                Title = model.Title,
                Description = model.Description,
                CreatedOn = DateTime.UtcNow,
            };

            var createdNote = await _noteRepo.AddNote(noteEntity);
            return createdNote;
        }

        public async Task<NoteEntity?> UpdateNote(UpdateNoteModel model, int noteId, int userId)
        {
            var note = await _noteRepo.GetNoteByUserId(noteId, userId);

            if (note == null) return null;

            if (!string.IsNullOrWhiteSpace(model.Title))
                note.Title = model.Title;

            if (model.ClearDescription)
                note.Description = null;
            if (model.Description != null)
                note.Description = model.Description;
            

            note.UpdatedOn = DateTime.UtcNow;
            
            var updatedNote = await _noteRepo.UpdateNote(note);
            return updatedNote;
        }

        public async Task<bool> DeleteNote(int noteId, int userId)
        {
            return await _noteRepo.DeleteNote(noteId, userId);
        }

        public async Task<NoteEntity?> GetNoteById(int noteId, int userId)
        {
            return await _noteRepo.GetNoteById(noteId, userId);
        }

        public async Task<IEnumerable<NoteEntity>> GetArchivedNotes(int userId)
        {
            return await _noteRepo.GetArchivedNotes(userId);
        }

        public async Task<NoteEntity?> ArchiveNote(int noteId, int userId)
        {
            return await _noteRepo.ArchiveNote(noteId, userId);
        }

        public async Task<NoteEntity?> UnarchiveNote(int noteId, int userId)
        {
            return await _noteRepo.UnarchiveNote(noteId, userId);
        }
    }
}
