using API.Models;

namespace API.Repositories
{
    public interface IEnvironment2DRepository
    {
        Task<IEnumerable<Environment2D>> GetAllAsync();
        Task<Environment2D> GetByIdAsync(Guid id);
        Task<Guid> InsertAsync(Environment2D environment);
        Task<bool> UpdateAsync(Environment2D environment);
        Task<bool> DeleteAsync(Guid id);
    }
}
