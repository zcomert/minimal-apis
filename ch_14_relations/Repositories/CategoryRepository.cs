using Entities;

namespace Repositories;

public class CategoryRepository : RepositoryBase<Category>
{
    public CategoryRepository(RepositoryContext context) : base(context)
    {
    }
}