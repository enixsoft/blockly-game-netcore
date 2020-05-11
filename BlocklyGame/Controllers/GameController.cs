using BlocklyGame.Helpers;
using BlocklyGame.Models;
using BlocklyGame.Models.Game;
using Localization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace BlocklyGame.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GameController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAntiforgery _antiforgery;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer _localizer;
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static readonly int[] categoryHasLevelsArray = { 6, 6 };
        private static readonly int categoryMin = 1;
        private static readonly int categoryMax = categoryHasLevelsArray.Length;
        private static readonly int levelMin = 1;

        public GameController(
            ILogger<HomeController> logger,
            ApplicationDbContext dbContext,
            IAntiforgery antiforgery, UserManager<ApplicationUser> userManager,
            IStringLocalizer<SharedResource> localizer,
            IOptions<ApplicationSettings> appSettings,
            IWebHostEnvironment hostingEnvironment
            )
        {
            _logger = logger;
            _antiforgery = antiforgery;
            _userManager = userManager;
            _localizer = localizer;
            _appSettings = appSettings;
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
        }

        [HttpGet("{category}/{level}")]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int category, int level)
        {
            if (!ValidateCategoryAndLevel(category, level))
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            if (category < categoryMax && level == categoryHasLevelsArray[category - 1])
            {
                return RedirectToAction(nameof(GameController.Index), "Game", new { category = (category + 1).ToString(), level = levelMin.ToString() });
            }

            bool requiresJsonResponse = HttpContext.Request.GetTypedHeaders()
                                                   .Accept.Any(t => t.Suffix.Value?.ToUpper() == "JSON"
                                                   || t.SubTypeWithoutSuffix.Value?.ToUpper() == "JSON");

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            GameDataModel result = await RunGame(category, level, user);

            if (requiresJsonResponse)
            {
                return Json(result);
            }

            if(result == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            IndexModel indexModel = new IndexModel
            {
                CsrfToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken
            };

            string lang = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            indexModel.Lang = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\locales\\{lang}.json"));

            indexModel.Cookies.Add("msg", _localizer["cookies.msg"]);
            indexModel.Cookies.Add("dismiss", _localizer["cookies.dismiss"]);
            indexModel.Cookies.Add("link", _localizer["cookies.link"]);
            indexModel.Title = _localizer["title"];
            indexModel.RecaptchaKey = _appSettings.Value.GOOGLE_RECAPTCHA_KEY;
            List<string> roles = new List<string>(await _userManager.GetRolesAsync(user));
            indexModel.User = JsonSerializer.Serialize(new { username = user.UserName, email = user.Email, role = roles.Contains("Administrator") ? "admin" : "user" });
            indexModel.GameData = JsonSerializer.Serialize(result);

            return View("Index", indexModel);
        }

        [HttpGet("/play")]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> StartNewGameOrContinue()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            
            Progress inGameProgress = _dbContext.Progress
                                      .Where(p => p.UserId == user.Id)
                                      .FirstOrDefault();

            if (inGameProgress == null)
            {
                return RedirectToAction(nameof(GameController.Index), "Game", new { category = "1", level = "1" });
            }
            else if (inGameProgress.Percentage == 100)
            {
                return RedirectToAction(nameof(GameController.Index), "Game", new { category = inGameProgress.Category.ToString(), level = (inGameProgress.Level + 1).ToString() });    
            }
            else
            {
                return RedirectToAction(nameof(GameController.Index), "Game", new { category = inGameProgress.Category.ToString(), level = inGameProgress.Level.ToString() });
            }
        }

        [HttpGet("/start/{category}/{level}")]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> StartLevelAsNew(int category, int level)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (!ValidateCategoryAndLevel(category, level))
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            Progress inGameProgress = _dbContext.Progress
                                      .Where(p => p.UserId == user.Id && p.Category == category)
                                      .FirstOrDefault();

            if (inGameProgress == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else if (inGameProgress.Level > level || (inGameProgress.Level == level && inGameProgress.Percentage == 100))
            {
                string jsonStartGame = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\start{category}x{level}.json"));

                SavedGame savedGame = _dbContext.SavedGames
                .Where(s => s.UserId == user.Id && s.Category == category && s.Level == level && s.Progress == 0)            
                .FirstOrDefault();

                if (savedGame != null)
                {
                    savedGame.Progress = 0;                    
                }
                else
                {
                    savedGame = new SavedGame()
                    {
                        UserId = user.Id,
                        Category = category,
                        Level = level,
                        Progress = 0,
                        Json = jsonStartGame
                    };
                    _dbContext.Add(savedGame);
                }
                _dbContext.SaveChanges();
            }
            return RedirectToAction(nameof(GameController.Index), "Game", new { category, level });
        }

        [HttpGet("/continue/{category}/{level}")]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ContinueLevel(int category, int level)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (!ValidateCategoryAndLevel(category, level))
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else
            {
                Progress inGameProgress = _dbContext.Progress
                              .Where(p => p.UserId == user.Id && p.Category == category)
                              .FirstOrDefault();

                if (inGameProgress != null && inGameProgress.Level >= level)
                {
                    return RedirectToAction(nameof(GameController.Index), "Game", new { category, level });
                }
                else return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private bool ValidateCategoryAndLevel(int category, int level)
        {
            int levelMax = 0;

            if (category <= categoryMax && category >= categoryMin)
            {
                levelMax = categoryHasLevelsArray[category - 1];
            }

            if (category > categoryMax || category < categoryMin || level < levelMin || level > levelMax)
            {
                return false;
            }
            else if (category == categoryMax && level == levelMax)
            {
                return false;
            }
            return true;
        }

        private async Task<GameDataModel> RunGame(int category, int level, ApplicationUser user)
        {
            Progress inGameProgress = _dbContext.Progress
                                      .Where(p => p.UserId == user.Id && p.Category == category)
                                      .FirstOrDefault();

            string jsonStartGame = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\start{category}x{level}.json"));
            SavedGame savedGame = null;

            if (inGameProgress == null) //game was not played at all or requested category was not played yet            
            {
                if (category == 1)
                {
                    //no ingame progress in category 1 exists yet for this user, set level 1 with 0 progress

                    _dbContext.Progress.Add(new Progress()
                    {
                        UserId = user.Id,
                        Category = category,
                        Level = level,
                        Percentage = 0
                    });

                    //no saved game in this category and level exists yet for this user, create first saved game with 0 progress from startgame json

                    savedGame = new SavedGame()
                    {
                        UserId = user.Id,
                        Category = category,
                        Level = level,
                        Progress = 0,
                        Json = jsonStartGame
                    };

                    _dbContext.SavedGames.Add(savedGame);

                    _dbContext.SaveChanges();
                }

                else
                {
                    //check if player has 100 progress in previous max level of previous category
                    Progress inGameProgressOfPreviousLevelOfPreviousCategory = _dbContext.Progress
                                      .Where(p => p.UserId == user.Id && (p.Category == category - 1) && (level == categoryHasLevelsArray[category - 1] - 1))
                                      .FirstOrDefault();

                    if (inGameProgressOfPreviousLevelOfPreviousCategory == null || inGameProgressOfPreviousLevelOfPreviousCategory.Percentage != 100)
                    {
                        return null;
                    }
                    else
                    {
                        if (level == 1)
                        {
                            _dbContext.Progress.Add(new Progress()
                            {
                                UserId = user.Id,
                                Category = category,
                                Level = level,
                                Percentage = 0
                            });

                            savedGame = new SavedGame()
                            {
                                UserId = user.Id,
                                Category = category,
                                Level = level,
                                Progress = 0,
                                Json = jsonStartGame
                            };

                            _dbContext.SavedGames.Add(savedGame);

                            _dbContext.SaveChanges();

                        }
                        else
                        {
                            return null;
                        }

                    }

                }

            }

            else
            //progress for requested category exists            
            {
                if (inGameProgress.Level > level)
                {
                    //if player has progress beyond the requested level
                    savedGame = _dbContext.SavedGames
                                      .Where(s => s.UserId == user.Id && s.Category == category && s.Level == level)
                                      .OrderByDescending(p => p.UpdatedDate)
                                      .FirstOrDefault();
                }
                else if (inGameProgress.Level == level)
                {
                    //if player has progress on the par with requested level, has not completed it yet
                    if (inGameProgress.Percentage != 100)
                    {

                        savedGame = _dbContext.SavedGames
                                      .Where(s => s.UserId == user.Id && s.Category == category && s.Level == level && s.Progress == inGameProgress.Percentage)
                                      .OrderByDescending(p => p.UpdatedDate)
                                      .FirstOrDefault();

                        //if player due to error does not have the savedgame with progress he has, he gets the latest savedgame
                        if (savedGame == null)
                        {
                            savedGame = _dbContext.SavedGames
                                      .Where(s => s.UserId == user.Id && s.Category == category && s.Level == level)
                                      .OrderByDescending(p => p.UpdatedDate)
                                      .FirstOrDefault();
                        }

                    }

                    //player has completed the level, but requests it again
                    else
                    {

                        savedGame = _dbContext.SavedGames
                                     .Where(s => s.UserId == user.Id && s.Category == category && s.Level == level)
                                     .OrderByDescending(p => p.UpdatedDate)
                                     .FirstOrDefault();
                    }

                }
                else if (inGameProgress.Level == level - 1 && inGameProgress.Percentage == 100)
                {

                    //if player has completed the level below requested level, update progress            

                    inGameProgress.Category = category;
                    inGameProgress.Level = level;
                    inGameProgress.Percentage = 0;


                    savedGame = new SavedGame()
                    {
                        UserId = user.Id,
                        Category = category,
                        Level = level,
                        Progress = 0,
                        Json = jsonStartGame
                    };

                    _dbContext.SavedGames.Add(savedGame);

                    _dbContext.SaveChanges();

                }
                else
                {
                    return null;
                }

            }


            GameDataModel result = new GameDataModel
            { 
                category = category.ToString(),
                level = level.ToString(),
                xmlToolbox = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\toolbox{category}x{level}.xml")),
                xmlStartBlocks = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\common\\xmlStartBlocks{category}.xml")),
                savedGame = new Dictionary<string, string>() { { "json", savedGame.Json }, { "progress", savedGame.Progress.ToString() } },
                jsonTasks = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\modals{category}x{level}.json")),
                jsonModals = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\common\\modals.json")),
                jsonRatings = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\ratings{category}x{level}.json"))
            };

            return result;
        }
    }
}       
    

