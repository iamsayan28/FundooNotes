using Microsoft.EntityFrameworkCore;
using ModelLayer.Entity;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Repository
{
    public class NoteRepository : INoteRepository
    {
        private readonly FundooDbContext _context;
        public NoteRepository(FundooDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NoteEntity>> GetAllNotesByUserId(int id)
        {
            return await _context.Notes.Where(n => n.UserId == id && !n.IsArchived).ToListAsync();
        }

        public async Task<NoteEntity> AddNote(NoteEntity note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<NoteEntity?> GetNoteByUserId(int noteId, int userId)
        {
            return await _context.Notes.FirstOrDefaultAsync(u => u.UserId == userId && u.Id == noteId);
        }

        public async Task<NoteEntity> UpdateNote(NoteEntity note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<bool> DeleteNote(int noteId, int userId)
        {
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

            if (note == null)
                return false;

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<NoteEntity?> GetNoteById(int noteId, int userId)
        {
            return await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);
        }

        public async Task<IEnumerable<NoteEntity>> GetArchivedNotes(int userId)
        {
            return await _context.Notes.Where(n => n.UserId == userId && n.IsArchived).ToListAsync();
        }

        public async Task<NoteEntity?> ArchiveNote(int noteId, int userId)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

            if (note == null)
                return null;

            note.IsArchived = true;
            note.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return note;
        }

        public async Task<NoteEntity?> UnarchiveNote(int noteId, int userId)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.IsArchived);

            if (note == null)
                return null;

            note.IsArchived = false;
            note.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return note;
        }
    }
}
