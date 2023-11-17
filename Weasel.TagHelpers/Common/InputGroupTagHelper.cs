using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using Weasel.Tools.Extensions.Common;

namespace Weasel.TagHelpers.Common;

[HtmlTargetElement("input-group")]
public sealed class InputGroupTagHelper : TagHelper
{
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; } = null!;
    [HtmlAttributeName("div-id")]
    public string? DivId { get; set; }
    [HtmlAttributeName("div-name")]
    public string? DivName { get; set; }
    [HtmlAttributeName("type")]
    public string? Type { get; set; }
    [HtmlAttributeName("autocomplete")]
    public string? Autocomplete { get; set; }
    [HtmlAttributeName("hidden")]
    public bool Hidden { get; set; } = false;
    [HtmlAttributeNotBound]
    private IHtmlGenerator Generator { get; set; }
    public InputGroupTagHelper(IHtmlGenerator generator)
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
        if (DivId != null)
        {
            output.Attributes.Add("id", DivId);
        }
        if (DivName != null)
        {
            output.Attributes.Add("name", DivName);
        }
        if (Hidden)
        {
            output.Attributes.Add("hidden", "hidden");
        }
        output.Content.AppendHtml(await GenerateLabel());
        output.Content.AppendHtml(await GenerateInput());
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

    private async Task<IHtmlContent> GenerateInput()
    {
        InputTagHelper input = new InputTagHelper(Generator)
        {
            For = For,
            ViewContext = ViewContext,
        };
        TagHelperContext context;
        var attributes = new List<TagHelperAttribute>();
        if (Type == null)
        {
            if (For.ModelExplorer.ModelType == typeof(DateOnly) || For.ModelExplorer.ModelType == typeof(DateOnly?))
            {
                Type = "date";
            }
            if (For.ModelExplorer.ModelType == typeof(TimeOnly) || For.ModelExplorer.ModelType == typeof(TimeOnly?))
            {
                Type = "time";
            }
        }
        if (Type != null)
        {
            input.InputTypeName = Type;
            attributes.Add(new TagHelperAttribute("type", Type));
        }
        if (Autocomplete != null)
        {
            attributes.Add(new TagHelperAttribute("autocomplete", Autocomplete));
        }
        var attrList = new TagHelperAttributeList(attributes);
        context = TagHelperExtensions.CreateEmptyContext(attributes);
        var output = TagHelperExtensions.CreateEmptyOutput(attrList, "input");
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