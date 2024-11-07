using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Net.payOS.Types;
using Net.payOS;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Repository.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartedIn.Service.Services.Interface;
using StartedIn.Domain.Entities;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Payment;

namespace StartedIn.Service.Services
{
    public class PayOsService : IPayOsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        //private readonly PayOsSetting payOSSetting;
        private readonly string _websiteDomain;
        private PayOS _payOS;
        public PayOsService(IUnitOfWork unitOfWork, IConfiguration configuration, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userManager = userManager;
            //payOSSetting = new PayOsSetting()
            //{
            //    ClientId = _configuration.GetValue<string>("PayOSClientId"),
            //    ApiKey = _configuration.GetValue<string>("PayOSApiKey"),
            //    ChecksumKey = _configuration.GetValue<string>("PayOSChecksumKey")
            //};
            //_payOS = new PayOS(payOSSetting.ClientId, payOSSetting.ApiKey, payOSSetting.ChecksumKey);
            _websiteDomain = _configuration.GetValue<string>("WebSiteDomain");
        }
        public async Task<PaymentResultResponseDTO> PaymentWithPayOs(string disbursementId)
        {

            ////Khai báo DataDetailList
            //List<ItemData> items = new List<ItemData>();
            ////Khai báo content chuyển khoản
            //string content = $"{user.FullName} - {booking.BookingCode}";
            //int expiredAt = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * 5)); // 5 minutes from now

            //long orderCodeLong = disbursementId.OrderCode; // or use ConvertGuidToLong or ConvertGuidToLongUsingBase64

            //PaymentData paymentData = new PaymentData(
            //    orderCodeLong,
            //    (int)booking.TotalPrice,
            //    content,
            //    items,
            //    $"{_websiteDomain}/api/payment/cancel",
            //    $"{_websiteDomain}/api/payment/return",
            //    null,
            //    user.FullName,
            //    user.Email,
            //    user.PhoneNumber,
            //    null,
            //    expiredAt
            //);

            //var createPaymentData = await _payOS.createPaymentLink(paymentData);
            //if (createPaymentData == null || string.IsNullOrEmpty(createPaymentData.checkoutUrl))
            //{
            //    throw new Exception("Failed to create payment link");
            //}
            //PayOsCreatePaymentResult createPaymentResult = new PayOsCreatePaymentResult
            //{
            //    AccountNumber = createPaymentData.accountNumber,
            //    Amount = createPaymentData.amount,
            //    Bin = createPaymentData.bin,
            //    CheckoutUrl = createPaymentData.checkoutUrl,
            //    Currency = createPaymentData.currency,
            //    Description = createPaymentData.description,
            //    ExpiredAt = createPaymentData.expiredAt,
            //    OrderCode = createPaymentData.orderCode,
            //    PaymentLinkId = createPaymentData.paymentLinkId,
            //    QrCode = createPaymentData.qrCode,
            //    Status = createPaymentData.status,

            //};
            //PaymentResultResponseDTO paymentResultResponseDTO = new PaymentResultResponseDTO 
            //{ 
            //    Code = "00",
            //    Desc = "Success - Thành công",
            //    CreatedPaymentResult = createPaymentResult,
            //    Signature = paymentData.signature,
            //};
            //return paymentResultResponseDTO;
            throw new Exception("Failed to create payment link");
        }
        public async Task<PaymentLinkInformation> GetPaymentStatus(string disbursementId)
        {
            //var booking = await _bookingService.GetBookingById(bookingId);
            //if (booking == null)
            //{
            //    throw new NotFoundException("Booking not found");
            //}

            //long orderCodeLong = booking.BookingCode;

            //var paymentStatus = await _payOS.getPaymentLinkInformation(orderCodeLong);

            //return paymentStatus;
            throw new NotFoundException("Transaction not found");
        }

    }
}
