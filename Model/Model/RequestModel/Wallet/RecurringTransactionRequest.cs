using FluentValidation;

namespace Model.RequestModel.Wallet
{
    public class RecurringTransactionRequest
    {
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Amount { get; set; }
        public required string TransactionType { get; set; }
        public required string Frequency { get; set; }
        public int IntervalValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextRunDate { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }
    }

    public class RecurringTransactionRequestValidator : AbstractValidator<RecurringTransactionRequest>
    {
        public RecurringTransactionRequestValidator()
        {
            RuleFor(x => x.WalletId).NotEmpty().WithMessage("WalletId is required");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("CategoryId is required");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
            RuleFor(x => x.TransactionType).NotEmpty().WithMessage("TransactionType is required");
            RuleFor(x => x.Frequency).NotEmpty().WithMessage("Frequency is required");
            RuleFor(x => x.IntervalValue).GreaterThan(0).WithMessage("IntervalValue must be greater than 0");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("StartDate is required");
            RuleFor(x => x.NextRunDate).NotEmpty().WithMessage("NextRunDate is required");
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue)
                .WithMessage("EndDate must be greater than or equal to StartDate");
        }
    }
}
