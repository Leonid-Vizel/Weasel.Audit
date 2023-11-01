using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Weasel.Tools.Extensions.Common;

public static class TagHelperExtensions
{
    public static TagHelperOutput CreateEmptyOutput(string tag = "")
    {
        var attrs = new TagHelperAttributeList();
        return new TagHelperOutput(tag, attrs, (useCachedResult, encoder) => Task.Factory.StartNew<TagHelperContent>(() => new DefaultTagHelperContent()));
    }

    public static TagHelperOutput CreateEmptyOutput(TagHelperAttributeList attributes, string tag = "")
    {
        var attrs = new TagHelperAttributeList(attributes.ToList());
        return new TagHelperOutput(tag, attrs, (useCachedResult, encoder) => Task.Factory.StartNew<TagHelperContent>(() => new DefaultTagHelperContent()));
    }

    public static TagHelperContext CreateEmptyContext()
    {
        var attrs = new TagHelperAttributeList();
        var items = new Dictionary<object, object>();
        var id = Guid.NewGuid().ToString();
        return new TagHelperContext(attrs, items, id);
    }

    public static TagHelperContext CreateEmptyContext(IReadOnlyCollection<TagHelperAttribute> attributes)
    {
        var attrs = new TagHelperAttributeList(attributes);
        var items = new Dictionary<object, object>();
        var id = Guid.NewGuid().ToString();
        return new TagHelperContext(attrs, items, id);
    }
}
