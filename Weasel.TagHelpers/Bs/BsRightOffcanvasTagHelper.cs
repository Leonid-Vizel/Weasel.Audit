using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Weasel.TagHelpers.Bs;

[HtmlTargetElement("bs-right-offcanvas")]
public sealed class BsRightOffcanvasTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string Title { get; set; } = null!;
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("offcanvas", HtmlEncoder.Default);
        output.AddClass("offcanvas-end", HtmlEncoder.Default);
        output.Attributes.Add("tabindex", "-1");
        output.Attributes.Add("id", "offcanvas");

        TagBuilder modalHeaderTitleBuilder = new TagBuilder("h5");
        modalHeaderTitleBuilder.AddCssClass("offcanvas-title");
        modalHeaderTitleBuilder.MergeAttribute("id", "offcanvas-title");
        modalHeaderTitleBuilder.InnerHtml.Append(Title);

        TagBuilder modalHeaderButtonBuilder = new TagBuilder("button");
        modalHeaderButtonBuilder.AddCssClass("btn-close");
        modalHeaderButtonBuilder.MergeAttribute("type", "button");
        modalHeaderButtonBuilder.MergeAttribute("data-bs-dismiss", "offcanvas");

        TagBuilder modalHeaderBuilder = new TagBuilder("div");
        modalHeaderBuilder.AddCssClass("offcanvas-header");
        modalHeaderBuilder.InnerHtml.AppendHtml(modalHeaderTitleBuilder);
        modalHeaderBuilder.InnerHtml.AppendHtml(modalHeaderButtonBuilder);

        TagBuilder modalBodyBuilder = new TagBuilder("div");
        modalBodyBuilder.AddCssClass("offcanvas-body");
        modalBodyBuilder.MergeAttribute("id", "offcanvas-body");
        var passedContent = await output.GetChildContentAsync();
        modalBodyBuilder.InnerHtml.AppendHtml(passedContent);
        output.Content.Clear();
        output.Content.AppendHtml(modalHeaderBuilder);
        output.Content.AppendHtml(modalBodyBuilder);
    }
}
