namespace CFFileSystemConnection.Interfaces
{
    /// <summary>
    /// Interface for service that manages an entity with an Id property
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntityWithIdService<TEntity> where TEntity : class
    {
        List<TEntity> GetAll();

        TEntity? GetById(string id);
        
        void Add(TEntity entity);

        void Delete(string id);

        void Update(TEntity entity);
    }
}
