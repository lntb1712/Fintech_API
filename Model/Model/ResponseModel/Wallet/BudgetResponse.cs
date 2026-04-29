namespace Model.ResponseModel.Wallet
{
    public class BudgetResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public string? WalletName { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
        public required string Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
