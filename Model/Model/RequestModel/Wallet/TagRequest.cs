using FluentValidation;

namespace Model.RequestModel.Wallet
{
    public class TagRequest
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
    }

    public class TagRequestValidator : AbstractValidator<TagRequest>
    {
        public TagRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }
}
