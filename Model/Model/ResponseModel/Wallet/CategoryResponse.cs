using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ResponseModel.Wallet
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public Guid ParentId { get; set; }
        public string? ParentName { get; set; }
        public required string Type { get; set; } // INCOME | EXPENSE | TRANSFER
    }
}
