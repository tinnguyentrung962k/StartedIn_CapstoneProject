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
        public string? IdCardNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? AcademicYear { get; set; }

        [StringLength(120)]
        public string? Bio { get; set; }
        public DateTimeOffset? Verified { get; set; }
        public string? RefreshToken { get; set; }
        [JsonIgnore] public virtual IEnumerable<UserRole> UserRoles { get; }
        public ICollection<UserProject>? UserProjects { get; set; }
        public IEnumerable<UserContract>? UserContracts { get; set; }
        public ICollection<ShareEquity>? ShareEquities { get; set; }
        public ICollection<DealOffer>? DealOffers { get; set; }
        public ICollection<Application>? Applications { get; set; }
        public ICollection<Disbursement>? Disbursements { get; set; }
        public ICollection<UserAppointment>? UserAppointments { get; set; }
    }
}
