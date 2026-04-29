namespace Model.ResponseModel.Wallet
{
    public class SharedWalletResponse
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public string? WalletName { get; set; }
        public Guid OwnerId { get; set; }
    }
}
