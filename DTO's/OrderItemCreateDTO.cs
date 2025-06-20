namespace SingularSystems_SelfKiosk_Software.DTO
{
    public class OrderItemCreateDTO
    {
        public int ProductId { get; set; }
        public int OrderItemsQuantity { get; set; }
        public decimal OrderItemSubtotal { get; set; }
    }
}
