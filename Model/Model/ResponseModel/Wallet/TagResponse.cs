namespace Model.ResponseModel.Wallet
{
    public class TagResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
    }
}
