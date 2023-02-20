using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;

namespace CTBC.CSFS.Areas.Agent.Controllers
{
    public class AgentMeetInfoController : AppController
    {
        CaseMeetBiz meetBiz = new CaseMeetBiz();

        // GET: Agent/AgentMeetInfo
        public ActionResult Index(Guid caseId)
        {
            ViewBag.CaseId = caseId;
            //* 首先呼叫Biz取得已存資料
            CaseMeetMaster master = meetBiz.GetCaseMeetMaster(caseId);
            if (master == null)
            {
                //* 說明沒存過, new一個新的,並且從參數表讀得從表的設定
                master = new CaseMeetMaster
                {
                    ListDetails = new List<CaseMeetDetails>(),
                    //StandardDateS = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd")),
                    //StandardDateE = UtlString.FormatDateTw(DateTime.Now.ToString("yyyy/MM/dd"))
                };
                IList<PARMCode> listCode = meetBiz.GetCodeData("MEET_UNIT");
                if (listCode != null && listCode.Any())
                {
                    listCode = listCode.OrderBy(m => m.SortOrder).ToList();
                    foreach (PARMCode code in listCode)
                    {
                        master.ListDetails.Add(new CaseMeetDetails()
                        {
                            CaseId = caseId,
                            MeetKind = code.CodeDesc,
                            MeetUnit = code.CodeTag,
                            SortOrder = code.SortOrder,
                            IsSelected = true,
                            Result = ""
                        });
                    }
                }
            }
            //else
            //{
            //    master.StandardDateS = UtlString.FormatDateTw(master.StandardDateS);
            //    master.StandardDateE = UtlString.FormatDateTw(master.StandardDateE);
            //}
            //* 將Model顯示在畫面上
            #region 判斷vip狀態
            List<CaseObligor> listObligor = new CaseObligorBIZ().ObligorModel(caseId);
            var listObligors = from list in listObligor         //排序
                               orderby list.ObligorNo ascending
                               select list;

            string strObligorNo = string.Empty;
            if (listObligors != null && listObligors.Any())
            {
                foreach (CaseObligor item in listObligors)
                {
                    strObligorNo += "'" + item.ObligorNo + "',";
                }
                strObligorNo = strObligorNo.TrimEnd(',');
                List<CaseMeetMaster> vipList = new CaseAccountBiz().GetBranchVipFromTx(strObligorNo);
                string strSub = string.Empty;
                if (vipList != null && vipList.Any())
                {
                    foreach (var item in vipList)
                    {
                        if (item.BranchViptext.Length > 0)
                        {
                            strSub = item.BranchViptext.Substring(0, 2);
                            if (Convert.ToInt32(strSub) >= 1 && Convert.ToInt32(strSub) <= 8)//當頭兩位在1-8則屬於VIP,只要有一筆就屬於vip，然偶胡跳出。
                            {
                                master.BranchVip = true;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            return View(master);
        }
        public ActionResult DoSaveMeetInfo(CaseMeetMaster master)
        {
            //* 因為你不知道是新增還是修改.反正都寫就是了.到時候sql insert/update欄位不更新就是了
            //master.StandardDateS = UtlString.FormatDateTwStringToAd(master.StandardDateS);
            //master.StandardDateE = UtlString.FormatDateTwStringToAd(master.StandardDateE);
            master.CreatedUser = LogonUser.Account;
            master.ModifiedUser = LogonUser.Account;
            return Json(meetBiz.SaveCaseMeetInfo(master) ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
        }
    }
}