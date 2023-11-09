using System.ComponentModel.DataAnnotations;

namespace Weasel.Audit.Interfaces;

public interface IGuidKeyedEntity
{
    [Key]
    Guid Id { get; set; }
}
