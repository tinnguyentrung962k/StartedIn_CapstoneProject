using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class UpdateTaskboardPositionDTO
    {
        public string Id { get; set; }
        public int Position { get; set; }
        public bool NeedsReposition { get; set; }
    }
}
