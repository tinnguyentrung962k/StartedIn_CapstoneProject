using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Appointment
{
    public class AppointmentInCalendarResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
    }
}
