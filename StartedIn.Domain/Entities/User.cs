using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StartedIn.Domain.Entities
{
    public class User : IdentityUser
    {
        public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastUpdatedTime { get; set; } = DateTimeOffset.UtcNow;
        public string FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? CoverPhoto { get; set; }
        public string? StudentCode { get; set; }

        [StringLength(120)]
        public string? Bio { get; set; }
        public DateTimeOffset? Verified { get; set; }
        public string? RefreshToken { get; set; }
        [JsonIgnore] public virtual IEnumerable<UserRole> UserRoles { get; }
    }
}
