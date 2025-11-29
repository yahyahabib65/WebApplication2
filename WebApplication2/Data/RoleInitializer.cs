using Microsoft.AspNetCore.Identity;
using WebApplication2.Models;

namespace WebApplication2.Data;

public static class RoleInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        string[] roleNames = { "Admin", "Seller", "Customer" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Create the roles and seed them to the database
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create Admin user
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = "admin@example.com", Email = "admin@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, "Password123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Create Seller user and store
        var sellerUser = await userManager.FindByEmailAsync("seller@example.com");
        if (sellerUser == null)
        {
            sellerUser = new IdentityUser { UserName = "seller@example.com", Email = "seller@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(sellerUser, "Password123!");
            await userManager.AddToRoleAsync(sellerUser, "Seller");

            var store = new Store
            {
                Name = "Seller's Store",
                OwnerId = sellerUser.Id,
                Status = StoreStatus.Approved,
                PaymentStatus = PaymentStatus.Approved,
                BusinessAddress = "123 Main St",
                BusinessRegistrationNumber = "123456789"
            };
            context.Stores.Add(store);
            await context.SaveChangesAsync();
        }

        // Create Customer user
        var customerUser = await userManager.FindByEmailAsync("customer@example.com");
        if (customerUser == null)
        {
            customerUser = new IdentityUser { UserName = "customer@example.com", Email = "customer@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(customerUser, "Password123!");
            await userManager.AddToRoleAsync(customerUser, "Customer");
        }

        // Seed Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Electronics" },
                new Category { Name = "Books" },
                new Category { Name = "Clothing" },
                new Category { Name = "Home & Kitchen" },
                new Category { Name = "Toys & Games" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}
