namespace Weasel.Attributes.Audit.Search;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AuditCustomSearchAttribute : Attribute
{
    public string SearchName { get; set; }
    public string SearchUrlName { get; set; }
    public AuditCustomSearchAttribute(string searchName)
    {
        if (string.IsNullOrEmpty(searchName))
        {
            throw new ArgumentNullException(nameof(searchName));
        }
        SearchName = searchName.ToLower();
        SearchUrlName = searchName;
    }
}
