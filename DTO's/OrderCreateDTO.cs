using SingularSystems_SelfKiosk_Software.DTO;
using SingularSystems_SelfKiosk_Software.DTOs;

namespace SingularSystems_SelfKiosk_Software.DTOs
{
    public class OrderCreateDTO
    {
        public int UserId { get; set; }
        public int CustomerTransactionId { get; set; }
        public decimal OrderTotalAmount { get; set; }
        public bool OrderStatus { get; set; }
        public List<OrderItemCreateDTO> OrderItems { get; set; }
    }
}
