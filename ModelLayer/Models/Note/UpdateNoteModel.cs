using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Models.Note
{
    public class UpdateNoteModel
    {
        [StringLength(200, ErrorMessage = "Title should be within 200 characters")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool ClearDescription { get; set; } = false;
    }
}
