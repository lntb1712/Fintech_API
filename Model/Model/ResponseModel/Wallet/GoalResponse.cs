namespace Model.ResponseModel.Wallet
{
    public class GoalResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }
    }
}
