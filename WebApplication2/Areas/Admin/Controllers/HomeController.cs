using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using WebApplication2.Areas.Admin.Models;

namespace WebApplication2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stores = await _context.Stores
                .Include(s => s.Owner)
                .Where(s => s.PaymentStatus == PaymentStatus.PendingVerification || s.Status != StoreStatus.Pending)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                Stores = stores,
                TotalStores = await _context.Stores.CountAsync(),
                PendingStores = await _context.Stores.CountAsync(s => s.Status == StoreStatus.Pending),
                ApprovedStores = await _context.Stores.CountAsync(s => s.Status == StoreStatus.Approved),
                RejectedStores = await _context.Stores.CountAsync(s => s.Status == StoreStatus.Rejected),
                TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount),
                TotalOrders = await _context.Orders.CountAsync()
            };

            return View(viewModel);
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
                TempData["success"] = "Payment approved and store enabled successfully.";
            }
            else
            {
                TempData["error"] = "Store not found.";
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
                TempData["success"] = "Payment rejected and store disabled successfully.";
            }
            else
            {
                TempData["error"] = "Store not found.";
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
                TempData["success"] = "Store disabled successfully.";
                // TODO: Implement refund logic
            }
            else
            {
                TempData["error"] = "Store not found.";
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
                TempData["success"] = "Store re-enabled successfully.";
            }
            else
            {
                TempData["error"] = "Store not found or already active.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SalesReport()
        {
            var salesData = await _context.OrderDetails
                .Include(od => od.Order)
                    .ThenInclude(o => o.Store)
                        .ThenInclude(s => s.Owner)
                .Select(od => new SalesReportViewModel
                {
                    Date = od.Order.OrderDate,
                    VendorId = od.Order.Store.Owner.Id,
                    Name = od.Order.Store.Owner.UserName,
                    StoreName = od.Order.Store.Name,
                    Payment = od.UnitPrice * od.Quantity
                })
                .ToListAsync();

            return View(salesData);
        }

        public async Task<IActionResult> VendorRatings()
        {
            var stores = await _context.Stores
                .Include(s => s.Owner)
                .ToListAsync();

            var viewModel = new List<VendorRatingViewModel>();
            foreach (var store in stores)
            {
                var ratings = await _context.Ratings
                    .Where(r => r.StoreId == store.Id)
                    .ToListAsync();

                var avgRating = ratings.Any() ? ratings.Average(r => r.Stars) : 0;

                viewModel.Add(new VendorRatingViewModel
                {
                    StoreId = store.Id,
                    StoreName = store.Name,
                    AverageRating = avgRating,
                    Ratings = ratings,
                    IsBlocked = store.Status == StoreStatus.Rejected // Assuming 'Rejected' status means blocked
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> BlockVendor(int storeId)
        {
            var store = await _context.Stores.FindAsync(storeId);
            if (store != null)
            {
                store.Status = StoreStatus.Rejected; // Or a new 'Blocked' status
                await _context.SaveChangesAsync();
                TempData["success"] = "Vendor blocked successfully.";
            }
            else
            {
                TempData["error"] = "Store not found.";
            }
            return RedirectToAction(nameof(VendorRatings));
        }

        [HttpPost]
        public async Task<IActionResult> UnblockVendor(int storeId)
        {
            var store = await _context.Stores.FindAsync(storeId);
            if (store != null)
            {
                store.Status = StoreStatus.Approved;
                await _context.SaveChangesAsync();
                TempData["success"] = "Vendor unblocked successfully.";
            }
            else
            {
                TempData["error"] = "Store not found.";
            }
            return RedirectToAction(nameof(VendorRatings));
        }

        public async Task<IActionResult> DownloadSalesReportCsv()
        {
            var salesData = await GetSalesDataAsync();
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Date,VendorId,Name,StoreName,Payment");
            foreach (var sale in salesData)
            {
                builder.AppendLine($"{sale.Date},{sale.VendorId},{sale.Name},{sale.StoreName},{sale.Payment}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "sales-report.csv");
        }

        private async Task<List<SalesReportViewModel>> GetSalesDataAsync()
        {
            return await _context.OrderDetails
                .Include(od => od.Order)
                    .ThenInclude(o => o.Store)
                        .ThenInclude(s => s.Owner)
                .Select(od => new SalesReportViewModel
                {
                    Date = od.Order.OrderDate,
                    VendorId = od.Order.Store.Owner.Id,
                    Name = od.Order.Store.Owner.UserName,
                    StoreName = od.Order.Store.Name,
                    Payment = od.UnitPrice * od.Quantity
                })
                .ToListAsync();
        }

        public async Task<IActionResult> DownloadSalesReportPdf()
        {
            var salesData = await GetSalesDataAsync();

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                document.Add(new Paragraph("Sales Report"));
                document.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(5);
                table.AddCell("Date");
                table.AddCell("Vendor ID");
                table.AddCell("Name");
                table.AddCell("Store Name");
                table.AddCell("Payment");

                foreach (var sale in salesData)
                {
                    table.AddCell(sale.Date.ToShortDateString());
                    table.AddCell(sale.VendorId);
                    table.AddCell(sale.Name);
                    table.AddCell(sale.StoreName);
                    table.AddCell(sale.Payment.ToString("C"));
                }

                document.Add(table);
                document.Close();
                writer.Close();

                return File(ms.ToArray(), "application/pdf", "sales-report.pdf");
            }
        }
    }
}
