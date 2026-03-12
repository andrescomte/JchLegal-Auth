namespace AuthService.Domain.SeedWork
{
    public interface IEntityRepository<T> : IRepository where T : Entity
    {
        T Create(T businessObject);
    }
}