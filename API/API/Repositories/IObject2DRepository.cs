using API.Models;

namespace API.Repositories
{
    public interface IObject2DRepository
    {
        Task<IEnumerable<Object2D>> GetAllAsync();
        Task<Object2D> GetByIdAsync(Guid id);
        Task<Guid> InsertAsync(Object2D object2D);
        Task<bool> UpdateAsync(Object2D object2D);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<Object2D>> GetByEnvironmentIdAsync(Guid environmentId);
    }
}