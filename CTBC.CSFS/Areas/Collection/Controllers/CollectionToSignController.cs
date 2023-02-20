using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTBC.CSFS.Areas.Collection
{
    public class CollectionToSignController : AppController
    {
        CollectionToSignBIZ CToSBIZ;

        public CollectionToSignController()
        {
            CToSBIZ = new CollectionToSignBIZ(this);
        }
        public ActionResult Index()
        {
            Bind();
            return View();
        }
        public void Bind()
        {
            CommonBIZ biz = new CommonBIZ();
            ViewBag.SpeedList = new SelectList(biz.GetCodeData("INCOME_SPEED"),"CodeNo", "CodeDesc");
            ViewBag.ReceiveKindList = new SelectList(biz.GetCodeData("INCOME_TYPE"),"CodeNo", "CodeDesc");
            ViewBag.CaseKindList = new SelectList(biz.GetCodeData("CASE_KIND"), "CodeNo", "CodeDesc");
            ViewBag.CaseKind2List = new SelectList(biz.GetCodeData("CASE_SEIZURE"), "CodeNo", "CodeDesc");
        }
        public ActionResult _QueryResult(CollectionToSign CToS, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            return PartialView("_QueryResult", SearchList(CToS, pageNum, strSortExpression, strSortDirection));
        }

        public CollectionToSignViewModel SearchList(CollectionToSign CToS, int pageNum = 1, string strSortExpression = "CaseNo", string strSortDirection = "asc")
        {
            CollectionToSignViewModel viewModel;
            CToS.LanguageType = Session["CultureName"].ToString();
            IList<CollectionToSign> result = CToSBIZ.GetQueryList(CToS, pageNum, strSortExpression, strSortDirection);

            viewModel = new CollectionToSignViewModel()
            {
                CollectionToSign = CToS,
                CollectionToSignList = result,
            };

            //分頁相關設定
            viewModel.CollectionToSign.PageSize = CToSBIZ.PageSize;
            viewModel.CollectionToSign.CurrentPage = CToSBIZ.PageIndex;
            viewModel.CollectionToSign.TotalItemCount = CToSBIZ.DataRecords;
            viewModel.CollectionToSign.SortExpression = strSortExpression;
            viewModel.CollectionToSign.SortDirection = strSortDirection;

            viewModel.CollectionToSign.GovUnit = CToS.GovUnit;
            viewModel.CollectionToSign.GovNo = CToS.GovNo;
            viewModel.CollectionToSign.Person = CToS.Person;
            viewModel.CollectionToSign.Speed = CToS.Speed;
            viewModel.CollectionToSign.ReceiveKind = CToS.ReceiveKind;
            viewModel.CollectionToSign.CaseKind = CToS.CaseKind;
            viewModel.CollectionToSign.CaseKind2 = CToS.CaseKind2;
            viewModel.CollectionToSign.GovDateS = CToS.GovDateS;
            viewModel.CollectionToSign.GovDateE = CToS.GovDateE;
            return viewModel;
        }

        public ActionResult Sign(string strIds)
        {
            List<Guid> guidList = new List<Guid>();
            string[] aryId = strIds.Split(',');
            foreach (string id in aryId)
            {
                if(!string.IsNullOrEmpty(id)){
                    guidList.Add(new Guid(id));
                }                
            }
            string userId = Convert.ToString(Session["UserAccount"]);
            return Json(CToSBIZ.Sign(guidList, userId) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }

        public ActionResult Return(string strIds)
        {
            List<Guid> guidList = new List<Guid>();
            string[] aryId = strIds.Split(',');
            foreach (string id in aryId)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    guidList.Add(new Guid(id));
                }
            }
            string userId = Convert.ToString(Session["UserAccount"]);
            return Json(CToSBIZ.Return(guidList, userId) > 0 ? new JsonReturn() { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn() { ReturnCode = "0", ReturnMsg = Lang.csfs_del_fail });
        }
    }
}