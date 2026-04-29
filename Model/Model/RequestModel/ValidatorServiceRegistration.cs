using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Model.RequestModel.PermissionManagement;
using Model.RequestModel.Wallet;

namespace Model.RequestModel
{
    public static class ValidatorServiceRegistration
    {
        public static IServiceCollection ValidatorsServiceRegistration(this IServiceCollection service)
        {
            service.AddScoped<IValidator<SysActivityRequest>, SysActivityRequestValidator>();
            service.AddScoped<IValidator<SysRoleRequest>, SysRoleRequestValidator>();
            service.AddScoped<IValidator<SysAccountRequest>, SysAccountRequestValidator>();
            service.AddScoped<IValidator<SysDeviceRequest>, SysDeviceRequestValidator>();
            service.AddScoped<IValidator<WalletRequest>, WalletRequestValidator>();
            service.AddScoped<IValidator<CategoryRequest>, CategoryRequestValidator>();
            service.AddScoped<IValidator<TransactionRequest>, TransactionRequestValidator>();
            service.AddScoped<IValidator<BudgetRequest>, BudgetRequestValidator>();
            service.AddScoped<IValidator<GoalRequest>, GoalRequestValidator>();
            service.AddScoped<IValidator<RecurringTransactionRequest>, RecurringTransactionRequestValidator>();
            service.AddScoped<IValidator<TagRequest>, TagRequestValidator>();
            service.AddScoped<IValidator<SharedWalletRequest>, SharedWalletRequestValidator>();
            service.AddScoped<IValidator<SharedWalletMemberRequest>, SharedWalletMemberRequestValidator>();
            service.AddScoped<IValidator<TransactionTagRequest>, TransactionTagRequestValidator>();
            service.AddScoped<IValidator<TransferRequest>, TransferRequestValidator>();
            return service;
        }
    }
}
