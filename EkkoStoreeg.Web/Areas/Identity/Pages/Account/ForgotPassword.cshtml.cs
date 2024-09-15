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
				var isPhone = Regex.IsMatch(Input.EmailOrPhone, @"^(?:\+20|0)?1[0125]\d{8}$");

				// Find the user by email or phone
				var user = await _userManager.FindByNameAsync(Input.EmailOrPhone);
				if (user == null)
				{
					if (isPhone)
					{
						ModelState.AddModelError(string.Empty,"Phone number does not exist");
					}
					else
					{
						ModelState.AddModelError(string.Empty,"Email does not exist");
					}
					return Page();
				}

				// Generate password reset token
				var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

				var callbackUrl = Url.Page(
					"/Account/ResetPassword",
					pageHandler: null,
					values: new { area = "Identity", code, emailorphone = Input.EmailOrPhone },
					protocol: Request.Scheme);

				return Redirect(callbackUrl);
			}

			return Page();
		}
	}
}

