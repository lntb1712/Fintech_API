using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entity.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fintech_Log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogType = table.Column<string>(type: "varchar(10)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(50)", nullable: false),
                    EndPoint = table.Column<string>(type: "varchar(50)", nullable: false),
                    ParamsOrBody = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_Account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "varchar(15)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(50)", nullable: false),
                    Email = table.Column<string>(type: "varchar(50)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Group = table.Column<string>(type: "varchar(50)", nullable: true),
                    Base = table.Column<string>(type: "varchar(15)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    FullNameNoAccent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_Account", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_Activity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "varchar(15)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    ApplicationName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_Activity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_AppVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppPlatform = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinVersion = table.Column<int>(type: "int", nullable: false),
                    AppVersion = table.Column<int>(type: "int", nullable: false),
                    VersionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReleaseNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DownloadPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAwaitingApproval = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsForceDev = table.Column<bool>(type: "bit", nullable: false),
                    IsForceUpdate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_AppVersion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_Device",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UDID = table.Column<string>(type: "varchar(50)", nullable: false),
                    OSVersion = table.Column<string>(type: "varchar(50)", nullable: true),
                    OSName = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeviceType = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeviceName = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeviceDescription = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RefreshToken = table.Column<string>(type: "varchar(1000)", nullable: false),
                    RfTokenCreateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    RfTokenExpiryTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    RfTokenRevokedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    RfTokenCreatedByIp = table.Column<string>(type: "varchar(50)", nullable: false),
                    RfTokenRevokedByIp = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_Device", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_MailTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MailCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MailTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MailSubject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MailContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_MailTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Receiver = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    IsSaved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_Role_Activity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    C = table.Column<bool>(type: "bit", nullable: false),
                    R = table.Column<bool>(type: "bit", nullable: false),
                    U = table.Column<bool>(type: "bit", nullable: false),
                    D = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_Role_Activity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_SysRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    RoleType = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_SysRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_User_Activity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    C = table.Column<bool>(type: "bit", nullable: false),
                    R = table.Column<bool>(type: "bit", nullable: false),
                    U = table.Column<bool>(type: "bit", nullable: false),
                    D = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_User_Activity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_UserDevice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceUUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppBuild = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PushToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DevicePlatform = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceOS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsNotify = table.Column<bool>(type: "bit", nullable: true),
                    IsActivate = table.Column<bool>(type: "bit", nullable: true),
                    IsMainDevice = table.Column<bool>(type: "bit", nullable: true),
                    OTPCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OTPDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OTPCount = table.Column<int>(type: "int", nullable: true),
                    OTPFailures = table.Column<int>(type: "int", nullable: true),
                    QRCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_UserDevice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fintech_Sys_UserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updater = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fintech_Sys_UserRole", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Log_CreatedDate",
                table: "Fintech_Log",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_Account_Code_UserName",
                table: "Fintech_Sys_Account",
                columns: new[] { "Code", "UserName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_Account_CreatedDate",
                table: "Fintech_Sys_Account",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_Activity_CreatedDate",
                table: "Fintech_Sys_Activity",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_Device_CreatedDate",
                table: "Fintech_Sys_Device",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_Device_UserId_UDID",
                table: "Fintech_Sys_Device",
                columns: new[] { "UserId", "UDID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_Role_Activity_CreatedDate",
                table: "Fintech_Sys_Role_Activity",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_SysRole_CreatedDate",
                table: "Fintech_Sys_SysRole",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_User_Activity_CreatedDate",
                table: "Fintech_Sys_User_Activity",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fintech_Sys_UserRole_CreatedDate",
                table: "Fintech_Sys_UserRole",
                column: "CreatedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fintech_Log");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_Account");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_Activity");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_AppVersion");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_Device");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_MailTemplate");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_Notification");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_Role_Activity");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_SysRole");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_User_Activity");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_UserDevice");

            migrationBuilder.DropTable(
                name: "Fintech_Sys_UserRole");
        }
    }
}
