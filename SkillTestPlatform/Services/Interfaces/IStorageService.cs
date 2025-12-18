namespace SkillTestPlatform.Services.Interfaces
{
    public interface IStorageService<T>
    {
        Task<T?> GetAsync(string id);
        Task<IReadOnlyList<T>> GetAsync();
        Task AddAsync(T entity, string id);
        Task UpdateAsync(T entity, string id);
        Task<bool> DeleteAsync(string id);
    }
}
