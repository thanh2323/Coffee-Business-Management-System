using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static QRCoder.PayloadGenerator;

namespace CoffeeShop.Application.Service.Gateways
{
    public class VNPayGateway : IPaymentGateway
    {
        private readonly IConfiguration _configuration;
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _baseUrl;
        private readonly string _returnUrl;


        public VNPayGateway(IConfiguration configuration)
        {
            _configuration = configuration;
            _tmnCode = _configuration["VnPay:vnp_TmnCode"] ?? "";
            _hashSecret = _configuration["VnPay:vnp_HashSecret"] ?? "";
            _baseUrl = _configuration["VnPay:vnp_Url"] ?? "";
            _returnUrl = _configuration["VnPay:vnp_ReturnUrl"] ?? "";

        }

        public PaymentGateway Gateway => PaymentGateway.VNPay;

        public async Task<PaymentLinkResult> CreatePaymentLinkAsync(int branchId, decimal amount, string description, string reference)
        {
            try
            {

                var createDate = DateTime.UtcNow.AddHours(7); // GMT+7
                var expireDate = createDate.AddMinutes(15); // Link valid for 15 minutes
                var amountInVnd = (long)(amount * 100); // Convert to VND (multiply by 100)

                var vnpayParams = new SortedDictionary<string, string>
                {
                    {"vnp_Command", "pay"},
                    {"vnp_TmnCode", _tmnCode},
                    {"vnp_Amount", amountInVnd.ToString()},
                    {"vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss")},
                    {"vnp_CurrCode", "VND"},
                    {"vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss")},
                    {"vnp_IpAddr", "127.0.0.1"}, // TODO: Get real IP
                    {"vnp_Locale", "vn"},
                    {"vnp_OrderInfo", $"Thanh toan don hang:{description}"},
                    {"vnp_OrderType", "other"},
                    {"vnp_ReturnUrl", _returnUrl},
                    {"vnp_TxnRef", reference},
                    {"vnp_Version", "2.1.0"}
                };

                // Create secure hash
                var signData = string.Join("&", vnpayParams.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));
                var secureHash = CreateSecureHash(_hashSecret, signData);

                //URL
                var query = QueryHelpers.AddQueryString(_baseUrl, vnpayParams);
                var paymentUrl = $"{query}&vnp_SecureHash={secureHash}";




                return await Task.FromResult(PaymentLinkResult.Success(paymentUrl, reference));
            }
            catch (Exception ex)
            {
                return PaymentLinkResult.Failed($"Failed to create VNPay link: {ex.Message}");
            }
        }

        public async Task<bool> VerifyPaymentAsync(IQueryCollection queryParams)
        {
            try
            {
                var receivedSecureHash = queryParams["vnp_SecureHash"];
                var sortedParams = new SortedDictionary<string, string>();

                foreach (var key in queryParams.Keys)
                {
                    if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                    {
                        sortedParams.Add(key, queryParams[key]!);
                    }
                }

       
                var signData = string.Join("&", sortedParams.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));
                var secureHash = CreateSecureHash(_hashSecret, signData);

                if (!secureHash.Equals(receivedSecureHash, StringComparison.OrdinalIgnoreCase))
                    return await Task.FromResult(false);

                var responseCode = queryParams["vnp_ResponseCode"];
                
                return await Task.FromResult(responseCode == "00"); // 00 = success
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        private static string CreateSecureHash(string key, string input)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);
            using var hmac = new HMACSHA512(keyBytes);
            var hashValue = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }
    }
}


