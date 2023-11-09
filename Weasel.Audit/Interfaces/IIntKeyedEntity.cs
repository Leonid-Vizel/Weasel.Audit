using System.ComponentModel.DataAnnotations;

namespace Weasel.Audit.Interfaces;

public interface IIntKeyedEntity
{
    [Key]
    int Id { get; set; }
}
