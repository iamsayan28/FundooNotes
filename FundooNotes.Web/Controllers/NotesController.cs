using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Entity;
using ModelLayer.Models;
using System.Globalization;
using System.Security.Claims;
using BusinessLayer.Service;
using ModelLayer.Models.Note;
using Microsoft.Extensions.Caching.Distributed;

namespace FundooNotes.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;
        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet("test-redis")]
        //[Route()]
        public async Task<IActionResult> TestRedis([FromServices] IDistributedCache cache)
        {
            await cache.SetStringAsync("test-key", "Redis connected!");

            var value = await cache.GetStringAsync("test-key"); 

            return Ok(new { message = value });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var notes = await _noteService.GetAllNotesByUserId(userId);

            return Ok(new ResponseModel<IEnumerable<NoteEntity>>
            {
                Success = true,
                Message = "Notes retrieved successfully",
                Data = notes
            });
        }

        [HttpPost]
        [Route("create-notes")]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteModel model)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var createdNote = await _noteService.CreateNote(model, userId);

            return CreatedAtAction(
                nameof(CreateNote),
                new { id = createdNote.Id },
                new ResponseModel<NoteEntity>()
                {
                    Success = true,
                    Message = "Successful Note Creation",
                    Data = createdNote
                });
        }

        [HttpPost]
        [Route("update-note/{noteId}")]
        public async Task<IActionResult> UpdateNote(int noteId, [FromBody] UpdateNoteModel model)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId)) return Unauthorized();

            var updatedNote = await _noteService.UpdateNote(model, noteId, userId);

            if(updatedNote == null)
            {
                return NotFound(new ResponseModel<NoteEntity>
                {
                    Success = false,
                    Message = "Note not found.",
                    //Data = null
                });
            }

            return Ok(new ResponseModel<NoteEntity>
            {
                Success = true,
                Message = "Note updated successfully.",
                Data = updatedNote
            });
        }

        [HttpDelete("delete-note/{noteId}")]
        public async Task<IActionResult> DeleteNote(int noteId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var isDeleted = await _noteService.DeleteNote(noteId, userId);

            if (!isDeleted)
            {
                return NotFound(new ResponseModel<object>
                {
                    Success = false,
                    Message = "Note not found.",
                    Data = null
                });
            }

            return Ok(new ResponseModel<object>
            {
                Success = true,
                Message = "Note deleted successfully.",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var note = await _noteService.GetNoteById(id, userId);

            if (note == null)
            {
                return NotFound(new ResponseModel<NoteEntity>
                {
                    Success = false,
                    Message = "Note not found.",
                    Data = null
                });
            }

            return Ok(new ResponseModel<NoteEntity>
            {
                Success = true,
                Message = "Note retrieved successfully.",
                Data = note
            });
        }

        [HttpGet]
        [Route("archived")]
        public async Task<IActionResult> GetArchiveNotes()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId)) return Unauthorized();
            
            var archivedNotes = await _noteService.GetArchivedNotes(userId);

            return Ok(new ResponseModel<IEnumerable<NoteEntity>>
            {
                Success = true,
                Message = "Archived notes retrieved successfully.",
                Data = archivedNotes
            });
        }


        [Authorize]
        [HttpPut("archive-note/{id}")]
        public async Task<IActionResult> ArchiveNote(int id)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var note = await _noteService.ArchiveNote(id, userId);

            if (note == null)
            {
                return NotFound(new ResponseModel<NoteEntity>
                {
                    Success = false,
                    Message = "Note not found.",
                    Data = null
                });
            }

            return Ok(new ResponseModel<NoteEntity>
            {
                Success = true,
                Message = "Note archived successfully.",
                Data = note
            });
        }

        [Authorize]
        [HttpPut("unarchive-note/{id}")]
        public async Task<IActionResult> UnarchiveNote(int id)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var note = await _noteService.UnarchiveNote(id, userId);

            if (note == null)
            {
                return NotFound(new ResponseModel<NoteEntity>
                {
                    Success = false,
                    Message = "Note not found.",
                    Data = null
                });
            }

            return Ok(new ResponseModel<NoteEntity>
            {
                Success = true,
                Message = "Note unarchived successfully.",
                Data = note
            });
        }
    }
}
