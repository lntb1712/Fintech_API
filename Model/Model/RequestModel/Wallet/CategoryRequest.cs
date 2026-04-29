using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.RequestModel.Wallet
{
    public class CategoryRequest
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public Guid ParentId { get; set; }
        public required string Type { get; set; } 
    }

    public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
    {
        public CategoryRequestValidator()
        {
            RuleFor(x => x.Type).NotEmpty().WithMessage("Type is required");
        }
    }
}
