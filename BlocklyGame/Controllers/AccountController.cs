using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BlocklyGame.Models;
using Microsoft.Extensions.Localization;
using Localization;
using BlocklyGame.Helpers;
using System.Net.Http;
using System.Text.Json;

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
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly IHttpClientFactory _clientFactory;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IStringLocalizer<SharedResource> localizer,
            IOptions<ApplicationSettings> appSettings,
            IHttpClientFactory clientFactory
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
            _appSettings = appSettings;
            _clientFactory = clientFactory;
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
                    TempData["errors"] = JsonSerializer.Serialize(new Dictionary<string, List<string>>() { { "username", new List<string> () { _localizer["These credentials do not match our records."] } } });
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
            }

            TempData["errors"] = JsonSerializer.Serialize(new Dictionary<string, List<string>>() { { "username", new List<string>() { _localizer["These credentials do not match our records."] } } });
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("/register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if(!String.IsNullOrEmpty(_appSettings.Value.GOOGLE_RECAPTCHA_KEY) && !String.IsNullOrEmpty(_appSettings.Value.GOOGLE_RECAPTCHA_SECRET))
                {
                    if (await ValidateRecaptchaAsync(model.RecaptchaResponse) == false)
                    {
                        this.ModelState.AddModelError("g-recaptcha-response", _localizer["Please ensure that you are a human!"]);

                        TempData["errors"] = GetErrors(ModelState
                            .Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        ));

                        TempData["old"] = GetOldInputs(ModelState
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.RawValue?.ToString()
                        ));
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }                   
                }   

                ApplicationUser user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                foreach (var error in result.Errors)
                {
                    string[] switchStrings = { "username", "email", "password" };
                    switch (switchStrings.FirstOrDefault<string>(s => error.Code.ToLower().Contains(s)))
                    {
                        case
                            "username":
                            ModelState.AddModelError("register-username", error.Description);
                            break;
                        case
                            "email":
                            ModelState.AddModelError("register-email", error.Description);
                            break;
                        case
                            "password":
                            ModelState.AddModelError("register-password", error.Description);
                            break;
                        default:
                            break;
                    }
                }
                
            }

            TempData["errors"] = GetErrors(ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
            ));
            TempData["old"] = GetOldInputs(ModelState
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.RawValue?.ToString()
            ));

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Route("/registeruserbyadmin")]
        public async Task<IActionResult> RegisterUserByAdmin(UserRegisteredByAdmin userRegisteredByAdmin)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            List<string> roles = new List<string>(await _userManager.GetRolesAsync(user));
            if (roles.Contains("Administrator"))
            {
                try
                {
                    IdentityResult result = await _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = userRegisteredByAdmin.username,
                        Email = userRegisteredByAdmin.username + "@blocklygame.com"
                    }, userRegisteredByAdmin.password);

                    if (result.Succeeded)
                    {
                        return Ok();
                    }
                }
                catch
                {
                    return BadRequest();
                }

            }
            return BadRequest();
        }

        public class UserRegisteredByAdmin
        {
            public string username { get; set; }

            public string password { get; set; }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private string GetErrors(Dictionary<string, List<string>> errorList)
        {
            if(errorList.ContainsKey("register-password_confirmation"))
            {
                if (!errorList.ContainsKey("register-password"))
                {
                    errorList["register-password"] = new List<string>();
                }
                errorList["register-password"].AddRange(errorList["register-password_confirmation"]);
            }

            return JsonSerializer.Serialize(errorList);
        }

        private string GetOldInputs(Dictionary<string, string> oldInputs)
        {
            string[] filteredFields = { "register-password", "register-password_confirmation"};
            oldInputs = oldInputs.Where(kvp => !filteredFields.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return JsonSerializer.Serialize(oldInputs);
        }

        private async Task<bool> ValidateRecaptchaAsync(string recaptchaResponse)
        {
            if (String.IsNullOrEmpty(recaptchaResponse))
            {
                return false;
            }

            HttpClient httpClient = _clientFactory.CreateClient();
            try
            {
                var formContent = new FormUrlEncodedContent(new[]
                {   
                        new KeyValuePair<string, string>("secret", _appSettings.Value.GOOGLE_RECAPTCHA_SECRET),
                        new KeyValuePair<string, string>("response", recaptchaResponse)
                });


                HttpResponseMessage response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", formContent);
                response.EnsureSuccessStatusCode();
                string apiResponse = await response.Content.ReadAsStringAsync();

                JsonDocument apiJson = JsonDocument.Parse(apiResponse);            
                return apiJson.RootElement.GetProperty("success").GetBoolean();
            }
            catch (HttpRequestException ex)
            {
                // Something went wrong with the API.          
                _logger.LogError(ex, "Unexpected error calling reCAPTCHA api.");
                return false;
            }
        }

    }
}