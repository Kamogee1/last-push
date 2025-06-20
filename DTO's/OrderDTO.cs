using System;
using System.Collections.Generic;


namespace SingularSystems_SelfKiosk_Software.DTO
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerTransactionId { get; set; }
        public decimal OrderTotalAmount { get; set; }
        public bool OrderStatus { get; set; }
        
        public List<OrderItemDTO>? OrderItems { get; set; }
    }
}
