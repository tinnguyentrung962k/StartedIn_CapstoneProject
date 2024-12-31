using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class Appointment : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }

        [ForeignKey(nameof(Milestone))]
        public string? MilestoneId { get; set; }

        [ForeignKey(nameof(TerminationRequest))]
        public string? TerminationRequestId { get; set; }
        
        [ForeignKey(nameof(Contract))]
        public string? ContractId { get; set; }
        [MaxLength(50)]
        public string Title { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
        public DateTimeOffset? AppointmentEndTime { get; set; }
        [MaxLength(255)]
        public string? Description { get; set; }
        public string MeetingLink { get; set; }
        public MeetingStatus Status { get; set; }
        public Project Project { get; set; }
        public Milestone? Milestone { get; set; }
        public TerminationRequest? TerminationRequest { get; set; }
        public Contract? Contract { get; set; }
        public ICollection<MeetingNote>? MeetingNotes { get; set; }
        public ICollection<Document>? Documents { get; set; }
        public ICollection<UserAppointment> UserAppointments { get; set; }
    }
}
