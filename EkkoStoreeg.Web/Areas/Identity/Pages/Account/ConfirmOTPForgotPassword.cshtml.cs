using EkkoSoreeg.DataAccess.Data;
using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Utilities.OTP;
using EkkoSoreeg.Utilities.SMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace EkkoSoreeg.Web.Areas.Identity.Pages.Account
{
    public class ConfirmOTPForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly ISmsService _smsService;

        public ConfirmOTPForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService,IEmailSender emailSender, ApplicationDbContext context, ISmsService smsService)
        {
            _userManager = userManager;
            _otpService = otpService;
            _emailSender = emailSender;
            _context = context;
            _smsService = smsService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string EmailOrPhone { get; set; }
            public string OTP { get; set; }
        }

        public IActionResult OnGet(string emailOrPhone)
        {
            Input = new InputModel
            {
                EmailOrPhone = emailOrPhone
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Verify the OTP
            var isValid = _otpService.VerifyOTP(Input.EmailOrPhone, Input.OTP);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid OTP.");
                return Page();
            }

            var user = await _context.TbapplicationUser
                        .FirstOrDefaultAsync(x => x.UserName == Input.EmailOrPhone);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            // Generate password reset token
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Redirect to password reset page with the token and email
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code, emailorphone = Input.EmailOrPhone },
                protocol: Request.Scheme);

            return Redirect(callbackUrl);
        }
        public async Task<IActionResult> OnPostResendOTPAsync([FromBody] string emailOrPhone)
        {
            try
            {
                var isPhone = Regex.IsMatch(emailOrPhone, @"^(?:\+20|0)?1[0125]\d{8}$");

                var user = await _context.TbapplicationUser
                    .FirstOrDefaultAsync(x =>
                        (!isPhone && x.Email == emailOrPhone) ||
                        (isPhone && x.PhoneNumber == emailOrPhone));

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "User not found." });
                }

                var otp = _otpService.GenerateOTP();
                _otpService.StoreOTP(emailOrPhone, otp, TimeSpan.FromSeconds(45));

                if (!isPhone)
                {
                    await _emailSender.SendEmailAsync(emailOrPhone, "Rest code", $"<h1>{otp}</h1>");
                }
                else
                {
                    var smsResult = _smsService.SendTwilioSMSAsync(emailOrPhone, $"Rest code is: {otp}");

                    if (smsResult == null)
                    {
                        return BadRequest(new { success = false, message = "Failed to send SMS. Please try again later." });
                    }
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while sending OTP. Please try again later.", error = ex.Message });
            }
        }
    }

}
