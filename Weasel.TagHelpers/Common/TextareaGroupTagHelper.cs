using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using Weasel.Tools.Extensions.Common;

namespace Weasel.TagHelpers.Common;

[HtmlTargetElement("textarea-group")]
public sealed class TextareaGroupTagHelper : TagHelper
{
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = null!;
    [HtmlAttributeName("rows")]
    public ushort Rows { get; set; } = 3;
    [HtmlAttributeNotBound]
    private IHtmlGenerator Generator { get; set; }
    public TextareaGroupTagHelper(IHtmlGenerator generator)
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
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("mb-1", HtmlEncoder.Default);
        output.Content.AppendHtml(await GenerateLabel());
        output.Content.AppendHtml(await GenerateTextArea());
        output.Content.AppendHtml(await GenerateSpan());
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

    private async Task<IHtmlContent> GenerateTextArea()
    {
        TextAreaTagHelper input = new TextAreaTagHelper(Generator)
        {
            For = For,
            ViewContext = ViewContext,
        };
        var context = TagHelperExtensions.CreateEmptyContext();
        var output = TagHelperExtensions.CreateEmptyOutput("textarea");
        output.Attributes.Add("row", Rows);
        output.AddClass("form-control", HtmlEncoder.Default);
        await input.ProcessAsync(context, output);
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