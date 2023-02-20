using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using System.Text;

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class AgentDocumentInfoDetailController : AppController
    {
        CaseSeizureViewModel caseview = new CaseSeizureViewModel();
        CaseMasterBIZ casemaster = new CaseMasterBIZ();
        CaseAttachmentBIZ attachment = new CaseAttachmentBIZ();
        CaseObligorBIZ obligor = new CaseObligorBIZ();
        CaseAccountBiz caseAccount = new CaseAccountBiz();
        ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
        //
        // GET: /Common/AgentDocumentInfoDetail/
        public ActionResult Index(Guid CaseId, string FromControl)
        {
            ViewBag.CaseId = CaseId;
            ViewBag.FromControl = FromControl;
            caseview.CaseMaster = casemaster.MasterModel(CaseId);  //* 得到CaseMaster的model
            caseview.CaseMaster.GovDate = UtlString.FormatDateTw(caseview.CaseMaster.GovDate);//將西元年轉換為民國年
            caseview.CaseObligorlistO = new List<CaseObligor>(10);//* 初始化義務人員行數
            caseview.CaseAttachmentlistO = attachment.AttachmentList(CaseId);//* 得到CaseAttachment的model
            caseview.CaseEdocFilelist = CaseEdocFile.GetCaseEdocFileList(CaseId);//* 得到CaseEdocFile的model
            List<CaseObligor> list = obligor.ObligorModel(CaseId);    //* 得到CaseObligor的model
            for (int i = 0; i < list.Count; i++)
            {
                caseview.CaseObligorlistO.Add(list[i]);
            }
            for (int i = list.Count; i < 10; i++)
            {
                caseview.CaseObligorlistO.Add(new CaseObligor());
            }
			CaseEdocFile caseEdocFile = caseAccount.OpenTxtDoc(CaseId);
			ViewBag.HasFile = caseEdocFile == null ? "0" : "1";
            #region //受文者、來函扣押總金額、金額未達毋需扣押 默認值顯示 IR-1008
            //if (caseview.CaseMaster.ReceiveKind == "電子公文")
            //{
            //    if (caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizure || caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizureAndPay)//細類為扣押或扣押並支付時，這三個欄位才有默認值
            //    {
            //        //受文者
            //        if (caseview.CaseMaster.Receiver == "")
            //        {
            //            caseview.CaseMaster.Receiver = "8888";//1.電子來文 8888(預設值)
            //        }
            //        ////來函扣押總金額 txt的「合計」如果合計沒有才顯示DB里的ReceiveAmount
            //        //string text = string.Empty;
            //        //if (caseEdocFile != null)
            //        //{
            //        //    byte[] file = caseEdocFile.FileObject;
            //        //    text = Encoding.UTF8.GetString(file);
            //        //    int beginIndex = text.IndexOf("合計：");
            //        //    int endIndex = text.IndexOf("備註：");
            //        //    if (beginIndex > 0 && endIndex > 0 && beginIndex < endIndex)
            //        //    {
            //        //        string amt = text.Substring(beginIndex + 3, endIndex - beginIndex - 3).Trim();
            //        //        if (!string.IsNullOrEmpty(amt))
            //        //        {
            //        //            caseview.CaseMaster.ReceiveAmount = int.Parse(amt);
            //        //        }
            //        //    }
            //        //}
            //        //金額未達毋需扣押
            //        if (caseview.CaseMaster.NotSeizureAmount == 0)
            //        {
            //            caseview.CaseMaster.NotSeizureAmount = 450;//1.電子來文 450(預設值)
            //        }
            //    }
            //}
            //else
            //{
            //    if (caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizure || caseview.CaseMaster.CaseKind2 == CaseKind2.CaseSeizureAndPay)//細類為扣押或扣押並支付時，這三個欄位才有默認值
            //    {
            //        //受文者
            //        if (caseview.CaseMaster.Receiver == "")
            //        {
            //            caseview.CaseMaster.Receiver = caseview.CaseMaster.Unit;//紙本來文 預設與分行別同
            //        }
            //        //來函扣押總金額 紙本來文 依分行人員鍵檔

            //        //金額未達毋需扣押
            //        if (caseview.CaseMaster.NotSeizureAmount == 0)
            //        {
            //            //紙本來文 預設值:法院1250 執行署450
            //            if (caseview.CaseMaster.GovKind == "法院")
            //            {
            //                caseview.CaseMaster.NotSeizureAmount = 1250;
            //            }
            //            else
            //            {
            //                caseview.CaseMaster.NotSeizureAmount = 450;
            //            }
            //        }
            //    }
            //}
            #endregion
            // 新增個資LOG
            string _user = (HttpContext.Session["UserAccount"] != null) ? HttpContext.Session["UserAccount"].ToString() : "";
            System.Collections.Specialized.NameValueCollection _parameters = new System.Collections.Specialized.NameValueCollection();
            //System.Collections.Specialized.NameValueCollection _parameters = ("POST".Equals(HttpContext.Request.HttpMethod)) ? HttpContext.Request.Form : HttpContext.Request.QueryString;
            string _controller = (this.ControllerContext.RouteData.Values["Controller"] != null) ? this.ControllerContext.RouteData.Values["Controller"].ToString() : "";
            string _action = (this.ControllerContext.RouteData.Values["Action"] != null) ? this.ControllerContext.RouteData.Values["Action"].ToString() : "";
            string _ip = (HttpContext.Request != null && !string.IsNullOrEmpty(HttpContext.Request.ServerVariables["REMOTE_ADDR"])) ? HttpContext.Request.ServerVariables["REMOTE_ADDR"] : "";
            BaseBusinessRule _business = new BaseBusinessRule();
            if (list.Count > 0)
            {
                //for (int i = 0; i < list.Count; i++)
                //{
                    _business.ExecuteAPLOGSave(_user, _controller, _action, _ip, "CASEID=" + CaseId.ToString(), list[0].ObligorNo.ToString());
                //}
            }
            // 新增結束
            
            //APLog Redis ader 2022-07-07 - ADD
            if (caseview.CaseObligorlistO != null && caseview.CaseObligorlistO.Count > 0)
            {
                obligor.SaveAPLog(caseview.CaseObligorlistO.Select(x => x.ObligorNo).ToArray());
            }
            //APLog Redis ader 2022-07-07 - END

            return View(caseview);
        }
    }
}