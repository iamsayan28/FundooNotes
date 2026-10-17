using ModelLayer.Entity;
using ModelLayer.Models.Note;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interface
{
    public interface INoteService
    {
        public Task<NoteEntity> CreateNote(CreateNoteModel model, int userId);
        public Task<IEnumerable<NoteEntity>> GetAllNotesByUserId(int id);
        public Task<NoteEntity?> UpdateNote(UpdateNoteModel model, int noteId, int userId);
        public Task<bool> DeleteNote(int noteId, int userId);
        public Task<NoteEntity?> GetNoteById(int noteId, int userId);
        public Task<IEnumerable<NoteEntity>> GetArchivedNotes(int userId);
        public Task<NoteEntity?> ArchiveNote(int noteId, int userId);
        public Task<NoteEntity?> UnarchiveNote(int noteId, int userId);
    }
}
