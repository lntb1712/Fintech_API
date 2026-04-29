using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.Account;
using DomainService.Interfaces.Wallet;
using Entity.Entities.Wallet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel.Wallet;
using Model.ResponseModel.Wallet;

namespace Infrastructure.Implements.Wallet
{
    public class CategoryService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), ICategoryService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateCategoryAsync(Guid currentUserId, CategoryRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var category = new FintechCategory
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Name = request.Name,
                ParentId = request.ParentId,
                Type = request.Type,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechCategory>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(category));
        }

        public async Task<object> DeleteCategoryAsync(Guid currentUserId, Guid categoryId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var category = await GetOwnedCategory(currentUserId, categoryId);
            category.IsDeleted = true;
            category.UpdatedById = currentUserId;
            category.Updater = user.FullName;
            category.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechCategory>().Update(category);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(category));
        }

        public async Task<object> GetAllCategoriesAsync(Guid currentUserId)
        {
            var categories = await _unitOfWork.Repository<FintechCategory>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => new CategoryResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Name = x.Name ?? string.Empty,
                    ParentId = x.ParentId,
                    ParentName = _unitOfWork.Repository<FintechCategory>()
                        .Where(p => p.Id == x.ParentId && !p.IsDeleted)
                        .Select(p => p.Name)
                        .FirstOrDefault(),
                    Type = x.Type
                })
                .ToListAsync();

            return Utils.CreateResponseModel(categories, categories.Count);
        }

        public async Task<object> GetCategoryByIdAsync(Guid currentUserId, Guid categoryId)
        {
            var category = await GetOwnedCategory(currentUserId, categoryId);
            return Utils.CreateResponseModel(ToResponse(category), 1);
        }

        public async Task<object> UpdateCategoryAsync(Guid currentUserId, Guid categoryId, CategoryRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var category = await GetOwnedCategory(currentUserId, categoryId);
            category.Name = request.Name;
            category.ParentId = request.ParentId;
            category.Type = request.Type;
            category.UpdatedById = currentUserId;
            category.Updater = user.FullName;
            category.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechCategory>().Update(category);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(category), 1);
        }

        private async Task<FintechCategory> GetOwnedCategory(Guid currentUserId, Guid categoryId)
        {
            return await _unitOfWork.Repository<FintechCategory>()
                .FirstOrDefaultAsync(x => x.Id == categoryId && x.UserId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Category"));
        }

        private static CategoryResponse ToResponse(FintechCategory category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                UserId = category.UserId,
                Name = category.Name ?? string.Empty,
                ParentId = category.ParentId,
                Type = category.Type
            };
        }
    }
}
