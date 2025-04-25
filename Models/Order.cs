using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.PortableExecutable;
using System.Transactions;
using SingularSystems_SelfKiosk_Software.Models;

namespace Singular_Systems_SelfKiosk_Software.Models
{
    public class Order
{
        
        public int OrderId { get; set; } // Primary Key

        public int UserId { get; set; } //Foreign Key ino the order table 

        public DateTime OrderDate { get; set; }
     
        public int CustomerTransactionId { get; set; }

        [Column(TypeName = "decimal(10,2)")] 
        public decimal OrderTotalAmount { get; set; }

        public bool OrderStatus { get; set; }

       // Navigation property:  
        public  User User { get; set; }


        
        public  ICollection<OrderItem> OrderItems { get; set; } // Navigation property
        public ICollection<CustomerTransaction> CustomerTransactions { get; set; } // Navigation property







    }
}
