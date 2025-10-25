using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult RegisterVendor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterVendor(RegisterVendorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Programmatically confirm the email for the vendor
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, code);

                    // Assign the "Seller" role
                    await _userManager.AddToRoleAsync(user, "Seller");

                    // Create a new store for the vendor
                    var store = new Store
                    {
                        Name = model.StoreName,
                        OwnerId = user.Id,
                        BusinessAddress = model.BusinessAddress,
                        BusinessRegistrationNumber = model.BusinessRegistrationNumber,
                        Status = StoreStatus.Pending
                    };
                    _context.Stores.Add(store);
                    await _context.SaveChangesAsync();

                    // Redirect to a confirmation page instead of signing in
                    return RedirectToAction("Pay", "Payment", new { storeId = store.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VendorRegistrationConfirmation()
        {
            return View();
        }
    }
}
