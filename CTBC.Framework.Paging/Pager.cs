/// <summary>
/// 程式說明:分頁共用模組
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>


using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Routing;

namespace CTBC.FrameWork.Paging
{
    public class Pager
    {
       
        private ViewContext viewContext;
        private readonly int pageSize;
        private readonly int currentPage;
        private readonly int totalItemCount;
        private readonly RouteValueDictionary linkWithoutPageValuesDictionary;
        private readonly AjaxOptions ajaxOptions;
        private readonly string countText;
        private readonly string PageText;
        private readonly string IndexText;
        private readonly string memo;
        private readonly string sortExpression;
        private readonly string sortDirection;
        private readonly string extrFunc; //通過下拉分頁時要激發額外的js函數

        public Pager(ViewContext viewContext, int pageSize, int currentPage, int totalItemCount, RouteValueDictionary valuesDictionary
            , AjaxOptions ajaxOptions, string countText, string PageText, string IndexText, string sortExpression, string sortDirection, string extrFunc = "")
        {
            this.viewContext = viewContext;
            this.pageSize = pageSize;
            this.currentPage = currentPage;
            this.totalItemCount = totalItemCount;
            this.linkWithoutPageValuesDictionary = valuesDictionary;
            this.ajaxOptions = ajaxOptions;
            this.countText = countText;
            this.PageText = PageText;
            this.IndexText = IndexText;
            this.sortExpression = sortExpression;
            this.sortDirection = sortDirection;
            this.extrFunc = extrFunc;
        }

        public HtmlString RenderHtml()
        {
            var pageCount = 0;

            if (totalItemCount != 0 && pageSize != 0)
            {
                pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);
            }
            var sb = new StringBuilder("");
            sb.Append("<div class='Paging'><table><tr><td>");
            sb.Append(currentPage > 1 ? GeneratePageLink("class='but Paging Paging_first'", 1) : string.Format("<a href='#' disabled='disabled' class='but Paging Paging_first'></a>"));
            sb.Append(currentPage > 1 ? GeneratePageLink("class='but Paging'", currentPage - 1) : string.Format("<a href='#' disabled='disabled' class='but Paging'></a>"));
            sb.Append("</td><td><select id='txtPageNum' name='txtPageNum' onchange=\"CkPageNumberDiv(event, '" + ajaxOptions.UpdateTargetId + "', '" + this.sortExpression + "', '" + this.sortDirection + "','" + extrFunc + "');\" >");
            string selectoption = "";
            for (int i = 1; i <= pageCount; i++)
            {
                if (i == currentPage)
                {
                    selectoption += "<option selected='selected'>" + i + "</option>";
                }
                else
                {
                    selectoption += "<option>" + i + "</option>";
                }
            }
            sb.Append(selectoption);
            sb.Append("</select></td><td>");
            sb.Append(currentPage < pageCount ? GeneratePageLink("class='but Paging Paging_right'", (currentPage + 1)) : string.Format("<a href='#'  disabled='disabled' class='but Paging Paging_right'></a>"));
            sb.Append(currentPage < pageCount ? GeneratePageLink("class='but Paging Paging_last'", pageCount) : string.Format("<a href='#'  disabled='disabled' class='but Paging Paging_last'></a>"));
            sb.Append("</td><td>");
            sb.Append(" | ");
            sb.Append(this.PageText.Replace("0", pageCount.ToString()) + " / " + this.IndexText.Replace("0", totalItemCount.ToString()));
            sb.Append(" | ");
            sb.Append("</td></tr></table></div>");
            sb.Append(" ");
            sb.Append(GenerateQueryCriteriaPath());
            return new HtmlString(sb.ToString());
        }

        private string GeneratePageLink(string linkText, int pageNumber)
        {
            var pageLinkValueDictionary = new RouteValueDictionary(linkWithoutPageValuesDictionary) { { "pageNum", pageNumber }, { "strSortExpression", this.sortExpression }, { "strSortDirection", this.sortDirection } };
            var virtualPathForArea = RouteTable.Routes.GetVirtualPathForArea(viewContext.RequestContext, pageLinkValueDictionary);

            if (virtualPathForArea == null)
                return null;
            virtualPathForArea.VirtualPath = virtualPathForArea.VirtualPath + "&radom_xx=" + DateTime.Now.ToString("yyyyMMddhhmmssffff");
            var stringBuilder = new StringBuilder("<a " + linkText);

            if (ajaxOptions != null)
                foreach (var ajaxOption in ajaxOptions.ToUnobtrusiveHtmlAttributes())
                    stringBuilder.AppendFormat(" {0}=\"{1}\"", ajaxOption.Key, ajaxOption.Value);

            stringBuilder.AppendFormat(" href=\"{0} \"></a>", virtualPathForArea.VirtualPath);

            return stringBuilder.ToString();
        }

        private string GenerateQueryCriteriaPath()
        {
            var virtualPathForArea = RouteTable.Routes.GetVirtualPathForArea(viewContext.RequestContext, linkWithoutPageValuesDictionary);

            if (virtualPathForArea == null)
                return null;

            var stringBuilder = new StringBuilder("<input type=\"hidden\" id=\"hidQueryCriteriaPath\"");

            stringBuilder.AppendFormat(" value=\"{0}\"></input>", virtualPathForArea.VirtualPath);

            return stringBuilder.ToString();
        }
    }
}