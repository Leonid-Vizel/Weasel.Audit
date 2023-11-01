using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using Weasel.Tools.Extensions.Common;

namespace Weasel.TagHelpers.Common;

[HtmlTargetElement("check-group")]
public sealed class CheckBoxGroupTagHelper : TagHelper
{
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = null!;
    [HtmlAttributeNotBound]
    private IHtmlGenerator Generator { get; set; }
    [HtmlAttributeName("onchange")]
    public string? OnChange { get; set; } = null!;
    public CheckBoxGroupTagHelper(IHtmlGenerator generator)
    {
        Generator = generator;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (For == null)
        {
            throw new ArgumentNullException(nameof(For));
        }
        if (ViewContext == null)
        {
            throw new ArgumentNullException(nameof(ViewContext));
        }
        if (Generator == null)
        {
            throw new ArgumentNullException(nameof(Generator));
        }
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("mb-1", HtmlEncoder.Default);
        TagBuilder checkGroup = new TagBuilder("div");
        checkGroup.AddCssClass("form-check");
        checkGroup.InnerHtml.AppendHtml(await GenerateInput());
        checkGroup.InnerHtml.AppendHtml(await GenerateLabel());
        output.Content.AppendHtml(checkGroup);
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
        output.AddClass("form-check-label", HtmlEncoder.Default);
        await label.ProcessAsync(context, output);
        return output;
    }

    private async Task<IHtmlContent> GenerateInput()
    {
        InputTagHelper input = new InputTagHelper(Generator)
        {
            For = For,
            ViewContext = ViewContext,
        };
        var attrs = new List<TagHelperAttribute>();
        if (OnChange != null)
        {
            attrs.Add(new TagHelperAttribute("onchange", OnChange));
        }
        var attrData = new TagHelperAttributeList(attrs);
        var context = TagHelperExtensions.CreateEmptyContext();
        var output = TagHelperExtensions.CreateEmptyOutput(attrData, "input");
        output.AddClass("form-check-input", HtmlEncoder.Default);
        output.Attributes.Add("type", "checkbox");
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