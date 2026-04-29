using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.RequestModel.Wallet
{
    public class WalletRequest
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public required string Type { get; set; } //CASH | EWALLET | BANK
        public required string Currency { get; set; } 
        public decimal Balance { get; set; }
        public int Status { get; set; }
    }

    public class  WalletRequestValidator : AbstractValidator <WalletRequest>
    {
        public WalletRequestValidator()
        {
            RuleFor(x => x.Type).NotEmpty().WithMessage("Type is required");
            RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required");
            RuleFor(x => x.Balance).GreaterThanOrEqualTo(0).WithMessage("Balance must be greater than or equal to 0");
        }
    }
}
