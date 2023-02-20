using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Areas.QueryAndExport.Controllers
{
    public class WarningAccountQueryController : AppController
    {
        MeetingResultBIZ mResult = new MeetingResultBIZ();
        MeetingResultDetailBIZ dResult = new MeetingResultDetailBIZ();

        WarningAccountQueryBIZ wqBiz = new WarningAccountQueryBIZ();
        // GET: /QueryAndExport/WarningAccountQuery/
        [RootPageFilter]
        public ActionResult Index()
        {
            WarningAccountQuery model = new WarningAccountQuery();

            model.ForCDateS = UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd"));
            model.ForCDateE = UtlString.FormatDateTw(DateTime.Today.ToString("yyyy/MM/dd"));

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
            // adam // 金額區間
            List<SelectListItem> HangAmountlist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="一仟以下", Value = "1"},
                new SelectListItem() {Text ="伍萬以下至一仟(含)", Value = "2"},
                new SelectListItem() {Text ="伍萬以上(含)", Value = "3"},
            };
            ViewBag.HangAmountlist = HangAmountlist;
            // adam // 帳戶狀況
            List<SelectListItem> NotificationSourcelist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="1.非電話詐財", Value = "1"},
                new SelectListItem() {Text ="2.電話詐財", Value = "2"},
                new SelectListItem() {Text ="3.刑事", Value = "3"},
            };
            ViewBag.NotificationSourcelist = NotificationSourcelist;
            // adam // 狀態
            List<SelectListItem> AccountStatuslist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="1.未結清", Value = "1"},
                new SelectListItem() {Text ="2.結清", Value = "2"},
            };
            ViewBag.AccountStatuslist = AccountStatuslist;
            List<SelectListItem> Itemlist = new List<SelectListItem>
            {
                new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="1.空白", Value = "1"},
                new SelectListItem() {Text ="2.掛帳(正向)", Value = "2"},
                new SelectListItem() {Text ="3.還款(負向)", Value = "3"},
            };
            ViewBag.Itemlist = Itemlist;
            List<SelectListItem> Otherlist = new List<SelectListItem>
            {
                //new SelectListItem() {Text ="-請選擇-", Value = ""},
                new SelectListItem() {Text ="1.明細", Value = "1"},
                new SelectListItem() {Text ="2.彙總", Value = "2"},
            };
            ViewBag.Otherlist = Otherlist;
        }
        /// <summary>
        /// 結果列表
        /// </summary>
        public ActionResult _QueryResult(WarningAccountQuery model, int pageNum = 1)
        {
            HttpCookie modelCookie = new HttpCookie("QueryCookie");
            modelCookie.Values.Add("DocNo", model.DocNo);
            modelCookie.Values.Add("HangAmount", model.HangAmount);
            modelCookie.Values.Add("HangAmountlist", model.HangAmount);
            modelCookie.Values.Add("ItemType", model.ItemType);
            modelCookie.Values.Add("AccountStatus", model.AccountStatus);
            modelCookie.Values.Add("NotificationSource", model.NotificationSource);
            modelCookie.Values.Add("Other", model.Other);
            modelCookie.Values.Add("ForCDateS", model.ForCDateS);
            modelCookie.Values.Add("ForCDateE", model.ForCDateE);
            modelCookie.Values.Add("CurrentPage", pageNum.ToString());
            Response.Cookies.Add(modelCookie);
            return PartialView("_QueryResult", SearchList(model, pageNum));
        }

        // CaseMaster model, int pageNum = 1, string strSortExpression = "CaseId", string strSortDirection = "asc"
        /// <summary>
        /// 實際查詢動作
        /// </summary>
        public WarningAccountQueryViewModel SearchList(WarningAccountQuery model, int pageNum = 1)
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
            //modelCookie.Values.Add("CustId", model.CustId);
            //modelCookie.Values.Add("CustAccount", model.CustAccount);
            //modelCookie.Values.Add("DocNo", model.DocNo);
            //modelCookie.Values.Add("VictimName", model.VictimName);
            modelCookie.Values.Add("DocNo", model.DocNo);
            modelCookie.Values.Add("HangAmount", model.HangAmount);
            modelCookie.Values.Add("HangAmountlist", model.HangAmountlist);
            modelCookie.Values.Add("ItemType", model.ItemType);
            modelCookie.Values.Add("AccountStatus", model.AccountStatus);
            modelCookie.Values.Add("NotificationSource", model.NotificationSource);
            modelCookie.Values.Add("Other", model.Other);
            modelCookie.Values.Add("ForCDateS", model.ForCDateS);
            modelCookie.Values.Add("ForCDateE", model.ForCDateE);
            Response.Cookies.Add(modelCookie);
            #endregion
            model.ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
            model.ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);
            var viewModel = new WarningAccountQueryViewModel();
            //Add adam 20190325
            if (model.Other == "1")
            { 
                IList<WarningAccountQuery> result = wqBiz.GetQueryList(model, pageNum);
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
                viewModel.WarningAccountQuery = model;
                viewModel.WarningAccountQuery.PageSize = wqBiz.PageSize;
                viewModel.WarningAccountQuery.CurrentPage = wqBiz.PageIndex;
                viewModel.WarningAccountQuery.TotalItemCount = wqBiz.DataRecords;
                viewModel.WarningAccountQueryList = result;
            }
            if (model.Other == "2")
            {
                IList<WarningAccountQuery> result1 = wqBiz.GetQueryList1(model,pageNum);
                if (result1.Count > 0)
                {
                    for (int i = 0; i < result1.Count; i++)
                    {
                        CustId = result1[i].CustId.Trim();
                        if (CustId.Length > 0)
                        {
                            _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "DOCNO=" + result1[i].DocNo, CustId);
                        }
                    }
                }
                viewModel.WarningAccountQuery = model;
                viewModel.WarningAccountQuery.PageSize = wqBiz.PageSize;
                viewModel.WarningAccountQuery.CurrentPage = wqBiz.PageIndex;
                viewModel.WarningAccountQuery.TotalItemCount = wqBiz.DataRecords;
                viewModel.WarningAccountQueryList = result1;

            }

            //viewModel.WarningAccountQuery = model;
            //viewModel.WarningAccountQuery.PageSize = wqBiz.PageSize;
            //viewModel.WarningAccountQuery.CurrentPage = wqBiz.PageIndex;
            //viewModel.WarningAccountQuery.TotalItemCount = wqBiz.DataRecords;
            return viewModel;
        }
        //public MeetingResultDetail UploadFile(HttpPostedFileBase upFile)
        //{
        //    if (upFile == null || upFile.ContentLength <= 0) return null;

        //    //获取用户上传文件的后缀名,重命名為當前登入者ID+年月日時分秒毫秒
        //    string newFileName = LogonUser.Account + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + Path.GetExtension(upFile.FileName);

        //    string serverPath = Path.Combine("~/", ConfigurationManager.AppSettings["UploadFolder"], DateTime.Today.ToString("yyyyMM"));
        //    string realPath = Server.MapPath(serverPath);
        //    //*月份文件夾不存在則新增
        //    if (!FrameWork.Util.UtlFileSystem.FolderIsExist(realPath))
        //        FrameWork.Util.UtlFileSystem.CreateFolder(realPath);

        //    //利用file.SaveAs保存图片
        //    string name = Path.Combine(realPath, newFileName);
        //    upFile.SaveAs(name);

        //    MeetingResultDetail aModel = new MeetingResultDetail
        //    {
        //        AttatchDetailName = Path.GetFileName(upFile.FileName),
        //        AttatchDetailServerName = newFileName,
        //        AttatchDetailServerPath = serverPath,
        //        isDelete = 0,
        //        CreatedUser = LogonUser.Account
        //    };
        //    return aModel;
        //}
        public ActionResult Excel(WarningAccountQuery model)
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
                if (cookies.Values["DocNo"] != null) model.DocNo = cookies.Values["DocNo"];
                if (cookies.Values["VictimName"] != null) model.VictimName = cookies.Values["VictimName"];
                if (cookies.Values["ForCDateS"] != null) model.ForCDateS = cookies.Values["ForCDateS"];
                if (cookies.Values["ForCDateE"] != null) model.ForCDateE = cookies.Values["ForCDateE"];
                if (cookies.Values["StateType"] != null) model.StateType = cookies.Values["StateType"];
                if (cookies.Values["RelieveDateS"] != null) model.RelieveDateS = cookies.Values["RelieveDateS"];
                if (cookies.Values["RelieveDateE"] != null) model.RelieveDateE = cookies.Values["RelieveDateE"];
                if (cookies.Values["Other"] != null) model.Other = cookies.Values["Other"];
                if (cookies.Values["NotificationSource"] != null) model.NotificationSource = cookies.Values["NotificationSource"];
                if (cookies.Values["HangAmount"] != null) model.HangAmount = cookies.Values["HangAmount"];
                if (cookies.Values["HangAmountlist"] != null) model.HangAmountlist = cookies.Values["HangAmount"];
                if (cookies.Values["ItemType"] != null) model.ItemType = cookies.Values["ItemType"];
                if (cookies.Values["AccountStatus"] != null) model.AccountStatus = cookies.Values["AccountStatus"];
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
            fileName = Lang.csfs_menu_tit_warningAccountQuery + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

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


        //public ActionResult Details(string DocNo)
        //{
        //ViewBag.DocNo = DocNo;
        //BindWarningStatusList();
        //CaseWarningBIZ warn = new CaseWarningBIZ();

        //CaseWarningViewModel viewmodel = new CaseWarningViewModel()
        //{
        //    WarningMaster = warn.GetWarnMasterListByDocNo(DocNo),
        //    WarningAttachmentList = warn.GetWarnAttatchmentList(DocNo),
        //    WarningStateList = warn.GetWarnStateQueryList(DocNo),
        //    WarningDetailsList = warn.WarningDetailsSearchList(DocNo)
        //};
        //return PartialView("Details", viewmodel);
        [HttpPost]
        public ActionResult Details(HttpPostedFileBase fileAttNames)
        {
            string xlsname = "";
            if (fileAttNames.ContentLength > 0)
            {
                var fileName = Path.GetFileName(fileAttNames.FileName);
                var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                fileAttNames.SaveAs(path);
                xlsname = path;
            }
            MemoryStream ms = new MemoryStream();
            ms = wqBiz.ImportXLSX_NPOI(xlsname);
            string OutPutfileName= string.Empty;
            OutPutfileName = Lang.csfs_menu_tit_warningAccountQuery + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            if (ms != null && ms.Length > 0)
            {
                Response.ClearContent();
                Response.ClearHeaders();
            }
            else
            {
                ms = new MemoryStream();
            }
            return File(ms.ToArray(), "application/vnd.ms-excel", OutPutfileName);

        }
        [HttpGet]
        public ActionResult Details(MemoryStream ms)
        {
            string OutPutfileName = string.Empty;
            OutPutfileName = Lang.csfs_menu_tit_warningAccountQuery + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            if (ms != null && ms.Length > 0)
            {
                Response.ClearContent();
                Response.ClearHeaders();
            }
            else
            {
                ms = new MemoryStream();
            }
            return File(ms.ToArray(), "application/vnd.ms-excel", OutPutfileName);
        }

        //public HttpPostedFileBase UploadFile(HttpPostedFileBase upFile)
        //{
        //    //if (upFile == null || upFile.ContentLength <= 0) return null;

        //    //获取用户上传文件的后缀名,重命名為當前登入者ID+年月日時分秒毫秒
        //    string newFileName = LogonUser.Account + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + Path.GetExtension(upFile.FileName);

        //    string serverPath = Path.Combine("~/", ConfigurationManager.AppSettings["UploadFolder"], DateTime.Today.ToString("yyyyMM"));
        //    string realPath = Server.MapPath(serverPath);
        //    //*月份文件夾不存在則新增
        //    if (!FrameWork.Util.UtlFileSystem.FolderIsExist(realPath))
        //        FrameWork.Util.UtlFileSystem.CreateFolder(realPath);

        //    //利用file.SaveAs保存图片
        //    string name = Path.Combine(realPath, newFileName);
        //    upFile.SaveAs(name);

        //    //MeetingResultDetail aModel = new MeetingResultDetail
        //    //{
        //    //    AttatchDetailName = Path.GetFileName(upFile.FileName),
        //    //    AttatchDetailServerName = newFileName,
        //    //    AttatchDetailServerPath = serverPath,
        //    //    isDelete = 0,
        //    //    CreatedUser = LogonUser.Account
        //    //};
        //    return upFile;
        //}
        //public MeetingResultDetail Details(HttpPostedFileBase upFile)
        //{
        //    if (upFile == null || upFile.ContentLength <= 0) return null;

        //    //获取用户上传文件的后缀名,重命名為當前登入者ID+年月日時分秒毫秒
        //    string newFileName = LogonUser.Account + "_" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + Path.GetExtension(upFile.FileName);

        //    string serverPath = Path.Combine("~/", ConfigurationManager.AppSettings["UploadFolder"], DateTime.Today.ToString("yyyyMM"));
        //    string realPath = Server.MapPath(serverPath);
        //    //*月份文件夾不存在則新增
        //    if (!FrameWork.Util.UtlFileSystem.FolderIsExist(realPath))
        //        FrameWork.Util.UtlFileSystem.CreateFolder(realPath);

        //    //利用file.SaveAs保存图片
        //    string name = Path.Combine(realPath, newFileName);
        //    upFile.SaveAs(name);
        //    //IList<WarningAccountQuery> result = wqBiz.ExcelList(model, pageNum);
        //    MeetingResultDetail aModel = new MeetingResultDetail
        //    {
        //        AttatchDetailName = Path.GetFileName(upFile.FileName),
        //        AttatchDetailServerName = newFileName,
        //        AttatchDetailServerPath = serverPath,
        //        isDelete = 0,
        //        CreatedUser = LogonUser.Account
        //    };

        //    return aModel;

        //}
    }
}