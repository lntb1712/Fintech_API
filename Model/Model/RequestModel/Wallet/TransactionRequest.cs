using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.RequestModel.Wallet
{
    public class TransactionRequest
    {
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Amount { get; set; }
        public required string TransactionType { get; set; } // INCOME | EXPENSE | TRANSFER
        public string? Description { get; set; }
        public required string Source { get; set; }  // MANUAL | SMS | IMPORT
        public string? ExternalReference { get; set; }
    }

    public class TransactionRequestValidator : AbstractValidator<TransactionRequest>
    {
        public TransactionRequestValidator()
        {
            RuleFor(x => x.WalletId).NotEmpty().WithMessage("WalletId is required");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("CategoryId is required");
            RuleFor(x => x.TransactionType).NotEmpty().WithMessage("TransactionType is required");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
            RuleFor(x => x.Source).NotEmpty().WithMessage("Source is required");
        }
    }
}
