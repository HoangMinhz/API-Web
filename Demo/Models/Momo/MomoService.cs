using Demo.Models.ViewModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
namespace Demo.Models.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        private readonly ILogger<MomoService> _logger;

        public MomoService(IOptions<MomoOptionModel> options, ILogger<MomoService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
        {
            try
            {
                var option = _options.Value;
                model.OrderInfo = string.IsNullOrEmpty(model.OrderInfo) ? "Thanh toán đơn hàng" : model.OrderInfo;
                model.ReturnUrl = string.IsNullOrEmpty(model.ReturnUrl) ? option.ReturnUrl : model.ReturnUrl;
                model.NotifyUrl = string.IsNullOrEmpty(model.NotifyUrl) ? option.NotifyUrl : model.NotifyUrl;

                var requestId = Guid.NewGuid().ToString();
                var orderId = model.OrderId.ToString();
                var amount = model.Amount.ToString("0");
                var orderInfo = model.OrderInfo;
                var returnUrl = model.ReturnUrl;
                var notifyUrl = model.NotifyUrl;
                var extraData = "";

                var rawSignature = $"accessKey={option.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={option.PartnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={option.RequestType}";

                _logger.LogInformation("Raw signature: {RawSignature}", rawSignature);

                var signature = ComputeHmacSha256(rawSignature, option.SecretKey);

                var requestData = new
                {
                    partnerCode = option.PartnerCode,
                    partnerName = "Test",
                    storeId = "MomoTestStore",
                    requestId = requestId,
                    amount = amount,
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = returnUrl,
                    ipnUrl = notifyUrl,
                    lang = "vi",
                    extraData = extraData,
                    requestType = option.RequestType,
                    signature = signature
                };

                var jsonRequest = JsonConvert.SerializeObject(requestData);
                _logger.LogInformation("Request to MoMo: {JsonRequest}", jsonRequest);

                using var httpClient = new HttpClient();
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(option.MomoApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Response from MoMo: {ResponseContent}", responseContent);

                var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(responseContent);
                return momoResponse ?? new MomoCreatePaymentResponseModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                return new MomoCreatePaymentResponseModel
                {
                    ResultCode = "99",
                    Message = "Có lỗi xảy ra trong quá trình xử lý"
                };
            }
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection["amount"].ToString();
            var orderInfo = collection["orderInfo"].ToString();
            var orderId = collection["orderId"].ToString();
            var partnerCode = collection["partnerCode"].ToString();
            var requestId = collection["requestId"].ToString();
            var responseTime = collection["responseTime"].ToString();
            var resultCode = collection["resultCode"].ToString();
            var message = collection["message"].ToString();
            var payType = collection["payType"].ToString();
            var transId = collection["transId"].ToString();
            var signature = collection["signature"].ToString();

            return new MomoExecuteResponseModel
            {
                Amount = amount,
                OrderInfo = orderInfo,
                OrderId = orderId,
                PartnerCode = partnerCode,
                RequestId = requestId,
                ResponseTime = responseTime,
                ResultCode = resultCode,
                Message = message,
                PayType = payType,
                TransId = transId,
                Signature = signature
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
} 