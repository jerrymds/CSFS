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

namespace CTBC.CSFS.Areas.Common.Controllers
{
    public class AgentMeetInfoDetailController : AppController
    {
        CaseMeetBiz meetBiz = new CaseMeetBiz();

        // GET: Agent/AgentMeetInfoDetail
        public ActionResult Index(Guid caseId, string FromControl)
        {
            ViewBag.CaseId = caseId;
            ViewBag.FromControl = FromControl;
            //* 首先呼叫Biz取得已存資料
            CaseMeetMaster master = meetBiz.GetCaseMeetMaster(caseId);
            if (master == null)
            {
                //* 說明沒存過, new一個新的,並且從參數表讀得從表的設定
                master = new CaseMeetMaster {ListDetails = new List<CaseMeetDetails>()};
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
                            IsSelected = false,
                            Result = ""
                        });
                    }
                }
            }
            else
            {
                master.StandardDateS = UtlString.FormatDateTw(master.StandardDateS);
                master.StandardDateE = UtlString.FormatDateTw(master.StandardDateE);
            }
            //* 將Model顯示在畫面上
            return View(master);
        }
        public ActionResult DoSaveMeetInfo(CaseMeetMaster master)
        {
            //* 因為你不知道是新增還是修改.反正都寫就是了.到時候sql insert/update欄位不更新就是了
            master.StandardDateS = UtlString.FormatDateTwStringToAd(master.StandardDateS);
            master.StandardDateE = UtlString.FormatDateTwStringToAd(master.StandardDateE);
            master.CreatedUser = LogonUser.Account;
            master.ModifiedUser = LogonUser.Account;
            return Json(meetBiz.SaveCaseMeetInfo(master) ? new JsonReturn { ReturnCode = "1", ReturnMsg = "" }
                                                        : new JsonReturn { ReturnCode = "0", ReturnMsg = Lang.csfs_save_fail });
        }
    }
}