using BlocklyGame.Helpers;
using BlocklyGame.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AdminSeeder
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly ApplicationDbContext context;
    private readonly IOptions<ApplicationSettings> appSettings;


    public AdminSeeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IOptions<ApplicationSettings> appSettings)
    {      
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.context = context;
        this.appSettings = appSettings;
    }

    public async Task Run()
    {
        var users = context.Users;

        if (!context.Roles.Any(r => r.Name == "Administrator"))
        {
            await roleManager.CreateAsync(new IdentityRole()
            {
                Name = "Administrator"
            });
        }

        if (!context.Users.Any(u => u.UserName == "admin"))
        {
            await userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@blocklygame.com"
            }, appSettings.Value.AdminPassword);

            await userManager.AddToRoleAsync(userManager.FindByNameAsync("admin").Result, "Administrator");
        }
    }

}