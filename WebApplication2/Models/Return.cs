using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Models
{
    public class Return
    {
        [Key]
        public int ReturnId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public DateTime ReturnDate { get; set; }

        public string Reason { get; set; }

        public ReturnStatus Status { get; set; }
    }

    public enum ReturnStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
