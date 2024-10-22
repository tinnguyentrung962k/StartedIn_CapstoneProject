using Net.payOS.Types;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IPayOsService
    {
        Task<PaymentResultResponseDTO> PaymentWithPayOs(string bookingId);
        Task<PaymentLinkInformation> GetPaymentStatus(string bookingId);
    }
}
