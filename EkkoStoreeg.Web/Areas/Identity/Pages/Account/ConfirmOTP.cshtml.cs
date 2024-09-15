using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

namespace EkkoSoreeg.Web.Areas.Identity.Pages.Account
{
    public class ConfirmOTPModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ConfirmOTPModel> _logger;
        private readonly IOtpService _otpService;
        private readonly ISmsService _smsSender;
        private readonly ApplicationDbContext _context;


        public ConfirmOTPModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<ConfirmOTPModel> logger,
            IOtpService otpService,ISmsService smsService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _otpService = otpService;
            _smsSender = smsService;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string EmailOrPhone { get; set; }
            public string OTP { get; set; }
        }

        public IActionResult OnGet(string emailOrPhone, string returnUrl = null)
        {
            Input = new InputModel
            {
                EmailOrPhone = emailOrPhone
            };
            ViewData["ReturnUrl"] = returnUrl;
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
            var isPhone = Regex.IsMatch(Input.EmailOrPhone, @"^(?:\+20|0)?1[0125]\d{8}$");
            var user = await _context.TbapplicationUser
                .FirstOrDefaultAsync(x =>
                    (!isPhone && x.UserName == Input.EmailOrPhone) ||
                    (isPhone && x.UserName == Input.EmailOrPhone));

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            if (!isPhone)
            {
                // Confirm the email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User confirmed their email.");
                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);

                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }
            else
            {
                // Confirm the phone number
                user.PhoneNumberConfirmed = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User confirmed their phone number.");
                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

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
                _otpService.StoreOTP(emailOrPhone, otp, TimeSpan.FromMinutes(1));

                if (!isPhone)
                {
                    await _emailSender.SendEmailAsync(emailOrPhone, "OTP Verification", $"<h1>{otp}</h1>");
                }
                else
                {
                    var smsResult = _smsSender.SendTwilioSMSAsync(emailOrPhone, $"Your OTP is: {otp}");

                    if (smsResult == null)
                    {
                        await _userManager.DeleteAsync(user);
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
