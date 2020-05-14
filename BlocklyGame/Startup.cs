using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BlocklyGame.Controllers;
using BlocklyGame.Helpers;
using BlocklyGame.Managers;
using BlocklyGame.Models;
using kedzior.io.ConnectionStringConverter;
using Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlocklyGame
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if(Configuration.GetSection("AppSettings").GetValue<bool>("MySQLDatabase"))
                {
                    string envConnectionString = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");
                    if (!String.IsNullOrEmpty(envConnectionString))
                    {
                        options.UseMySql(AzureMySQL.ToMySQLStandard(envConnectionString));
                    }
                    else
                    {
                        options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
                    }
                }     
                else
                {
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                }
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
                .AddEntityFrameworkStores<ApplicationDbContext>();                  
          
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            services.AddTransient<AdminSeeder>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = redirectContext =>
                    {
                        var uri = redirectContext.RedirectUri;
                        UriHelper.FromAbsolute(uri, out var scheme, out var host, out var path, out var query, out var fragment);
                        uri = UriHelper.BuildAbsolute(scheme, host, path, default, default, new FragmentString("#game"));
                        redirectContext.Response.Redirect(uri);
                        return Task.CompletedTask;
                    }
                };

                options.LoginPath = "/";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                Dictionary<string, string> localizations = Configuration.GetSection("AppSettings").GetSection("CountryCodeLocalization").GetChildren().ToDictionary(x => x.Key, x => x.Value);
                options.DefaultRequestCulture = new RequestCulture(localizations["default"]);

                List<CultureInfo> cultures = localizations.Values.Select(s => new CultureInfo(s)).Distinct().ToList();

                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders.Add(new LocalizationProvider());
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddTransient<DataManager>();

            services.AddMvc()
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(SharedResource));
                });    
                        
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AdminSeeder seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (Configuration.GetSection("AppSettings").GetValue<bool>("MigrationsEnabled")) 
            { 
                using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                }
            }

            //app.UseHttpsRedirection();

            app.UseRequestLocalization();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            if (Configuration.GetSection("AppSettings").GetValue<bool>("SeederEnabled"))
            {
                seeder.Run().Wait();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
