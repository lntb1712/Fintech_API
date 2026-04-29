using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface ICategoryService
    {
        Task<object> CreateCategoryAsync(Guid currentUserId, CategoryRequest request);
        Task<object> GetCategoryByIdAsync(Guid currentUserId, Guid categoryId);
        Task<object> GetAllCategoriesAsync(Guid currentUserId);
        Task<object> UpdateCategoryAsync(Guid currentUserId, Guid categoryId, CategoryRequest request);
        Task<object> DeleteCategoryAsync(Guid currentUserId, Guid categoryId);
    }
}
