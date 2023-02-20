using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Resource;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Areas.SystemManagement.Controllers
{
    public class WorkingDayController : AppController
    {
        PARMWorkingDayBIZ WKDayBiz;
        public WorkingDayController()
        {
            WKDayBiz = new PARMWorkingDayBIZ();
        }

        // GET: /SystemManagement/WorkingDay/
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

        //* 點擊儲存
        [HttpPost]
        public ActionResult Index(PARMWorkingDayVO model)
        {
            try
            {
                int rtn = WKDayBiz.Save(model.CkBox, model.CurrentDate);
                return Content("");
            }
            catch (Exception ex)
            {
                throw new CSFSException(model.CurrentDate.ToString("yyyy" + Lang.csfs_year + "MM" + Lang.csfs_month) + Lang.csfs_wkday + Lang.csfs_settings + Lang.csfs_data + Lang.csfs_save + Lang.csfs_fail + " " + ex.Message, null);
            }

        }

        //*點擊日期改變得到日期
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