using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IDisbursementService
    {
        Task FinishedTheTransaction(string disbursementId, string projectId, string apiKey);
        Task CancelPayment(string disbursementId, string projectId, string apiKey);
    }
}
