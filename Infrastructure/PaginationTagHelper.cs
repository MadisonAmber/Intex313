using Intex313.Models;
using Intex313.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex313.Infrastructure
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PaginationTagHelper : TagHelper
    {
        private IUrlHelperFactory uhf;

        public PaginationTagHelper(IUrlHelperFactory temp)
        {
            uhf = temp;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext vc { get; set; }

        public PageInfo PageModel { get; set; }
        public string PageAction { get; set; }
        public Accident PageFilter { get; set; }

        public override void Process(TagHelperContext thc, TagHelperOutput tho)
        {
            IUrlHelper uh = uhf.GetUrlHelper(vc);

            TagBuilder final = new TagBuilder("div");

            TagBuilder ul = new TagBuilder("ul");
            ul.Attributes["class"] = "pagination justify-content-center";

            if (PageModel.CurrentPage != 1)
            {
                ul.InnerHtml.AppendHtml(createBootstrapStyledListItem(PageModel.CurrentPage - 1, "Previous"));
            }

            ButtonSpan span = getButtonLimits();
            if(span.lowerLimit > 1)
            {
                ul.InnerHtml.AppendHtml(createBootstrapStyledListItem(1, "1..."));
            }

            for (int i = span.lowerLimit; i <= span.upperLimit; i++)
            {
                ul.InnerHtml.AppendHtml(createBootstrapStyledListItem(i, i.ToString()));
            }

            if(span.upperLimit < PageModel.TotalPages)
            {
                ul.InnerHtml.AppendHtml(createBootstrapStyledListItem(PageModel.TotalPages, "..." + PageModel.TotalPages.ToString()));
            }

            if (PageModel.CurrentPage != PageModel.TotalPages)
            {
                ul.InnerHtml.AppendHtml(createBootstrapStyledListItem(PageModel.CurrentPage + 1, "Next"));
            }

            final.InnerHtml.AppendHtml(ul);

            tho.Content.AppendHtml(final.InnerHtml);
        }

        private ButtonSpan getButtonLimits()
        {
            ButtonSpan span = new ButtonSpan
            {
                lowerLimit = 1,
                upperLimit = 2
            };
            int currentPage = PageModel.CurrentPage;
            int totalPages = PageModel.TotalPages;
            int numButtons = PageModel.NumButtons;

            int buttonRange = currentPage - (numButtons / 2) > 1 ? (numButtons / 2) : (numButtons / 2) + Math.Abs(currentPage - (numButtons / 2));

            span.lowerLimit = currentPage - buttonRange;
            span.lowerLimit = span.lowerLimit >= 1 ? span.lowerLimit : 1;

            span.upperLimit = currentPage + buttonRange;
            span.upperLimit = span.upperLimit <= totalPages ? span.upperLimit : totalPages;

            return span;
        }

        private TagBuilder createBootstrapStyledListItem(int currentPageNum, string pageText)
        {
            IUrlHelper uh = uhf.GetUrlHelper(vc);

            TagBuilder tb = new TagBuilder("a");

            tb.Attributes["href"] = uh.Action(PageAction, new { pageNum = currentPageNum, filter = JsonConvert.SerializeObject(PageFilter) });
            tb.Attributes["class"] = "page-link";

            tb.InnerHtml.Append(pageText);

            TagBuilder listItem = new TagBuilder("li");
            listItem.Attributes["class"] = "page-item";
            if (PageModel.CurrentPage == currentPageNum)
            {
                listItem.Attributes["class"] += " active";
            }
            listItem.InnerHtml.AppendHtml(tb);

            return listItem;
        }
    }

    class ButtonSpan
    {
        public int upperLimit { get; set; }
        public int lowerLimit { get; set; }
    }
}
