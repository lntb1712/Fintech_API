namespace Model.ResponseModel.Wallet
{
    public class SharedWalletMemberResponse
    {
        public Guid Id { get; set; }
        public Guid SharedWalletId { get; set; }
        public Guid UserId { get; set; }
    }
}
