using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Utilities.Validation;
using System.Text.RegularExpressions;
using EkkoSoreeg.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace EkkoSoreeg.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public class InputModel
        {
            [Required]
            [EmailOrPhone]
            public string EmailOrPhone { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var isPhone = Regex.IsMatch(Input.EmailOrPhone, @"^(?:\+20|0)?1[0125]\d{8}$");

                ApplicationUser user;
                if (isPhone)
                {
                    user = await _context.TbapplicationUser.FirstOrDefaultAsync(u => u.PhoneNumber == Input.EmailOrPhone);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Phone number does not exist.");
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
                }
                if (isPhone)
                {
                    if (!user.PhoneNumberConfirmed)
                    {
                        ModelState.AddModelError(string.Empty,"Invalid Phone Number or password");
                        return Page();
                    }

                }
                else
                {
                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty,"Invalid Email or password");
                        return Page();
                    }
                }
                var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, isPhone ? "Invalid Phone Number or password" : "Invalid Email or password");
                    return Page();
                }
            }

            return Page();
        }
    }
}
