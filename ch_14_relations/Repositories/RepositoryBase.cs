namespace Repositories;

public abstract class RepositoryBase<T>
where T:class, new()
{
    protected readonly RepositoryContext _context;

    protected RepositoryBase(RepositoryContext context)
    {
        _context = context;
    }

    public virtual T? Get(int id) =>
        _context
            .Set<T>()
            .Find(id);
    
    public virtual List<T> GetAll() =>
        _context
            .Set<T>()
            .ToList();

    public virtual void Add(T item)
    {
        _context
            .Set<T>()
            .Add(item);
        _context.SaveChanges();
    }
    
    public virtual void Remove(T item)
    {
        _context
            .Set<T>()
            .Remove(item);
        _context.SaveChanges();
    }

    public virtual void Update(T item)
    {
        _context
            .Set<T>()
            .Update(item);
        _context.SaveChanges();
    }
}
