using FluentValidation;

namespace Model.RequestModel.Wallet
{
    public class SharedWalletRequest
    {
        public Guid WalletId { get; set; }
        public Guid OwnerId { get; set; }
    }

    public class SharedWalletRequestValidator : AbstractValidator<SharedWalletRequest>
    {
        public SharedWalletRequestValidator()
        {
            RuleFor(x => x.WalletId).NotEmpty().WithMessage("WalletId is required");
        }
    }
}
