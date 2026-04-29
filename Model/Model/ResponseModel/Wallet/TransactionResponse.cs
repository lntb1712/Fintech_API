using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ResponseModel.Wallet
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid WalletId { get; set; }

        public string? WalletName { get; set; }

        public Guid CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public decimal Amount { get; set; }

        public required string TransactionType { get; set; } 

        public string? Description { get; set; }
        public required string Source { get; set; }

        public string? ExternalReference { get; set; }
    }
}
