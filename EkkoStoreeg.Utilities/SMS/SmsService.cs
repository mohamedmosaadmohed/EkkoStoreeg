using EkkoSoreeg.Utilities.Validation;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;

namespace EkkoSoreeg.Utilities.SMS
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _client;
        private readonly TwilioSettings _twilio;

        public SmsService(HttpClient client,IOptions<TwilioSettings> twilio)
        {
            _client = client;
            _twilio = twilio.Value ?? throw new InvalidCastException("Twilio AccountSID and AuthToken must be provided.");
        }

        // SMS MISR
        public async Task<bool> SendOTPAsync(string toPhone, string otp)
        {
            try
            {
                string url = $"{SD.Url}?environment=2&username={SD.UserName}&password={SD.Password}&sender={SD.SenderIDs}" +
                             $"&mobile={toPhone}&template={SD.Templete}&otp={otp}";

                HttpResponseMessage response = await _client.PostAsync(url, null);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrEmpty(responseContent);
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        // Twilio
        public MessageResource SendTwilioSMSAsync(string to, string body)
        {
            if (!to.StartsWith("+20"))
                  to = "+20" + to.TrimStart('0');
            try
            {
                TwilioClient.Init(_twilio.AccountSID, _twilio.AuthToken);
                var result = MessageResource.Create(
                body: body,
                from: new Twilio.Types.PhoneNumber(_twilio.PhoneNumber),
                to: new Twilio.Types.PhoneNumber(to)
                );
                return result;
            }
            catch (ApiException)
            {
                return null;
            }

        }
    }
}
