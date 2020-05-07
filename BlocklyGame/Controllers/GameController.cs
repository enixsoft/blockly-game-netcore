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
            bool requiresJsonResponse = HttpContext.Request.GetTypedHeaders()
                                                   .Accept.Any(t => t.Suffix.Value?.ToUpper() == "JSON"
                                                   || t.SubTypeWithoutSuffix.Value?.ToUpper() == "JSON");

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            Dictionary<string, string> result = await RunGame(category, level, user);

            if (requiresJsonResponse)
            {
                return Json(result);
            }

            TempData["gamedata"] = result;
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private async Task<Dictionary<string, string>> RunGame(int category, int level, ApplicationUser user)
        {
            int[] categoryHasLevelsArray = { 6, 6 };
            int categoryMin = 1;
            int categoryMax = categoryHasLevelsArray.Length;
            int levelMin = 1;
            int levelMax = 0;

            if (category <= categoryMax && category >= categoryMin)
            {
                levelMax = categoryHasLevelsArray[category - 1];
            }

            if (category > categoryMax || category < categoryMin || level < levelMin || level > levelMax)
            {
                return new Dictionary<string, string>();
            }
            else if (category == categoryMax && level == levelMax)
            {
                return new Dictionary<string, string>();
            }
            else if (category < categoryMax && level == levelMax)
            {
                return await RunGame(category + 1, levelMin, user);
            }

            Progress inGameProgress = _dbContext.Progress
                                      .Where(p => p.UserId == user.Id && p.Category == category)
                                      .FirstOrDefault();

            // string lang = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;

            string xmlToolBox = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\toolbox{category}x{level}.xml"));
            string xmlStartBlocks = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\common\\xmlStartBlocks{category}.xml"));
            string jsonStartGame = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\start{category}x{level}.json"));
            string jsonTasks = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\modals{category}x{level}.json"));
            string jsonRatings = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\{category}x{level}\\ratings{category}x{level}.json"));
            string jsonModals = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\common\\modals.json"));
            SavedGame savedGame = null;

            if (inGameProgress == null) //game was not played at all or requested category was not played yet            
            {
                if (category == 1)
                {
                    //no ingame progress in category 1 exists yet for this user, set level1 with 0 progress
                    //Progress::create(['username' => $auth->username, 'category' => $category, 'level' => $level, 'progress' => 0]);

                    _dbContext.Progress.Add(new Progress()
                    {
                        UserId = user.Id,
                        Category = category,
                        Level = level,
                        Percentage = 0
                    });

                    //no saved game in this category and level exists yet for this user, create first saved game with 0 progress from startgame json
                    _dbContext.SavedGames.Add(new SavedGame()
                    {
                        UserId = user.Id,
                        Category = category,
                        Level = level,
                        Progress = 0,
                        Json = jsonStartGame
                    });

                    _dbContext.SaveChanges();
                }

                else
                {
                    //check if player has 100 progress in previous max level of previous category
                    var inGameProgressOfPreviousLevelOfPreviousCategory = _dbContext.Progress
                                      .Where(p => p.UserId == user.Id && (p.Category == category - 1) && (level == categoryHasLevelsArray[category - 1] - 1))
                                      .First();

                    if (inGameProgressOfPreviousLevelOfPreviousCategory == null || inGameProgressOfPreviousLevelOfPreviousCategory.Percentage != 100)
                    {
                        //redirect startNewGameOrContinue
                        //return $this->startNewGameOrContinue($request);
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
                            return await RunGame(category, 1, user);
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
                                      .OrderByDescending(p => p.Id)
                                      .First();
                }
                else if (inGameProgress.Level == level)
                {
                    //if player has progress on the par with requested level, has not completed it yet
                    if (inGameProgress.Percentage != 100)
                    {

                        savedGame = _dbContext.SavedGames
                                      .Where(s => s.UserId == user.Id && s.Category == category && s.Level == level && s.Progress == inGameProgress.Percentage)
                                      .OrderByDescending(p => p.Id)
                                      .First();

                        //if player due to error does not have the savedgame with progress he has, he gets the latest savedgame
                        if (savedGame == null)
                        {
                            savedGame = _dbContext.SavedGames
                                      .Where(s => s.UserId == user.Id && s.Category == category && s.Level == level)
                                      .OrderByDescending(p => p.Id)
                                      .First();
                        }

                    }

                    //player has completed the level, but requests it again
                    else
                    {

                        savedGame = _dbContext.SavedGames
                                     .Where(s => s.UserId == user.Id && s.Category == category && s.Level == level)
                                     .OrderByDescending(p => p.Id)
                                     .First();
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
                    //return redirect
                    //return $this->startNewGameOrContinue($request);
                }

            }
   

            Dictionary<string, string> result = new Dictionary<string, string>();

            result.Add("category", category.ToString());
            result.Add("level", level.ToString());
            result.Add("xmlToolbox", xmlToolBox);
            result.Add("xmlStartBlocks", xmlStartBlocks);
            result.Add("savedGame", JsonSerializer.Serialize(savedGame));
            result.Add("jsonTasks", jsonTasks);
            result.Add("jsonModals", jsonModals);
            result.Add("jsonRatings", jsonRatings);

            return result;
            //return $this->redirectOrSendResponse(compact('category', 'level', 'xmlToolbox', 'xmlStartBlocks', 'savedGame', 'jsonTasks', 'jsonModals', 'jsonRatings'), $request);
        }
    }
}       
    

