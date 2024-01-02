using System.ComponentModel.DataAnnotations;

namespace Weasel.Audit.Example;

public enum Journal
{
    [Display(Name = "Неизвестный")]
    Unknown,
    [Display(Name = "Пользователи")]
    WebUsers,
    [Display(Name = "Списки ключевых слов")]
    KeyWordLists,
    [Display(Name = "Источники мониторинга")]
    MonitoringSource,
    [Display(Name = "Проекты мониторинга")]
    MonitoringProject,
}
