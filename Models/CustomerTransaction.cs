using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Singular_Systems_SelfKiosk_Software.Models;

namespace SingularSystems_SelfKiosk_Software.Models
{
    public class CustomerTransaction
    {
        
        public int CustomerTransactionId { get; set; } 
        public  int? OrderId { get; set; }
        public int WalletId { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public DateTime Date  { get; set; }
        public string Status { get; set; }
        public  Order? Order { get; set; }
        public  Wallet Wallet { get; set; }

    }
}
