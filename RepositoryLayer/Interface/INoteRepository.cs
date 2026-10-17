using ModelLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interface
{
    public interface INoteRepository
    {
        public Task<NoteEntity> AddNote(NoteEntity note);
        public Task<IEnumerable<NoteEntity>> GetAllNotesByUserId(int id);
        public Task<NoteEntity?> GetNoteByUserId(int noteId, int userId);
        public Task<NoteEntity> UpdateNote(NoteEntity note);
        public Task<bool> DeleteNote(int noteId, int userId);
        public Task<NoteEntity?> GetNoteById(int noteId, int userId);
        public Task<IEnumerable<NoteEntity>> GetArchivedNotes(int userId);
        public Task<NoteEntity?> ArchiveNote(int noteId, int userId);
        public Task<NoteEntity?> UnarchiveNote(int noteId, int userId);
    }
}