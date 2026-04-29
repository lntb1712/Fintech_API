using FluentValidation;

namespace Model.RequestModel.Wallet
{
    public class TransferRequest
    {
        public Guid FromWalletId { get; set; }
        public Guid ToWalletId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class TransferRequestValidator : AbstractValidator<TransferRequest>
    {
        public TransferRequestValidator()
        {
            RuleFor(x => x.FromWalletId).NotEmpty().WithMessage("FromWalletId is required");
            RuleFor(x => x.ToWalletId).NotEmpty().WithMessage("ToWalletId is required");
            RuleFor(x => x.ToWalletId).NotEqual(x => x.FromWalletId).WithMessage("ToWalletId must be different from FromWalletId");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
        }
    }
}
