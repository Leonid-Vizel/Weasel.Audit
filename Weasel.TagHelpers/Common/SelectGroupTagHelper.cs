using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using Weasel.Tools.Extensions.Common;

namespace Weasel.TagHelpers.Common;

[HtmlTargetElement("select-group")]
public sealed class SelectGroupTagHelper : TagHelper
{
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = null!;
    [HtmlAttributeName("asp-items")]
    public IEnumerable<SelectListItem> Items { get; set; } = null!;
    [HtmlAttributeNotBound]
    private IHtmlGenerator Generator { get; set; }
    public SelectGroupTagHelper(IHtmlGenerator generator)
    {
        Generator = generator;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (For == null)
        {
            throw new ArgumentNullException(nameof(For));
        }
        if (Generator == null)
        {
            throw new ArgumentNullException(nameof(Generator));
        }
        if (ViewContext == null)
        {
            throw new ArgumentNullException(nameof(ViewContext));
        }
        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "div";
        output.Content.AppendHtml(await GenerateLabel());
        output.Content.AppendHtml(await GenerateSelect(output.Attributes));
        output.Content.AppendHtml(await GenerateSpan());
        output.Attributes.Clear();
        output.AddClass("mb-1", HtmlEncoder.Default);
    }

    private async Task<IHtmlContent> GenerateLabel()
    {
        LabelTagHelper label = new LabelTagHelper(Generator)
        {
            For = For,
            ViewContext = ViewContext,
        };
        var context = TagHelperExtensions.CreateEmptyContext();
        var output = TagHelperExtensions.CreateEmptyOutput("label");
        output.AddClass("form-label", HtmlEncoder.Default);
        await label.ProcessAsync(context, output);
        return output;
    }

    private async Task<IHtmlContent> GenerateSelect(TagHelperAttributeList attributes)
    {
        SelectTagHelper select = new SelectTagHelper(Generator)
        {
            For = For,
            ViewContext = ViewContext,
            Items = Items,
        };
        var context = TagHelperExtensions.CreateEmptyContext();
        var output = TagHelperExtensions.CreateEmptyOutput(attributes, "select");
        output.AddClass("form-select", HtmlEncoder.Default);
        select.Init(context);
        await select.ProcessAsync(context, output);
        return output;
    }

    private async Task<IHtmlContent> GenerateSpan()
    {
        ValidationMessageTagHelper span = new ValidationMessageTagHelper(Generator)
        {
            For = For,
            ViewContext = ViewContext,
        };
        var context = TagHelperExtensions.CreateEmptyContext();
        var output = TagHelperExtensions.CreateEmptyOutput("span");
        output.AddClass("text-danger", HtmlEncoder.Default);
        await span.ProcessAsync(context, output);
        return output;
    }
}