/// <summary>
/// 程式說明:分頁共用模組
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Routing;

namespace CTBC.FrameWork.Paging
{
    public static class PagingExtensions
    {
        #region AjaxHelper extensions

        public static HtmlString Pager(this AjaxHelper ajaxHelper, int pageSize, int currentPage, int totalItemCount, AjaxOptions ajaxOptions)
        {
            return Pager(ajaxHelper, pageSize, currentPage, totalItemCount, null, null, ajaxOptions);
        }

        public static HtmlString Pager(this AjaxHelper ajaxHelper, int pageSize, int currentPage, int totalItemCount, string actionName, AjaxOptions ajaxOptions)
        {
            return Pager(ajaxHelper, pageSize, currentPage, totalItemCount, actionName, null, ajaxOptions);
        }

        public static HtmlString Pager(this AjaxHelper ajaxHelper, int pageSize, int currentPage, int totalItemCount, object values, AjaxOptions ajaxOptions, string countText, string PageText, string IndexText, string sortExpression, string sortDirection,string extraFunc="")
        {
            return Pager(ajaxHelper, pageSize, currentPage, totalItemCount, null, new RouteValueDictionary(values), ajaxOptions, countText, PageText, IndexText, sortExpression, sortDirection, extraFunc);
        }

        public static HtmlString Pager(this AjaxHelper ajaxHelper, int pageSize, int currentPage, int totalItemCount, string actionName, object values, AjaxOptions ajaxOptions)
        {
            return Pager(ajaxHelper, pageSize, currentPage, totalItemCount, actionName, new RouteValueDictionary(values), ajaxOptions);
        }

        public static HtmlString Pager(this AjaxHelper ajaxHelper, int pageSize, int currentPage, int totalItemCount, RouteValueDictionary valuesDictionary, AjaxOptions ajaxOptions)
        {
            return Pager(ajaxHelper, pageSize, currentPage, totalItemCount, null, valuesDictionary, ajaxOptions);
        }

        public static HtmlString Pager(this AjaxHelper ajaxHelper, int pageSize, int currentPage, int totalItemCount, string actionName, RouteValueDictionary valuesDictionary
            , AjaxOptions ajaxOptions, string countText, string PageText, string IndexText, string sortExpression, string sortDirection, string extraFunc)
        {
            if (valuesDictionary == null)
            {
                valuesDictionary = new RouteValueDictionary();
            }
            if (actionName != null)
            {
                if (valuesDictionary.ContainsKey("action"))
                {
                    throw new ArgumentException("The valuesDictionary already contains an action.", "actionName");
                }
                valuesDictionary.Add("action", actionName);
            }
            var pager = new Pager(ajaxHelper.ViewContext, pageSize, currentPage, totalItemCount, valuesDictionary, ajaxOptions
                , countText, PageText, IndexText, sortExpression, sortDirection, extraFunc);
            return pager.RenderHtml();
        }

        #endregion

        #region HtmlHelper extensions

        public static HtmlString Pager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount)
        {
            return Pager(htmlHelper, pageSize, currentPage, totalItemCount, null, null);
        }

        public static HtmlString Pager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, string actionName)
        {
            return Pager(htmlHelper, pageSize, currentPage, totalItemCount, actionName, null);
        }

        public static HtmlString Pager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, object values)
        {
            return Pager(htmlHelper, pageSize, currentPage, totalItemCount, null, new RouteValueDictionary(values));
        }

        public static HtmlString Pager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, string actionName, object values)
        {
            return Pager(htmlHelper, pageSize, currentPage, totalItemCount, actionName, new RouteValueDictionary(values));
        }

        public static HtmlString Pager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, RouteValueDictionary valuesDictionary)
        {
            return Pager(htmlHelper, pageSize, currentPage, totalItemCount, null, valuesDictionary);
        }

        public static HtmlString Pager(this HtmlHelper htmlHelper, int pageSize, int currentPage, int totalItemCount, string actionName, RouteValueDictionary valuesDictionary, string countText, string PageText, string IndexText, string sortExpression, string sortDirection)
        {
            if (valuesDictionary == null)
            {
                valuesDictionary = new RouteValueDictionary();
            }
            if (actionName != null)
            {
                if (valuesDictionary.ContainsKey("action"))
                {
                    throw new ArgumentException("The valuesDictionary already contains an action.", "actionName");
                }
                valuesDictionary.Add("action", actionName);
            }
            var pager = new Pager(htmlHelper.ViewContext, pageSize, currentPage, totalItemCount, valuesDictionary, null, countText, PageText, IndexText, sortExpression, sortDirection);
            return pager.RenderHtml();
        }

        #endregion

        #region IQueryable<T> extensions

        public static IPagedList<T> ToPagedList<T>(this IQueryable<T> source, int pageIndex, int pageSize, int? totalCount = null)
        {
            return new PagedList<T>(source, pageIndex, pageSize, totalCount);
        }

        #endregion

        #region IEnumerable<T> extensions

        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount = null)
        {
            return new PagedList<T>(source, pageIndex, pageSize, totalCount);
        }

        #endregion
    }
}