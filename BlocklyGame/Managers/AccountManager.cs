//using BlocklyGame.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace BlocklyGame.Controllers
//{
//    public class AccountManager
//    {
//        private readonly IServiceProvider _serviceProvider;


//        public AccountManager(IServiceProvider serviceProvider)
//        {
//            _serviceProvider = serviceProvider;
//            _ = CreateAdmin();
//        }

//        private async Task CreateAdmin()
//        {
//        try 
//            {   

//            var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
//            dbContext.Database.EnsureCreated();

//            var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
//            var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//            if (!userManager.Users.Any(u => u.UserName == "admin"))
//            {
//                await roleManager.CreateAsync(new IdentityRole()
//                {
//                    Name = "Administrator"
//                });

//                await userManager.CreateAsync(new User
//                {
//                    UserName = "admin",
//                    Email = "admin@blocklygame.com"
//                }, "admin123");

//               await userManager.AddToRoleAsync(userManager.FindByNameAsync("admin").Result, "Administrator");
//            }
//        }
//            catch (Exception ex)
//            {
//                var str = ex;
//            }
//        }
//    }
//}
