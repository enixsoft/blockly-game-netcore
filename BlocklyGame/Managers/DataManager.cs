using BlocklyGame.Controllers;
using BlocklyGame.Helpers;
using BlocklyGame.Models;
using BlocklyGame.Models.Game;
using Localization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlocklyGame.Managers
{
    public class DataManager
    {
        private readonly ILogger<DataManager> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAntiforgery _antiforgery;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer _localizer;
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DataManager(
            ILogger<DataManager> logger,
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

        public async Task<IndexModel> CreateIndexModel(string csrfToken, string lang, ApplicationUser user, GameDataModel gameDataModel = null)
        {
            IndexModel indexModel = new IndexModel
            {
                BaseUrl = _appSettings.Value.BaseURL,
                CsrfToken = csrfToken,
                Lang = await System.IO.File.ReadAllTextAsync(Path.Combine(_hostingEnvironment.ContentRootPath, $"Resources\\Game\\locales\\{lang}.json")),
                Title = _localizer["title"],
                RecaptchaKey = _appSettings.Value.GOOGLE_RECAPTCHA_KEY
            };

            if (user != null)
            {
                indexModel.User = JsonSerializer.Serialize(new User(user.Id, user.UserName, user.Email, new List<string>(await _userManager.GetRolesAsync(user))));

                List<Progress> progressList = _dbContext.Progress.Where(s => s.UserId == user.Id).ToList();

                if (progressList != null)
                {
                    List<int> progressValues = new List<int>();

                    foreach (Progress progress in progressList)
                    {
                        for (int i = 1; i <= progress.Level; i++)
                        {
                            if (i == progress.Level)
                            {
                                progressValues.Add(progress.Percentage);
                            }
                            else
                            {
                                progressValues.Add(100);
                            }
                        }
                    }
                    indexModel.InGameProgress = JsonSerializer.Serialize(progressValues);
                }
            }

            if (gameDataModel != null)
            {
                indexModel.GameData = JsonSerializer.Serialize(gameDataModel);
            }
           
            return indexModel;
        }

        public class User
        { 
            public string id { get; set; }
            public string username { get; set; }
            public string email { get; set; }
            public string role { get; set; }

            public  User(string id, string username, string email, List<string> roles)
            {
                this.id = id;
                this.username = username;
                this.email = email;
                this.role = roles.Contains("Administrator") ? "admin" : "user";
            }
        }

    }
}
