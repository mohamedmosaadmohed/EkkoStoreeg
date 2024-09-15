using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Utilities;
using EkkoSoreeg.Utilities.OTP;
using EkkoSoreeg.Utilities.SMS;
using EkkoSoreeg.Utilities.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace EkkoSoreeg.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOtpService _otpService;
        private readonly ISmsService _smsSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,IOtpService otpService,ISmsService smsService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _otpService = otpService;
            _smsSender = smsService;
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public class InputModel
        {
            [Required]
            [MaxLength(450)]
            public string Name { get; set; }

            [Display(Name = "Email or Phone")]
            [EmailOrPhone]
            public string EmailOrPhone { get; set; }

            [Required]
            [MinLength(6, ErrorMessage = "The Password must be at least 6 characters long.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            // Determine if the input is a phone number
            var isPhone = Regex.IsMatch(Input.EmailOrPhone, @"^(?:\+20|0)?1[0125]\d{8}$");
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                if (!isPhone)
                {
                    await _userStore.SetUserNameAsync(user, Input.EmailOrPhone, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, Input.EmailOrPhone, CancellationToken.None);
                }
                else
                {
                    await _userStore.SetUserNameAsync(user, Input.EmailOrPhone, CancellationToken.None);
                    user.PhoneNumber = Input.EmailOrPhone;
                }
                user.Name = Input.Name;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Assign role to the user
                    string role = HttpContext.Request.Form["Rolebtn"].ToString();
                    if (string.IsNullOrEmpty(role))
                    {
                        await _userManager.AddToRoleAsync(user, SD.CustomerRole);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, role);

                        if(isPhone)
                           user.PhoneNumberConfirmed = true;
                        else
                            user.EmailConfirmed = true;

                        return RedirectToAction("Index", "Users", new { area = "Admin" });
                    }
                    if (isPhone)
                    {
                        var otp = _otpService.GenerateOTP();
                        _otpService.StoreOTP(Input.EmailOrPhone, otp, TimeSpan.FromSeconds(45));
                        var body = $" Your Ekko Store Verification code is {otp}";
                        //await _smsSender.SendOTPAsync(Input.EmailOrPhone, body);
                       var twilioResult = _smsSender.SendTwilioSMSAsync(Input.EmailOrPhone, body);
                        if (twilioResult == null)
                        {
                            ModelState.AddModelError(string.Empty, "Unable to Send SMS to This Number, enter valid number");
                            await _userManager.DeleteAsync(user);
                            return Page();
                        }
                        return RedirectToPage("ConfirmOTP", new { emailOrPhone = Input.EmailOrPhone, returnUrl = returnUrl });
                    }
                    else
                    {
                        var otp = _otpService.GenerateOTP();
                        _otpService.StoreOTP(Input.EmailOrPhone, otp, TimeSpan.FromSeconds(45));
                        await _emailSender.SendEmailAsync(Input.EmailOrPhone, "OTP Verification", $"<h1>{otp}</h1>");
                        return RedirectToPage("ConfirmOTP", new { emailOrPhone = Input.EmailOrPhone, returnUrl = returnUrl });
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
