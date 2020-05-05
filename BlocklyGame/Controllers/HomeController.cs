using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BlocklyGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.Extensions.Localization;
using BlocklyGame.Managers;
using Localization;
using BlocklyGame.Helpers;
using Microsoft.Extensions.Options;
//using BlocklyGame.Models;

namespace BlocklyGame.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAntiforgery _antiforgery;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer _localizer;
        private readonly IOptions<ApplicationSettings> _appSettings;

        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext dbContext, 
            IAntiforgery antiforgery, UserManager<ApplicationUser> userManager, 
            IStringLocalizer<SharedResource> localizer,
            IOptions<ApplicationSettings> appSettings)
        {
            _logger = logger;
            _dbContext = dbContext;
            _antiforgery = antiforgery;
            _userManager = userManager;
            _localizer = localizer;
            _appSettings = appSettings;
        }
  
        public async Task<IActionResult> Index()
        {
            IndexModel indexModel = new IndexModel
            {
                CsrfToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken
            }; 

            indexModel.Cookies.Add("msg", _localizer["cookies.msg"]);
            indexModel.Cookies.Add("dismiss", _localizer["cookies.dismiss"]);
            indexModel.Cookies.Add("link", _localizer["cookies.link"]);
            indexModel.Title = _localizer["title"];
            indexModel.RecaptchaKey = _appSettings.Value.GOOGLE_RECAPTCHA_KEY;

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                List<string> roles = new List<string>(await _userManager.GetRolesAsync(user));
                indexModel.User = JsonConvert.SerializeObject(new { username = user.UserName, email = user.Email, role = roles.Contains("Administrator") ? "admin" : "user" });
            }  

            if (TempData.ContainsKey("errors"))
            {
                indexModel.Errors = TempData["errors"].ToString();
            }

            if (TempData.ContainsKey("old"))
            {
                indexModel.Old = TempData["Old"].ToString();
            }


            return View(indexModel);
        }

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
