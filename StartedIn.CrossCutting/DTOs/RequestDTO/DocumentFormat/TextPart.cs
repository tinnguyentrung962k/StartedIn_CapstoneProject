using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.DocumentFormat
{
    public class TextPart
    {
        public string Text { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
    }
}
