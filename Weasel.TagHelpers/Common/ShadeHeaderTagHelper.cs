using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Weasel.TagHelpers.Common;

[HtmlTargetElement("shade-header")]
public sealed class ShadeHeaderTagHelper : TagHelper
{
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext == null)
        {
            throw new ArgumentNullException(nameof(ViewContext));
        }
        string displayTitle = ViewContext.ViewData["Title"]?.ToString() ?? "";

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("container-fluid", HtmlEncoder.Default);
        output.AddClass("mb-3", HtmlEncoder.Default);

        TagBuilder textBuilder = new TagBuilder("h2");
        textBuilder.AddCssClass("text-center");
        textBuilder.InnerHtml.Append(displayTitle);

        TagBuilder shadowBuilder = new TagBuilder("div");
        shadowBuilder.AddCssClass("shadow");
        shadowBuilder.AddCssClass("p-3");
        shadowBuilder.AddCssClass("mb-3");
        shadowBuilder.AddCssClass("bg-white");
        shadowBuilder.AddCssClass("rounded");
        shadowBuilder.InnerHtml.AppendHtml(textBuilder);

        TagBuilder colBuilder = new TagBuilder("div");
        colBuilder.AddCssClass("col-8");
        colBuilder.AddCssClass("p-0");
        colBuilder.InnerHtml.AppendHtml(shadowBuilder);

        TagBuilder rowBuilder = new TagBuilder("div");
        rowBuilder.AddCssClass("row");
        rowBuilder.AddCssClass("justify-content-center");
        rowBuilder.InnerHtml.AppendHtml(colBuilder);

        output.Content.AppendHtml(rowBuilder);

        return Task.CompletedTask;
    }
}