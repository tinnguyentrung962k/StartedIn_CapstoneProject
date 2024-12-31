using System.ComponentModel.DataAnnotations.Schema;

namespace StartedIn.Domain.Entities;

public class UserAppointment
{
    [ForeignKey(nameof(Appointment))]
    public string AppointmentId { get; set; }
    [ForeignKey(nameof(User))]
    public string UserId { get; set; }
    
    public virtual Appointment Appointment { get; set; }
    public virtual User User { get; set; }
}