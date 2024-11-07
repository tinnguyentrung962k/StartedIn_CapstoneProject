using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.BaseDTO
{
    public class AuditResponseDTO : IdentityResponseDTO
    {
        public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastUpdatedTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DeletedTime { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
    }
}
