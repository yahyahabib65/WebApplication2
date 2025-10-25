using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stores = await _context.Stores
                .Include(s => s.Owner)
                .Where(s => s.PaymentStatus == PaymentStatus.PendingVerification || s.Status != StoreStatus.Pending)
                .ToListAsync();
            return View(stores);
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePayment(int storeId)
        {
            var store = await _context.Stores.FindAsync(storeId);
            if (store != null)
            {
                store.PaymentStatus = PaymentStatus.Approved;
                store.Status = StoreStatus.Approved; // Or keep this as a separate step
                await _context.SaveChangesAsync();
                // You might want to send an email to the vendor here
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectPayment(int storeId)
        {
            var store = await _context.Stores.FindAsync(storeId);
            if (store != null)
            {
                store.PaymentStatus = PaymentStatus.Rejected;
                store.Status = StoreStatus.Rejected;
                await _context.SaveChangesAsync();
                // You might want to send an email to the vendor here
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DisableStore(int storeId)
        {
            var store = await _context.Stores.FindAsync(storeId);
            if (store != null)
            {
                store.Status = StoreStatus.Rejected; // Or a new 'Disabled' status
                await _context.SaveChangesAsync();
                // TODO: Implement refund logic
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EnableStore(int storeId)
        {
            var store = await _context.Stores.FindAsync(storeId);
            if (store != null && store.Status == StoreStatus.Rejected)
            {
                store.Status = StoreStatus.Approved;
                store.PaymentStatus = PaymentStatus.Approved; // Also re-approve payment
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
