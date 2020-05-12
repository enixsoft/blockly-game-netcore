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
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
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
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly DataManager _dataManager;


        public HomeController(
            ILogger<HomeController> logger, 
            ApplicationDbContext dbContext, 
            IAntiforgery antiforgery, UserManager<ApplicationUser> userManager, 
            IStringLocalizer<SharedResource> localizer,
            IOptions<ApplicationSettings> appSettings,
            IWebHostEnvironment hostingEnvironment,
            DataManager dataManager
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _antiforgery = antiforgery;
            _userManager = userManager;
            _localizer = localizer;
            _appSettings = appSettings;
            _hostingEnvironment = hostingEnvironment;
            _dataManager = dataManager;
        }
  
        public async Task<IActionResult> Index()
        {
            IndexModel indexModel = await _dataManager.CreateIndexModel(
                 _antiforgery.GetAndStoreTokens(HttpContext).RequestToken,
                 Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name,
                 await _userManager.GetUserAsync(HttpContext.User)
                );

            if (TempData.ContainsKey("errors"))
            {
                indexModel.Errors = TempData["errors"].ToString();
            }

            if (TempData.ContainsKey("old"))
            {
                indexModel.Old = TempData["old"].ToString();
            }

            return View("Index", indexModel);
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
