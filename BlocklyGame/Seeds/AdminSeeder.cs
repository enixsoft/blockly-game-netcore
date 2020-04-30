using BlocklyGame.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AdminSeeder
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly ApplicationDbContext context;

    public AdminSeeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {      
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.context = context;
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
            }, "admin123");

            await userManager.AddToRoleAsync(userManager.FindByNameAsync("admin").Result, "Administrator");
        }
    }

}