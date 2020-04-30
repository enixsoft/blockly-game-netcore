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
//using BlocklyGame.Models;

namespace BlocklyGame.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAntiforgery _antiforgery;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IAntiforgery antiforgery, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _dbContext = dbContext;
            _antiforgery = antiforgery;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CsrfToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken; 
                                
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                List<string> roles = new List<string>(await _userManager.GetRolesAsync(user));
                ViewBag.User = JsonConvert.SerializeObject(new { username = user.UserName, email = user.Email, role = roles.Contains("Administrator") ? "Administrator" : "User" });
            }
            else
            {
                ViewBag.User = JsonConvert.SerializeObject(null);
            }      

            return View();
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
