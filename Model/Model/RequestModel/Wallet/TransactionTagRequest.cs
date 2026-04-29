using FluentValidation;

namespace Model.RequestModel.Wallet
{
    public class TransactionTagRequest
    {
        public Guid TagId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class TransactionTagRequestValidator : AbstractValidator<TransactionTagRequest>
    {
        public TransactionTagRequestValidator()
        {
            RuleFor(x => x.TagId).NotEmpty().WithMessage("TagId is required");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
        }
    }
}
