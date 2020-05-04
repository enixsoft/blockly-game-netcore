using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BlocklyGame.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Localization;
using Localization;

namespace BlocklyGame.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            //IEmailSender emailSender,
            ILogger<AccountController> logger,
            IStringLocalizer<SharedResource> localizer
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            _logger = logger;
            _localizer = localizer;
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("/login")]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
                else
                {
                    TempData["errors"] = JsonConvert.SerializeObject(new Dictionary<string, List<string>>() { { "username", new List<string> () { _localizer["These credentials do not match our records."] } } });
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
            }

            TempData["errors"] = JsonConvert.SerializeObject(new Dictionary<string, List<string>>() { { "username", new List<string>() { _localizer["These credentials do not match our records."] } } });
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("/register")]
        public async Task<IActionResult> Register(RegisterModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    ////var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    ////await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
                //AddErrors(result);
            }

            // If execution got this far, something failed, redisplay the form.
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }        

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}