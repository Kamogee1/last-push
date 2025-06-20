namespace SingularSystems_SelfKiosk_Software.DTO
{
    public class OrderUpdateDTO
    {
        public int OrderId { get; set; }  // still needed to match URL param
        public int UserId { get; set; }
        public int CustomerTransactionId { get; set; }
        public decimal OrderTotalAmount { get; set; }
        public bool OrderStatus { get; set; }
        public List<OrderItemCreateDTO> OrderItems { get; set; }  // same as for create
    }
}
