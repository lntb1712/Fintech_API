using FluentValidation;

namespace Model.RequestModel.Wallet
{
    public class BudgetRequest
    {
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Amount { get; set; }
        public required string Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class BudgetRequestValidator : AbstractValidator<BudgetRequest>
    {
        public BudgetRequestValidator()
        {
            RuleFor(x => x.WalletId).NotEmpty().WithMessage("WalletId is required");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("CategoryId is required");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
            RuleFor(x => x.Period).NotEmpty().WithMessage("Period is required");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("StartDate is required");
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage("EndDate must be greater than or equal to StartDate");
        }
    }
}
