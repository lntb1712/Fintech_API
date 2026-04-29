using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ResponseModel.Wallet
{
    public class WalletResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? FullName { get; set; }

        public required string Name { get; set; }

        public required string Type { get; set; } //CASH | EWALLET | BANK

        public required string Currency { get; set; } //USD | VND | EUR

        public required decimal Balance { get; set; }

        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

    }
}
