/// <summary>
/// 程式說明:CSFSLog Controller - Log
/// 維護部門:資訊管理處
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class CSFSLogController : AppController
    {
        CSFSLogBIZ _CSFSLogBIZ;

        public CSFSLogController()
        {
            _CSFSLogBIZ = new CSFSLogBIZ(this);
        }

        /// <summary>
        /// 查詢Log條件頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Query()
        {
            CSFSLogVO model = new CSFSLogVO()
            {
                //預設只查當天Log
                StartTime = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd"))),
                EndTime = Convert.ToDateTime(UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd"))),

                //設定預設值寫法
                //StartTime = DateTime.Now.AddDays(-14),
                //EndTime = Date
            };
            return View(model);
        }

        /// <summary>
        /// 查詢Log資料
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public ActionResult _QueryResult(CSFSLogVO CSFSLogVO, int pageNum = 1)
        {
            return View("_QueryResult", GetCSFSLogList(CSFSLogVO, pageNum));
        }

        /// <summary>
        /// 取得Log資料(分頁)
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public CSFSLogViewModel GetCSFSLogList(CSFSLogVO CSFSLogVO, int pageNum = 1)
        {
            if (CSFSLogVO != null)
            {
                if (CSFSLogVO.StartTime.HasValue)
                    CSFSLogVO.StartTime = UtlString.FormatDateTwStringToAd(CSFSLogVO.StartTime.Value);
                if (CSFSLogVO.EndTime.HasValue)
                    CSFSLogVO.EndTime = UtlString.FormatDateTwStringToAd(CSFSLogVO.EndTime.Value);
            }
            CSFSLogViewModel viewModel;

            //取出符合條件的Log資訊
            IList<CSFSLogVO> result = _CSFSLogBIZ.GetQueryList(CSFSLogVO, pageNum);

            viewModel = new CSFSLogViewModel()
            {
                CSFSLogVO = CSFSLogVO,
                CSFSLogList = result,
            };

            //分頁相關設定
            viewModel.CSFSLogVO.PageSize = _CSFSLogBIZ.PageSize;
            viewModel.CSFSLogVO.CurrentPage = _CSFSLogBIZ.PageIndex;
            viewModel.CSFSLogVO.TotalItemCount = _CSFSLogBIZ.DataRecords;

            return viewModel;
        }

    }
}
