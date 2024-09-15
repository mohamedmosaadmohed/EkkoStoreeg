using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace EkkoSoreeg.Utilities.SMS
{
    public interface ISmsService
    {
        Task<bool> SendOTPAsync(string phoneNumber, string otp);
        MessageResource SendTwilioSMSAsync(string to, string body);
    }
}
