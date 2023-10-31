using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Weasel.TagHelpers.Bs;

[HtmlTargetElement("bs-modal")]
public sealed class BsModalTagHelper : TagHelper
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("modal", HtmlEncoder.Default);
        output.AddClass("fade", HtmlEncoder.Default);
        output.Attributes.Add("id", "Modal");
        output.Attributes.Add("data-bs-backdrop", "static");
        output.Attributes.Add("data-bs-keyboard", "false");
        output.Attributes.Add("tabindex", "-1");
        output.Attributes.Add("aria-hidden", "false");

        TagBuilder modalContentBuilder = new TagBuilder("div");
        modalContentBuilder.AddCssClass("modal-content");
        modalContentBuilder.InnerHtml.AppendHtml(GenerateHeader());
        modalContentBuilder.InnerHtml.AppendHtml(GenerateBody());

        TagBuilder modalDialogBuilder = new TagBuilder("div");
        modalDialogBuilder.AddCssClass("modal-dialog");
        modalDialogBuilder.AddCssClass("modal-xl");
        modalDialogBuilder.AddCssClass("modal-dialog-centered");
        modalDialogBuilder.AddCssClass("modal-dialog-scrollable");
        modalDialogBuilder.InnerHtml.AppendHtml(modalContentBuilder);

        output.Content.AppendHtml(modalDialogBuilder);

        return Task.CompletedTask;
    }

    public IHtmlContent GenerateHeader()
    {
        TagBuilder modalTitleBuilder = new TagBuilder("h1");
        modalTitleBuilder.AddCssClass("modal-title");
        modalTitleBuilder.AddCssClass("fs-5");
        modalTitleBuilder.MergeAttribute("id", "ModalHeader");

        TagBuilder modalCloseButtonBuilder = new TagBuilder("button");
        modalCloseButtonBuilder.AddCssClass("btn-close");
        modalCloseButtonBuilder.MergeAttribute("type", "button");
        modalCloseButtonBuilder.MergeAttribute("data-bs-dismiss", "modal");

        TagBuilder modalHeaderBuilder = new TagBuilder("div");
        modalHeaderBuilder.AddCssClass("modal-header");
        modalHeaderBuilder.InnerHtml.AppendHtml(modalTitleBuilder);
        modalHeaderBuilder.InnerHtml.AppendHtml(modalCloseButtonBuilder);

        return modalHeaderBuilder;
    }

    public IHtmlContent GenerateBody()
    {
        TagBuilder spinnerHiddenSpanBuilder = new TagBuilder("span");
        spinnerHiddenSpanBuilder.AddCssClass("visually-hidden");
        spinnerHiddenSpanBuilder.MergeAttribute("role", "status");
        spinnerHiddenSpanBuilder.InnerHtml.Append("Loading...");

        TagBuilder spinnerBorderBuilder = new TagBuilder("div");
        spinnerBorderBuilder.AddCssClass("spinner-border");
        spinnerBorderBuilder.AddCssClass("text-primary");
        spinnerBorderBuilder.MergeAttribute("role", "status");
        spinnerBorderBuilder.InnerHtml.AppendHtml(spinnerHiddenSpanBuilder);

        TagBuilder spinnerBuilder = new TagBuilder("div");
        spinnerBuilder.AddCssClass("d-flex");
        spinnerBuilder.AddCssClass("justify-content-center");
        spinnerBuilder.MergeAttribute("id", "spinner");
        spinnerBuilder.InnerHtml.AppendHtml(spinnerBorderBuilder);

        TagBuilder modalBodyBuilder = new TagBuilder("div");
        modalBodyBuilder.AddCssClass("modal-body");
        modalBodyBuilder.MergeAttribute("id", "ModalBody");
        modalBodyBuilder.InnerHtml.AppendHtml(spinnerBuilder);

        return modalBodyBuilder;
    }
}
