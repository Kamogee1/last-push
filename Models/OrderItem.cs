using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Singular_Systems_SelfKiosk_Software.Models
{
    public class OrderItem
    {
       
        public int OrderItemId { get; set; } //Primary Key 

        public int ProductId { get; set; }
        public  int OrderId { get; set; } // Foreign Key into the OrderItem Table

        public int OrderItemsQuantity { get; set; }

        [Precision(18, 2)]
        public decimal OrderItemSubtotal { get; set; }

        public int Quantity { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }


    }
}
