using CTBC.CSFS.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using System.IO;
using System.Data;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class DeptAccessQueryController : AppController
    {
        // GET: QueryAndExport/DeptAccessQuery

        [RootPageFilter]
        public ActionResult Index(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                date =UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"));//初始化時顯示為當天日期
            }
            DeptAccessQuery model = new DeptAccessQuery();
            model.AccessDataS = date;
            model.AccessDataE = date;
            return View(model);
        }
        public ActionResult GetTxt(DeptAccessQuery model)
        {
            DeptAccessQueryBIZ DAQ = new DeptAccessQueryBIZ();
            DataTable dt = DAQ.GetData(UtlString.FormatDateTwStringToAd(model.AccessDataS), UtlString.FormatDateTwStringToAd(model.AccessDataE));
            MemoryStream ms = DAQ.bianma(dt);
            string fileName = Lang.csfs_query_send_list+"_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            return File(ms.ToArray(), "text/plain", fileName);
        }
    }
}