using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Interfaces;

public interface ICustomUpdatable<T>
{
    void Update(T entity, DbContext context);
}