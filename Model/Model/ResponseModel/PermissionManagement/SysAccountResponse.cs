using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ResponseModel.PermissionManagement
{
    public class SysAccountResponse
    {
        public Guid Id { get; set; }
        public required string Code { get; set; } // Mã nhân viên
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Group { get; set; }
        public string? Base { get; set; }
        public string? Note { get; set; }
        public string? FullNameNoAccent { get; set; }
        public string? Avatar { get; set; }
        public List<UserPermissionBaseResponse> LstUserPermission { get; set; } = new List<UserPermissionBaseResponse>();
        public DateTime CreatedDate { get; set; }
        public string? Creator { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", LastName == null ? "" : LastName.Trim(), FirstName == null ? "" : FirstName.Trim());
            }
        }
    }
}