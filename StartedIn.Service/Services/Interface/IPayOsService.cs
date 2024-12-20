﻿using Net.payOS.Types;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IPayOsService
    {
        Task<string> PaymentWithPayOs(string userId, string disbursementId);
        Task<PaymentLinkInformation> GetPaymentStatus(string userId, string disbursementId, string projectId);
    }
}
