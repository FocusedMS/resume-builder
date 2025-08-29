using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResumeApi.Models;

namespace ResumeApi
{
    public static class IdentitySeeder
    {
        public static async Task SeedIdentityAsync(this IHost app)
        {
            using var scope = app.Services.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var roles = new[] { "Admin", "RegisteredUser", "Guest" };
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            var email = cfg["AdminSeed:Email"];
            var pwd = cfg["AdminSeed:Password"];
            var usern = cfg["AdminSeed:UserName"] ?? email;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd))
                return; // nothing to seed

            var user = await userMgr.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser 
                { 
                    UserName = usern, 
                    Email = email, 
                    EmailConfirmed = true,
                    FullName = usern
                };
                var create = await userMgr.CreateAsync(user, pwd);
                if (!create.Succeeded) 
                    throw new Exception(string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            // ensure Admin role
            if (!await userMgr.IsInRoleAsync(user, "Admin"))
                await userMgr.AddToRoleAsync(user, "Admin");
        }
    }
}
