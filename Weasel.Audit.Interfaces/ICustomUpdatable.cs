using Microsoft.EntityFrameworkCore;

namespace ECRF.Tools.Actions.Interfaces;

public interface ICustomUpdatable<T>
{
    void Update(T entity, DbContext context);
}