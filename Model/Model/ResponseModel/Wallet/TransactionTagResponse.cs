namespace Model.ResponseModel.Wallet
{
    public class TransactionTagResponse
    {
        public Guid Id { get; set; }
        public Guid TagId { get; set; }
        public string? TagName { get; set; }
        public required string Description { get; set; }
    }
}
