using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EkkoSoreeg.DataAccess.Data;
using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Utilities.OTP;
using EkkoSoreeg.Utilities.SMS;
using EkkoSoreeg.Utilities.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace EkkoSoreeg.Web.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IOtpService _otpService;
        private readonly ApplicationDbContext _context;
        private readonly ISmsService _smsSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager,
            IEmailSender emailSender, IOtpService otpService, ApplicationDbContext context, ISmsService smsSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _otpService  = otpService;
            _context = context;
            _smsSender = smsSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Email or Phone")]
            [EmailOrPhone]
            public string EmailOrPhone { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Determine if the input is a phone number
                var isPhone = Regex.IsMatch(Input.EmailOrPhone, @"^(?:\+20|0)?1[0125]\d{8}$");
                ApplicationUser user;
                if (isPhone)
                {
                    user = await _context.TbapplicationUser.FirstOrDefaultAsync(u => u.UserName == Input.EmailOrPhone);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Phone number does not exist.");
                        return Page();
                    }
                    if (user == null || !user.PhoneNumberConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "The phone number does not exist or is not confirmed.");
                        return Page();
                    }
                }
                else
                {
                    user = await _context.TbapplicationUser.FirstOrDefaultAsync(u => u.Email == Input.EmailOrPhone);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Email does not exist.");
                        return Page();
                    }
                    if (user == null || !user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "The Email address does not exist or is not confirmed.");
                        return Page();
                    }
                }
                if (isPhone)
                {
                    var otp = _otpService.GenerateOTP();
                    _otpService.StoreOTP(Input.EmailOrPhone, otp, TimeSpan.FromSeconds(45));
                    var body = $"Rest code is {otp}";
                    //await _smsSender.SendOTPAsync(Input.EmailOrPhone, body);
                    var twilioResult = _smsSender.SendTwilioSMSAsync(Input.EmailOrPhone, body);
                    if (twilioResult == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to Send SMS to This Number");
                        await _userManager.DeleteAsync(user);
                        return Page();
                    }
                    return RedirectToPage("ConfirmOTPForgotPassword", new { emailOrPhone = Input.EmailOrPhone});
                }
                else
                {
                    var otp = _otpService.GenerateOTP();
                    _otpService.StoreOTP(Input.EmailOrPhone, otp, TimeSpan.FromMinutes(1));
                    await _emailSender.SendEmailAsync(Input.EmailOrPhone, "Rest code", $"<h1>{otp}</h1>");
                    return RedirectToPage("ConfirmOTPForgotPassword", new { emailOrPhone = Input.EmailOrPhone});
                }
            }
            return Page();
        }
    }
}

