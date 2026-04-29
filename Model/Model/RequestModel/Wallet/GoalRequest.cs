using FluentValidation;

namespace Model.RequestModel.Wallet
{
    public class GoalRequest
    {
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }
    }

    public class GoalRequestValidator : AbstractValidator<GoalRequest>
    {
        public GoalRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.TargetAmount).GreaterThan(0).WithMessage("TargetAmount must be greater than 0");
            RuleFor(x => x.CurrentAmount).GreaterThanOrEqualTo(0).WithMessage("CurrentAmount must be greater than or equal to 0");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("StartDate is required");
            RuleFor(x => x.TargetDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage("TargetDate must be greater than or equal to StartDate");
        }
    }
}
