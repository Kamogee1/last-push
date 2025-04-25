using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Singular_Systems_SelfKiosk_Software.Models;

namespace SingularSystems_SelfKiosk_Software.Models
{
    public class CustomerTransaction
    {
        
        public int CustomerTransactionId { get; set; } //Primary Key 

        public  int OrderId { get; set; }

        public int WalletId { get; set; }
        public int Amount { get; set; }

        public string Type { get; set; }

        public DateTime Date  { get; set; }

        public string Status { get; set; }

        // Navigation property: A transaction can have multiple orders 
        

        // Navigation property: A wallet can have multiple transactions 
       

        public  Order Order { get; set; }

        public  Wallet Wallet { get; set; }

    }
}
