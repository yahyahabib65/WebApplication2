using System.Collections.Generic;
using WebApplication2.Models;

namespace WebApplication2.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public List<Store> Stores { get; set; }
        public int TotalStores { get; set; }
        public int PendingStores { get; set; }
        public int ApprovedStores { get; set; }
        public int RejectedStores { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
    }
}
