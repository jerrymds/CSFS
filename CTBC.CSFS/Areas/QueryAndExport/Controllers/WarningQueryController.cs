using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class WarningQueryController : AppController
    {
        WarningQueryBIZ wqBiz = new WarningQueryBIZ();
        // GET: /QueryAndExport/WarningQuery/
        [RootPageFilter]
        public ActionResult Index()
        {
            WarningQuery model = new WarningQuery();
            //model.ForCDateS = UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd"));
            //model.ForCDateE = UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd"));

            IList<PARMCode> list = wqBiz.GetCodeData("ExportRoleList");
            if (list != null && list.Any())
            {
                string[] ary = { };
                var obj = list.FirstOrDefault();
                if (obj != null && !string.IsNullOrEmpty(obj.CodeMemo))
                {
                    ary = obj.CodeMemo.Split(';');
                }
                if (ary.Length > 0 && LogonUser.Roles.Any() && LogonUser.Roles.Any(m => ary.Contains(m.RoleLDAPId)))
                    ViewBag.CanExport = "1";
            }
            BindDDl();
            return View(model);
        }
        public void BindDDl()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="新增", Value = "1"},
                new SelectListItem() {Text ="撤銷", Value = "5"},
                new SelectListItem() {Text ="修改", Value = "4"},
            };
            ViewBag.TypeList = list;
            List<SelectListItem> Originallist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="Y", Value = "Y"},
                new SelectListItem() {Text ="N", Value = "N"},
            };
            ViewBag.OriginalList = Originallist;
            List<SelectListItem> Kindlist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="通報聯徵", Value = "通報聯徵"},
                new SelectListItem() {Text ="解除", Value = "解除"},
                new SelectListItem() {Text ="修改通報", Value = "修改通報"},
                new SelectListItem() {Text ="ID變更", Value = "ID變更"},
                new SelectListItem() {Text ="延長", Value = "延長"},
            };
            ViewBag.KindList = Kindlist;
        }
        /// <summary>
        /// 結果列表
        /// </summary>
        public ActionResult _QueryResult(WarningQuery model)
        {
            return PartialView("_QueryResult", SearchList(model));
        }

        /// <summary>
        /// 實際查詢動作
        /// </summary>
        public WarningQueryViewModel SearchList(WarningQuery model)
        {
            string CustId = "";
            // 新增個資LOG
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            #region Cookie
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("CustId", model.CustId);
            modelCookie.Values.Add("Kind", model.Kind);
            modelCookie.Values.Add("No_165", model.No_165);
            modelCookie.Values.Add("CustAccount", model.CustAccount);
            modelCookie.Values.Add("DocNo", model.DocNo);
            modelCookie.Values.Add("VictimName", model.VictimName);
            modelCookie.Values.Add("ForCDateS", model.ForCDateS);
            modelCookie.Values.Add("ForCDateE", model.ForCDateE);
            modelCookie.Values.Add("StateType", model.StateType);
            modelCookie.Values.Add("RelieveDateS", model.RelieveDateS);
            modelCookie.Values.Add("RelieveDateE", model.RelieveDateE);
            modelCookie.Values.Add("Original", model.Original);
            modelCookie.Values.Add("ModifyDateS", model.ModifyDateS);
            modelCookie.Values.Add("ModifyDateE", model.ModifyDateE);
            Response.Cookies.Add(modelCookie);
            #endregion
            model.ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
            model.ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);
            //Add by zhangwei 20180315 start
            model.RelieveDateS = UtlString.FormatDateTwStringToAd(model.RelieveDateS);
            model.RelieveDateE = UtlString.FormatDateTwStringToAd(model.RelieveDateE);
            model.ModifyDateS = UtlString.FormatDateTwStringToAd(model.ModifyDateS);
            model.ModifyDateE = UtlString.FormatDateTwStringToAd(model.ModifyDateE);
            //Add by zhangwei 20180315 end
            IList<WarningQuery> result = wqBiz.GetQueryList(model);
            if (result.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    if (!String.IsNullOrEmpty(result[i].CustId))
                    {
                        CustId = result[i].CustId.Trim();
                        _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "DOCNO=" + result[i].DocNo, CustId);
                    }
                }
            }

            var viewModel = new WarningQueryViewModel()
            {
                WarningQuery = model,
                WarningQueryList = result,
            };

            return viewModel;
        }

        public ActionResult Excel(WarningQuery model)
        {

            //model.ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
            //model.ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);

            //MemoryStream ms = new MemoryStream();
            //string fileName = string.Empty;
            //ms = wqBiz.ParmCodeExcel_NPOI(model);
            HttpCookie cookies = Request.Cookies.Get("QueryCookie");
            if (cookies != null)
            {
                if (cookies.Values["CustId"] != null) model.CustId = cookies.Values["CustId"];
                if (cookies.Values["CustAccount"] != null) model.CustAccount = cookies.Values["CustAccount"];
                if (cookies.Values["Kind"] != null)  model.Kind= cookies.Values["Kind"];
                if (cookies.Values["No_165"] != null) model.No_165 = cookies.Values["No_165"];
                if (cookies.Values["DocNo"] != null) model.DocNo = cookies.Values["DocNo"];
                if (cookies.Values["VictimName"] != null) model.VictimName = cookies.Values["VictimName"];
                if (cookies.Values["ForCDateS"] != null) model.ForCDateS = cookies.Values["ForCDateS"];
                if (cookies.Values["ForCDateE"] != null) model.ForCDateE = cookies.Values["ForCDateE"];
                if (cookies.Values["StateType"] != null) model.StateType = cookies.Values["StateType"];
                if (cookies.Values["RelieveDateS"] != null) model.RelieveDateS = cookies.Values["RelieveDateS"];
                if (cookies.Values["RelieveDateE"] != null) model.RelieveDateE = cookies.Values["RelieveDateE"];
                if (cookies.Values["Original"] != null) model.Original = cookies.Values["Original"];
                if (cookies.Values["ModifyDateS"] != null) model.ModifyDateS = cookies.Values["ModifyDateS"];
                if (cookies.Values["ModifyDateE"] != null) model.ModifyDateE = cookies.Values["ModifyDateE"];
            }
            model.ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
            model.ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);
            //Add by zhangwei 20180315 start
            model.RelieveDateS = UtlString.FormatDateTwStringToAd(model.RelieveDateS);
            model.RelieveDateE = UtlString.FormatDateTwStringToAd(model.RelieveDateE);
            model.ModifyDateS = UtlString.FormatDateTwStringToAd(model.ModifyDateS);
            model.ModifyDateE = UtlString.FormatDateTwStringToAd(model.ModifyDateE);
 
            MemoryStream ms = new MemoryStream();
            ms = wqBiz.ParmCodeExcel_NPOI(model);
            string fileName = string.Empty;
            fileName = Lang.csfs_menu_tit_warningquery + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            if (ms != null && ms.Length > 0)
            {
                Response.ClearContent();
                Response.ClearHeaders();
            }
            else
            {
                ms = new MemoryStream();
            }
            return File(ms.ToArray(), "application/vnd.ms-excel", fileName);
        }

        public void BindWarningStatusList()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            ViewBag.StatusList = new SelectList(parm.GetCodeData("WarningStatus"), "CodeNo", "CodeDesc");
        }

        public ActionResult Details(string DocNo, int SerialID)
        {
            ViewBag.DocNo = DocNo;
            ViewBag.SerialID = SerialID;
            BindWarningStatusList();
            CaseWarningBIZ warn = new CaseWarningBIZ();
            if (SerialID > 0)
            {
                CaseWarningViewModel viewmodel = new CaseWarningViewModel()
                {
                    WarningMaster = warn.GetWarnMasterListByDocNo(DocNo),
                    WarningAttachmentList = warn.GetWarnAttatchmentList(DocNo),
                    WarningStateList = warn.GetWarnStateQueryList(DocNo),
                    WarningDetailsList = warn.WarningDetailsSingleList(SerialID)
                };
                return PartialView("Details", viewmodel);
            }
            else
            {
                CaseWarningViewModel viewmodel = new CaseWarningViewModel()
                {
                    WarningMaster = warn.GetWarnMasterListByDocNo(DocNo),
                    WarningAttachmentList = warn.GetWarnAttatchmentList(DocNo),
                    WarningStateList = warn.GetWarnStateQueryList(DocNo),
                    WarningDetailsList = warn.WarningDetailsSearchList(DocNo)
                };
                return PartialView("Details", viewmodel);
            }

        }
        //public ActionResult Details(string DocNo )
        //{
        //    ViewBag.DocNo = DocNo;
        //    BindWarningStatusList();
        //    CaseWarningBIZ warn = new CaseWarningBIZ();

        //    CaseWarningViewModel viewmodel = new CaseWarningViewModel()
        //    {
        //        WarningMaster = warn.GetWarnMasterListByDocNo(DocNo),
        //        WarningAttachmentList = warn.GetWarnAttatchmentList(DocNo),
        //        WarningStateList = warn.GetWarnStateQueryList(DocNo),
        //        WarningDetailsList = warn.WarningDetailsSearchList(DocNo)
        //    };
        //    return PartialView("Details", viewmodel);
        //}
	}
}