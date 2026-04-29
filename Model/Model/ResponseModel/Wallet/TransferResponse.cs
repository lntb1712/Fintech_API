namespace Model.ResponseModel.Wallet
{
    public class TransferResponse
    {
        public Guid Id { get; set; }
        public Guid FromWalletId { get; set; }
        public string? FromWalletName { get; set; }
        public Guid ToWalletId { get; set; }
        public string? ToWalletName { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
