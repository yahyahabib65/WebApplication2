using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public IdentityUser Owner { get; set; }

        public StoreStatus Status { get; set; } = StoreStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.NotPaid;
        public string? PaymentProofPath { get; set; }

        // Add other store-related properties here
        public string? BusinessAddress { get; set; }
        public string? BusinessRegistrationNumber { get; set; }
    }
}
