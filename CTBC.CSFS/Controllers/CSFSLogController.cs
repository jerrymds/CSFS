/// <summary>
/// 程式說明:CSFSLog Controller - Log
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.ViewModels;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.Controllers
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
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,

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
        //20150216弱掃
        [HttpPost]
        public ActionResult _QueryResult(CSFSLogVO csfsLogVO, int pageNum = 1)
        {
            return View("_QueryResult", GetCSFSLogList(csfsLogVO, pageNum));
        }

        /// <summary>
        /// 取得Log資料(分頁)
        /// </summary>
        /// <param name="parmSchVO"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public CSFSLogViewModel GetCSFSLogList(CSFSLogVO csfsLogVO, int pageNum = 1)
        {
            CSFSLogViewModel viewModel;

            //取出符合條件的Log資訊
            IList<CSFSLogVO> result = _CSFSLogBIZ.GetQueryList(csfsLogVO, pageNum);

            viewModel = new CSFSLogViewModel()
            {
                CSFSLogVO = csfsLogVO,
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
