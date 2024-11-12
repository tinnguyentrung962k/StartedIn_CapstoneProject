using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class MeetingNote : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Appointment))]
        public string AppointmentId { get; set; }
        public string HostId { get; set; }
        public string SecretaryId { get; set; }
        public string MeetingContent { get; set; }
        public string Conclusion { get; set; }
        public string? SignNowFileId { get; set; }
        public Appointment Appointment { get; set; }
    }
}
