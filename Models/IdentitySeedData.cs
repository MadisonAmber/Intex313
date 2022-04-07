using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Intex313.Models
{
    public static class IdentitySeedData
    {
        private const string TAUser = "TAGrading";
        private const string TAPass = "PleaseHaveMercy123!";
        private const string adminUser = "Admin";
        private const string adminPassword = "313SpencerOut:)";

        private const string adminRoleName = "Admin";
        private const string userRoleName = "User";

        public static async void EnsurePopulated(IApplicationBuilder app)
        {
            AccidentDbContext context = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<AccidentDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            UserManager<IdentityUser> userManager = app.ApplicationServices
                .CreateScope().ServiceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            var RoleManager = app.ApplicationServices
                .CreateScope().ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            IdentityUser user = await userManager.FindByIdAsync(adminUser);
            IdentityUser TAuser = await userManager.FindByIdAsync(TAUser);

            var adminRoleExists = await RoleManager.RoleExistsAsync(adminRoleName);
            var userRoleExists = await RoleManager.RoleExistsAsync(userRoleName);

            if (!adminRoleExists)
            {
                await RoleManager.CreateAsync(new IdentityRole(adminRoleName));
            }

            if (!userRoleExists)
            {
                await RoleManager.CreateAsync(new IdentityRole(userRoleName));
            }

            if (user == null)
            {
                user = new IdentityUser(adminUser);
                user.Email = "guest@gmail.com";
                user.PhoneNumber = "123-4567";

                var createAdminUser = await userManager.CreateAsync(user, adminPassword);

                if(createAdminUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
            if (TAuser == null)
            {
                TAuser = new IdentityUser(TAUser);
                TAuser.Email = "email@gmail.com";
                TAuser.PhoneNumber = "223-4567";

                var createRegularUser = await userManager.CreateAsync(TAuser, TAPass);
                if(createRegularUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(TAuser, "User");
                }
            }
        }
    }
}