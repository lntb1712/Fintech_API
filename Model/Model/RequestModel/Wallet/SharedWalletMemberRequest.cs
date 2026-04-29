using FluentValidation;

namespace Model.RequestModel.Wallet
{
    public class SharedWalletMemberRequest
    {
        public Guid SharedWalletId { get; set; }
        public Guid UserId { get; set; }
    }

    public class SharedWalletMemberRequestValidator : AbstractValidator<SharedWalletMemberRequest>
    {
        public SharedWalletMemberRequestValidator()
        {
            RuleFor(x => x.SharedWalletId).NotEmpty().WithMessage("SharedWalletId is required");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        }
    }
}
