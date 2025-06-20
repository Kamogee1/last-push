namespace SingularSystems_SelfKiosk_Software.DTO_s
{
    public class CustomerTransactionDTO
    {
        public int CustomerTransactionId { get; set; }
        public int? OrderId { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }

    }
}
