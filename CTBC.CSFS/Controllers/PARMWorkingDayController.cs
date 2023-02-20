/// <summary>
/// 程式說明:PARMWorkingDay Controller - 營業日
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved. 
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;

namespace CTBC.CSFS.Controllers
{
    public class PARMWorkingDayController : AppController
    {
        PARMWorkingDayBIZ WKDayBiz;
        public PARMWorkingDayController()
        {
            WKDayBiz = new PARMWorkingDayBIZ(this);
        }

        public ActionResult Index()
        {
            try
            {
                var viewModel = GetViewModel(DateTime.Now);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult Index(PARMWorkingDayVO model)
        {
            try
            {
                int rtn = WKDayBiz.Save(model.CkBox, model.CurrentDate);
                //var viewModel = GetViewModel(model.CurrentDate);
                return Content(Lang.csfs_edit_ok);
            }
            catch (Exception ex)
            {
                throw new CSFSException(model.CurrentDate.ToString("yyyy" + Lang.csfs_year + "MM" + Lang.csfs_month) + Lang.csfs_wkday + Lang.csfs_settings + Lang.csfs_data + Lang.csfs_save + Lang.csfs_fail + " " + ex.Message, null);
            }

        }

        [HttpPost]
        public ActionResult _Calendar(string dte)
        {
            try
            {
                var viewModel = GetViewModel(Convert.ToDateTime(dte));
                return PartialView("_Calendar", viewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private PARMWorkingDayVO GetViewModel(DateTime dte)
        {
            try
            {
                IEnumerable<PARMWorkingDay> wkDte = WKDayBiz.SelectByMonth(dte);
                //若該月無資料時,則新增該月資料到DB,預設星期六日為非營業日
                if (wkDte.Count() == 0)
                {
                    WKDayBiz.Save(null, dte);
                    wkDte = WKDayBiz.SelectByMonth(dte);
                }
                var viewModel = WKDayBiz.FormatCalendar(wkDte, dte);
                return viewModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
