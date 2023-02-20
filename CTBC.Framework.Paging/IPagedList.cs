/// <summary>
/// 程式說明:分頁共用模組
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System.Collections.Generic;

namespace CTBC.FrameWork.Paging
{
	public interface IPagedList<T> : IList<T>
	{
		int PageCount { get; }
		int TotalItemCount { get; }
		int PageIndex { get; }
		int PageNumber { get; }
		int PageSize { get; }
		bool HasPreviousPage { get; }
		bool HasNextPage { get; }
		bool IsFirstPage { get; }
		bool IsLastPage { get; }
	}
}