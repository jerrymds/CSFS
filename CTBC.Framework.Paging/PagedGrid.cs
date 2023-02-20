/// <summary>
/// 程式說明:分頁共用模組
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections;

namespace CTBC.FrameWork.Paging
{
    public class PagedGrid
    {
        /// <summary>
        /// 當前頁
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁中資料的筆數
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 資料總筆數
        /// </summary>
        public int TotalItemCount { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int PageCount
        {
            get
            {
                // 計算方式：總筆數/當前頁的筆數
                // modify by Emily 2011/11/24 添加無數據的判斷
                if (TotalItemCount != 0 && PageSize != 0)
                {
                    return (int)Math.Ceiling(TotalItemCount / (double)PageSize);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 當前含有的頁碼
        /// </summary>
        public ArrayList PageList
        {
            get
            {
                // 定義一個ArrayList為dropDownList加入Option 
                ArrayList list = new ArrayList();

                // 循環為ArrayList賦值
                for (int i = 1; i <= PageCount; i++)
                {
                    list.Add(i);
                }

                return list;
            }
        }
    }
}