using System.ComponentModel.DataAnnotations;

namespace Weasel.Audit.Interfaces;

public interface ILongKeyedEntity
{
    [Key]
    public long Id { get; set; }
}
