using System.ComponentModel.DataAnnotations;

namespace Weasel.Audit.Interfaces;

public interface IGiudKeyedEntity
{
    [Key]
    public Guid Id { get; set; }
}
