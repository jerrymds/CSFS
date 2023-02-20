using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Filter;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Resource;
using CTBC.CSFS.ViewModels;
using CTBC.FrameWork.Util;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using CTBC.CSFS.WebService.OpenStream;

namespace CTBC.CSFS.Areas.TrsDetail.Contrllers
{
    public class DeadSendController : AppController
    {
        #region 全局變量
        CaseDeadQueryBIZ _CaseDeadQueryBIZ = new CaseDeadQueryBIZ();
        DataTable UploadTable = new DataTable();
        CSFSLogBIZ _csfsLogBIZ = new CSFSLogBIZ();

        #endregion

        #region 頁面
        /// <summary>
        /// 歷史記錄查詢與重送
        /// </summary>
        /// <returns></returns>
        [RootPageFilter]
        public ActionResult Index(string isBack)
        {
            // 調用查詢清單綁定方法
            BindList();

            CaseDeadCondition model = new CaseDeadCondition();

            if (isBack == "1")
            {
                //*執行cookie里的內容
                HttpCookie cookies = Request.Cookies.Get("CaseDeadSendWhere");
                if (cookies != null)
                {
                    if (cookies.Values["DocNo"] != null) model.DocNo = cookies.Values["DocNo"];
                    //if (cookies.Values["Option1"] != null) model.Option1 = cookies.Values["Option1"];
                    //if (cookies.Values["CustId"] != null) model.CustId = cookies.Values["CustId"];
                    //if (cookies.Values["CustAccount"] != null) model.CustAccount = cookies.Values["CustAccount"];
                    //if (cookies.Values["QFileName"] != null) model.QFileName = cookies.Values["QFileName"];   
                    //if (cookies.Values["ForCDateS"] != null) model.ForCDateS = cookies.Values["ForCDateS"];
                    //if (cookies.Values["ForCDateE"] != null) model.ForCDateE = cookies.Values["ForCDateE"];
                    if (cookies.Values["FileName"] != null) model.FileName = cookies.Values["FileName"];
                    ViewBag.CurrentPage = cookies.Values["CurrentPage"];
                    ViewBag.isQuery = "1";
                }
            }

            // 當前登錄者是否有重查的權限
            if (LogonUser != null && !string.IsNullOrEmpty(LogonUser.RCAFAccount) && !string.IsNullOrEmpty(LogonUser.RCAFBranch))
            {
                model.IsEnable = "Y";
            }
            else
            {
                model.IsEnable = "N";
            }


            return View(model);
        }


   
        public string ConvertToHalf( string wd)
        {
            // 全形轉半形
            string strReturn = Strings.StrConv(wd, VbStrConv.Narrow, 0).ToLower().Trim();
            // 小寫轉大寫
            strReturn = strReturn.ToUpper();
            return strReturn;
        }
  
        /// <param name="pKey">要更新的資料主鍵</param>
        /// <param name="pFlag">註記 pFlag=1：主管檢視放行；pFlag=2：記錄查詢與重送</param>
        /// <returns></returns>
        public ActionResult Edit(string pKey, string pFlag)
        {
            ViewBag.Key = pKey;
            ViewBag.Flag = pFlag;

            return View();
        }
        #endregion

        #region 自定義方法
        #region 下拉選單
        /// <summary>
        /// 查詢條件清單綁值
        /// </summary>
        public void BindList()
        {
            // 案件狀態
            ViewBag.CaseStatusList = new SelectList(_CaseDeadQueryBIZ.GetCodeData("CaseCustStatus"), "CodeNo", "CodeDesc");
            string sel = "TWD";
            ViewBag.CurrencyList = new SelectList(_CaseDeadQueryBIZ.GetCodeData("CaseCust_CURRENCY"), "CodeDesc", "CodeMemo", sel);
            // 處理方式
            ViewBag.ProcessingMethodList = BindProcessingMethodList();

            // 查詢項目
            ViewBag.SearchProgramList = BindSearchProgramList();
        }

        /// <summary>
        /// 處理方式
        /// </summary>
        /// <returns></returns>
        public SelectList BindProcessingMethodList()
        {
            // 選項集合
            IList<SelectListItem> lst = new List<SelectListItem>();

            lst.Add(new SelectListItem() { Value = "01", Text = "未處理" });
            lst.Add(new SelectListItem() { Value = "66", Text = "已處理" });
            lst.Add(new SelectListItem() { Value = "Y", Text = "已上傳" });

            return new SelectList(lst, "Value", "Text");
        }

        /// <summary>
        /// 查詢項目
        /// </summary>
        /// <returns></returns>
        public SelectList BindSearchProgramList()
        {
            // 選項存款大類集合
            IList<SelectListItem> lst = new List<SelectListItem>();

            lst.Add(new SelectListItem() { Value = "1", Text = "1.基本資料" });
            lst.Add(new SelectListItem() { Value = "2", Text = "2.存款明細" });
            lst.Add(new SelectListItem() { Value = "3", Text = "3.基本 + 存款明細" });

            return new SelectList(lst, "Value", "Text");
        }

        #endregion

        /// <summary>
        /// 實際查詢動作
        /// </summary>
        /// <param name="model">實體類</param>
        /// <param name="pageNum">當前頁面</param>
        /// <returns></returns>
   

        [HttpPost]
        public ActionResult Upload(CaseDeadCondition model, HttpPostedFileBase fileAttNames)
        {
            try
            {
                string FileName = "";
                CaseDeadCondition BatchModel = new CaseDeadCondition();
                DataTable DT = new DataTable();
                CaseDeadBIZ Biz = new CaseDeadBIZ();
                CaseMasterBIZ cm = new CaseMasterBIZ();
                //
                // 檢查外來文案號-取出 CaseID
                DataTable dtCaseId = cm.GetCaseMasterByDocNo(model.DocNo);
                if (dtCaseId.Rows.Count < 1)
                {
                    //return Json("無此外來交案號", JsonRequestBehavior.AllowGet);
                    return Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = "無此外來文案號" });
                }
                // 取得可以查的營業日
                //int chkEndDay = Biz.GetParmCodeEndDateDiff(model.ForCDateE);
                //if (chkEndDay == 1)
                //{
                //    return Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = "迄日不能大於營業日前二日" });
                //}
                #region 查詢條件記錄在cookie
                HttpCookie modelCookie = new HttpCookie("CaseDeadSendWhere");
                modelCookie.Values.Add("DocNo", model.DocNo);
                //modelCookie.Values.Add("Option1", model.Option1.ToString());
                //modelCookie.Values.Add("CustId", model.CustId);
                //modelCookie.Values.Add("Option2", model.Option2.ToString());
                //modelCookie.Values.Add("CustAccount", model.CustAccount);
                //modelCookie.Values.Add("Option3", model.Option3.ToString());
                //modelCookie.Values.Add("ForCDateS", model.ForCDateS);
                //modelCookie.Values.Add("ForCDateE", model.ForCDateE);
                modelCookie.Values.Add("FileName", model.FileName);
                //modelCookie.Values.Add("Currency", model.Currency);
                //modelCookie.Values.Add("CurrentPage", pageNum.ToString());
                Response.Cookies.Add(modelCookie);
                #endregion

                string DocNo = model.DocNo;

                string Filename = model.FileName;
                // 轉換統一編號 全形轉半形,小寫轉大寫


                #region 日期格式轉換
                // 建檔日期 起
                //if (!string.IsNullOrEmpty(model.ForCDateS))
                //{
                //    ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
                //}
                //// 建檔日期 訖
                //if (!string.IsNullOrEmpty(model.ForCDateE))
                //{
                //    ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);
                //}
                #endregion



                //轉換全形轉半形,小寫轉大寫

                string Inputerror = "";
                if (true)
                {
                    if (fileAttNames != null)
                    {
                        string xlsname = "";
                        if (fileAttNames.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(fileAttNames.FileName);
                            var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                            fileAttNames.SaveAs(path);
                            xlsname = path;
                            FileName = path;
                        }
                        MemoryStream ms = new MemoryStream();
                        DT.Clear();
                        DT = Biz.ImportXLSX_NPOI(xlsname);

                    }
                    // 判斷EXCEL 是否
                    DataTable TempTable = new DataTable();
                    int isDup = 0; 
                    TempTable.Clear();
                    TempTable = DT.Copy();
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (int chk = 0; chk < TempTable.Rows.Count; chk++)
                    {
                        if (TempTable.Rows[chk][0].ToString().Length == 0)
                        {
                            continue;
                        }
                        // 判斷如果當天已經有另一筆案號已設定,整個EXCEL都不做
                        isDup =Biz.GetDailyData(model.DocNo, TempTable.Rows[chk][5].ToString());
                        if (isDup > 0)
                        {
                            break;
                        }
                    }
                    if (isDup > 0)
                    {
                        return Json(new JsonReturn { ReturnCode = "0", ReturnMsg = "出現重覆;今日已有案號匯入" });
                    }

                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    if ((string.IsNullOrEmpty(DT.Rows[i][5].ToString())))
                    {
                        Inputerror = Inputerror + "F"   + "-不能為空白\r";
                        break;
                    }
                    if ((string.IsNullOrEmpty(DT.Rows[i][6].ToString())))
                    {
                        Inputerror = Inputerror + "G"   + "-不能為空白\r";
                        break;
                    }
                    if ((string.IsNullOrEmpty(DT.Rows[i][7].ToString())))
                    {
                        Inputerror = Inputerror + "H"+ "-不能為空白\r";
                        break;
                    }
                    if ((string.IsNullOrEmpty(DT.Rows[i][8].ToString())))
                    {
                        Inputerror = Inputerror + "I" + "-不能為空白\r";
                        break;
                    }
                    if ((string.IsNullOrEmpty(DT.Rows[i][9].ToString())))
                    {
                        Inputerror = Inputerror + "J" + "-不能為空白\r";
                        break;
                    }
                    if ((string.IsNullOrEmpty(DT.Rows[i][10].ToString())))
                    {
                        Inputerror = Inputerror + "K" + "-不能為空白\r";
                        break;
                    }
                    if ((string.IsNullOrEmpty(DT.Rows[i][16].ToString())))
                    {
                        Inputerror = Inputerror + "Q" + "-不能為空白\r";
                        break;
                    }
       
                }
            }
            if (Inputerror !="") //&& model.Option1 == "1"
            {
                    return Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = "上傳內容有錯:\r" + Inputerror });
                }


                this.LogWriter = this.AppLog.Writer;
                this.ApLog.Message = "Excel 上傳成功" ;
                this.ApLog.dic["UserId"] = "System";
                this.ApLog.dic["FunctionId"] = this.ControllerName;
                Log();
                //_csfsLogBIZ.LogonLogoutLog(this.ApLog);
                // 先將查詢資料或上傳資料寫入CaseDeadVersion
                string succes = Biz.InsertCaseDeadVersion(DT, DocNo, dtCaseId.Rows[0]["CaseId"].ToString(), this.LogonUser, "1", FileName, null);
                if (succes=="")
                {
                    this.LogWriter = this.AppLog.Writer;
                    this.ApLog.Message = "InsertCaseDeadVersion 成功";
                    this.ApLog.dic["UserId"] = "System";
                    this.ApLog.dic["FunctionId"] = this.ControllerName;
                    Log();
                    //_csfsLogBIZ.LogonLogoutLog(this.ApLog);
                    int iOK = Biz.insertCaseDeadQueryHistory(model, null, FileName);
                    this.LogWriter = this.AppLog.Writer;
                    this.ApLog.Message = "InsertCaseDeadQueryHistory 成功";
                    this.ApLog.dic["UserId"] = "System";
                    this.ApLog.dic["FunctionId"] = this.ControllerName;
                    Log();
                    //_csfsLogBIZ.LogonLogoutLog(this.ApLog);
                }
                else
                {
                    return Json(new JsonReturn() { ReturnCode = "0", ReturnMsg = succes });
                }
                //
                string fg = Biz.StartSearch(this.LogonUser, DocNo, dtCaseId.Rows[0]["caseid"].ToString());
                bool rtn = false;
                if (fg == "設定成功-待發查")
                {
                    this.LogWriter = this.AppLog.Writer;
                    this.ApLog.Message = "InsertApprMsgKey 成功";
                    this.ApLog.dic["UserId"] = "System";
                    this.ApLog.dic["FunctionId"] = this.ControllerName;
                    Log();
                    //_csfsLogBIZ.LogonLogoutLog(this.ApLog);
                    rtn = true;
                }
                else
                {
                    this.LogWriter = this.AppLog.Writer;
                    this.ApLog.Message = "InsertApprMsgKey 失敗";
                    this.ApLog.dic["UserId"] = "System";
                    this.ApLog.dic["FunctionId"] = this.ControllerName;
                    Log();
                    //_csfsLogBIZ.LogonLogoutLog(this.ApLog);
                    rtn = false;
                }
                return Json(rtn ? new JsonReturn { ReturnCode = "1", ReturnMsg = fg }
                                    : new JsonReturn { ReturnCode = "0", ReturnMsg = fg });
            }
            catch (Exception ex)
            {
                this.LogWriter = this.AppLog.Writer;
                this.ApLog.Message = ex.Message.ToString();
                this.ApLog.dic["UserId"] = "System";
                this.ApLog.dic["FunctionId"] = this.ControllerName;
                Log();
                //_csfsLogBIZ.LogonLogoutLog(this.ApLog);
                throw ex;
            }
        }

        private void Log()
        {
            CSFSLogBIZ _csfsLogBIZ = new CSFSLogBIZ();
            this.ApLog.Categories.Add("LogonLogout");
            this.ApLog.Title = "CaseDead";
            this.ApLog.Priority = 1;
            this.ApLog.EventId = 101;
            this.ApLog.TimeStamp = DateTime.Now;
            this.ApLog.Severity = System.Diagnostics.TraceEventType.Start;
            this.ApLog.dic["ActionCode"] = ActionCode.Login;
            this.ApLog.dic["TranFlag"] = TranFlag.After;
            this.ApLog.dic["FunctionId"] = "Action." + this.ControllerName + "." + this.ActionName;
            this.ApLog.dic["SessionId"] = HttpUtility.HtmlEncode(HttpContext.Session.SessionID);//20150108 horace 弱掃
            this.ApLog.dic["URL"] = HttpUtility.HtmlEncode(HttpContext.Request.RawUrl);//20150108 horace 弱掃
            this.ApLog.dic["IP"] = HttpUtility.HtmlEncode(HttpContext.Request.UserHostAddress);//20150108 horace 弱掃
            this.ApLog.dic["MachineName"] = HttpUtility.HtmlEncode(HttpContext.Request.UserHostName);//20150108 horace 弱掃
            this.ApLog.ExtendedProperties = this.ApLog.dic;
            _csfsLogBIZ.LogonLogoutLog(this.ApLog);//20150209弱掃
            this.ApLog.Categories.Remove("LogonLogout");
        }


        public int IsDuplicateField(DataTable dtCheck, string fieldName1,string fieldName2, System.Text.StringBuilder sb = null)
        {
            string strId; string strAcctNo; int intErr1 = 0; int intErr2 = 0;
            int RowNo = 0;
            int DelNo = 9999;
            if (sb != null)
            {
                foreach (DataRow R in dtCheck.Rows)
                {
                    strId = R[1] == DBNull.Value ? "" : R[1].ToString();
                    strAcctNo = R[2] == DBNull.Value ? "" : R[2].ToString();

                    if (strId.Trim() == fieldName1.Trim() && fieldName1.Length > 7 && fieldName2.Length < 12) 
                    {
                        intErr1 = intErr1 + 1;
                        if (intErr1 > 1)
                        {
                            DelNo = RowNo;
                        }
                    }
                    //if (strId.Trim() == fieldName1.Trim() && fieldName1.Length > 7 && fieldName2.Length > 11)
                    //{
                    //    intErr1 = intErr1 + 1;
                    //    if (intErr1 > 1)
                    //    {
                    //        DelNo = RowNo;
                    //    }
                    //}
                    if (strAcctNo.Trim() == fieldName2.Trim() && fieldName2.Length >= 12)
                    {
                        intErr2 = intErr2 + 1;
                        if (intErr2 > 1)
                        {
                            DelNo = RowNo;
                        }
                    }
                    RowNo = RowNo + 1;
                }
                // 只要有一個重覆就刪除
                return DelNo; 
            }
            else
            {
                return DelNo;
            }
        }

        /// <returns></returns>
        //[HttpPost]
        //public ActionResult _QueryResult(CaseDeadCondition model, HttpPostedFileBase fileAttNames)
        //{
        //    //
        //    string FileName = "";
        //    CaseDeadCondition BatchModel = new CaseDeadCondition();
        //    DataTable DT = new DataTable();
        //    CaseTrsBIZ Biz = new CaseTrsBIZ();
        //    CaseMasterBIZ cm = new CaseMasterBIZ();
        //    //
        //    // 檢查外來文案號-取出 CaseID
        //    DataTable dtCaseId = cm.GetCaseMasterByDocNo(model.DocNo);
        //    if (dtCaseId.Rows.Count < 1)
        //    {
        //        //return Json("無此外來交案號", JsonRequestBehavior.AllowGet);
        //        return Json(new JsonReturn() { ReturnCode = "1", ReturnMsg = "無此外來交案號" });
        //    }
        //    // 取得可以查的營業日
        //    int chkEndDay = Biz.GetParmCodeEndDateDiff(model.ForCDateE);
        //    if (chkEndDay == 1)
        //    {
        //        return Json(new JsonReturn() { ReturnCode = "1", ReturnMsg = "迄日不能大於營業日前二日" });
        //    }
        //    #region 查詢條件記錄在cookie
        //    HttpCookie modelCookie = new HttpCookie("CaseDeadSendWhere");
        //    modelCookie.Values.Add("DocNo", model.DocNo);
        //    modelCookie.Values.Add("Option1", model.Option1.ToString());
        //    modelCookie.Values.Add("CustId", model.CustId);
        //    //modelCookie.Values.Add("Option2", model.Option2.ToString());
        //    modelCookie.Values.Add("CustAccount", model.CustAccount);
        //    //modelCookie.Values.Add("Option3", model.Option3.ToString());
        //    modelCookie.Values.Add("ForCDateS", model.ForCDateS);
        //    modelCookie.Values.Add("ForCDateE", model.ForCDateE);
        //    modelCookie.Values.Add("FileName", model.FileName);
        //    //modelCookie.Values.Add("CurrentPage", pageNum.ToString());
        //    Response.Cookies.Add(modelCookie);
        //    #endregion

        //    string DocNo = model.DocNo;
        //    string CustId = model.CustId;
        //    string CustAccount = model.CustAccount;
        //    string ForCDateS = "";
        //    string ForCDateE = "";
        //    string Option1 = model.Option1;
        //    string Filename = model.FileName;
        //    // 轉換統一編號 全形轉半形,小寫轉大寫

        //    if (model.CustId != null)
        //    {
        //        CustId = ConvertToHalf(model.CustId);
        //    }
        //    if (model.CustAccount != null)
        //    {
        //        CustAccount = ConvertToHalf(model.CustAccount);
        //    }

        //    #region 日期格式轉換
        //    // 建檔日期 起
        //    if (!string.IsNullOrEmpty(model.ForCDateS))
        //    {
        //        ForCDateS = UtlString.FormatDateTwStringToAd(model.ForCDateS);
        //    }
        //    // 建檔日期 訖
        //    if (!string.IsNullOrEmpty(model.ForCDateE))
        //    {
        //        ForCDateE = UtlString.FormatDateTwStringToAd(model.ForCDateE);
        //    }
        //    #endregion



        //    //轉換全形轉半形,小寫轉大寫


        //    if (model.Option1 == "1")
        //    {
        //        if (fileAttNames != null)
        //        {
        //            string xlsname = "";
        //            if (fileAttNames.ContentLength > 0)
        //            {
        //                var fileName = Path.GetFileName(fileAttNames.FileName);
        //                var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
        //                fileAttNames.SaveAs(path);
        //                xlsname = path;
        //                FileName = path;
        //            }
        //            MemoryStream ms = new MemoryStream();

        //            DT = Biz.ImportXLSX_NPOI(xlsname);
        //            //UploadTable = TrsBiz.ImportCSVFile(xlsname);//改CSV
        //            //Model = TrsBiz.ImportToCasDead(xlsname);  
        //            //return View(Model);
        //        }
        //        //
        //        string Inputerror = "";
        //        for (int i = 0; i < DT.Rows.Count; i++)
        //        {
        //            if ((string.IsNullOrEmpty(DT.Rows[i][0].ToString())))
        //            {
        //                break;
        //            }
        //            if (DT.Rows[i][0].ToString().Length == 0)
        //            {
        //                Inputerror = Inputerror + "編號-" + i.ToString() + " 編號不能空白\r";
        //            }
        //            //編號 統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
        //            if (DT.Rows[i][1].ToString().Length != 8 && DT.Rows[i][1].ToString().Length != 11 && DT.Rows[i][1].ToString().Length != 10)
        //            {
        //                Inputerror = Inputerror + "編號-" + i.ToString() + " 統一編號長度錯誤\r";
        //            }
        //            else
        //            {
        //                DT.Rows[i][1] = DT.Rows[i][1].ToString().ToUpper();
        //            }
        //            if (DT.Rows[i][2].ToString().Length != 12)
        //            {
        //                Inputerror = Inputerror + "編號-" + i.ToString() + " 帳號長度錯誤\r";
        //            }
        //            else
        //            {
        //                DT.Rows[i][2] = DT.Rows[i][2].ToString().ToUpper();
        //            }
        //            if (DT.Rows[i][3].ToString().ToUpper() != "Y")
        //            {
        //                DT.Rows[i][3] = "N";
        //            }
        //            else
        //            {
        //                DT.Rows[i][3] = "Y";
        //            }
        //            if (DT.Rows[i][4].ToString().ToUpper() != "Y")
        //            {
        //                DT.Rows[i][4] = "N";
        //            }
        //            else
        //            {
        //                DT.Rows[i][4] = "Y";
        //            }
        //            DateTime Test;
        //            if (DateTime.TryParseExact(DT.Rows[i][5].ToString(), "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.None, out Test) == true)
        //            {
        //                DT.Rows[i][5] = DT.Rows[i][5].ToString();
        //            }
        //            else
        //            {
        //                if (DateTime.TryParseExact(DT.Rows[i][5].ToString(), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out Test) == true)
        //                {
        //                    DT.Rows[i][5] = DT.Rows[i][5].ToString();
        //                }
        //                else
        //                {
        //                    Inputerror = Inputerror + "編號-" + i.ToString() + " 區間起日(YYYY/MM/DD)錯誤\r";
        //                }
        //            }
        //            if (DateTime.TryParseExact(DT.Rows[i][6].ToString(), "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.None, out Test) == true)
        //            {
        //                DT.Rows[i][6] = DT.Rows[i][6].ToString();
        //            }
        //            else
        //            {
        //                if (DateTime.TryParseExact(DT.Rows[i][5].ToString(), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out Test) == true)
        //                {
        //                    DT.Rows[i][6] = DT.Rows[i][6].ToString();
        //                }
        //                else
        //                {
        //                    Inputerror = Inputerror + "編號-" + i.ToString() + " 區間起日(YYYY/MM/DD)錯誤\r";
        //                }
        //            }
        //        }
        //    }
        //    // Option = 2
        //    if (model.Option1 == "2")
        //    {
        //        //先執行產生EXCEL格式的DataTable
        //        DT.Clear();
        //        DT = Biz.GetCaseTrsQueryVersionToDT();
        //        DataRow newDataRow = DT.NewRow();
        //        newDataRow[0] = 1;
        //        newDataRow[1] = CustId;
        //        newDataRow[2] = "N";
        //        newDataRow[3] = "Y";
        //        newDataRow[4] = "Y";
        //        newDataRow[5] = ForCDateS;
        //        newDataRow[6] = ForCDateE;
        //        DT.Rows.Add(newDataRow);
        //        //統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
        //    }

        //    if (model.Option1 == "3")
        //    {
        //        //先執行產生EXCEL格式的DataTable
        //        DT.Clear();
        //        DT = Biz.GetCaseTrsQueryVersionToDT();
        //        DataRow newDataRow = DT.NewRow();
        //        newDataRow[0] = 1;
        //        newDataRow[1] = "N";
        //        newDataRow[2] = CustAccount;
        //        newDataRow[3] = "N";
        //        newDataRow[4] = "Y";
        //        newDataRow[5] = ForCDateS;
        //        newDataRow[6] = ForCDateE;
        //        DT.Rows.Add(newDataRow);
        //        //統一編號	帳號	基資	交易明細	區間起日(YYYY/MM/DD)	區間迄日(YYYY/MM/DD)
        //    }


        //    this.LogWriter = this.AppLog.Writer;
        //    // 先將查詢資料或上傳資料寫入CaseTrsQuery
        //    //bool succes = Biz.InsertCaseTrsQuery(DT, DocNo, dtCaseId.Rows[0]["CaseId"].ToString(), this.LogonUser, Option1, FileName,null);
        //    bool succes = Biz.InsertCaseTrsQueryVersion(DT, DocNo, dtCaseId.Rows[0]["CaseId"].ToString(), this.LogonUser, Option1, FileName, null);
        //    if (model.Option1 == "1")
        //    {
        //        int iOK = Biz.insertCaseDeadQuery(model, null, FileName);
        //    }
        //    //
        //    string fg = Biz.StartSearch(this.LogonUser, DocNo, dtCaseId.Rows[0]["caseid"].ToString());
        //    bool rtn = false;
        //    if (fg == "設定成功-待發查")
        //    {
        //        rtn = true;
        //    }
        //    else
        //    {
        //        rtn = false;
        //    }
        //    return Json(rtn ? new JsonReturn { ReturnCode = "1", ReturnMsg = fg }
        //                        : new JsonReturn { ReturnCode = "0", ReturnMsg = fg });

        //    //return PartialView();
        //}
  


  
        #endregion
    }
}