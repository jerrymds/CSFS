using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CTBC.CSFS.Models;
using CTBC.CSFS.Pattern;
using CTBC.FrameWork.Platform;
using NPOI.HSSF.UserModel;
using CTCB.APLog;//APLog Redis ader 2022-03-01 - ADD

namespace CTBC.CSFS.BussinessLogic
{
    public class CommonBIZ : BaseBusinessRule
    {
        public AppController _applController;
        // 策略物件    		
        #region 用戶信息
        //
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string Account
        {
            get
            {
                if (HttpContext.Current.Session["LogonUser"] != null)
                    if (!string.IsNullOrEmpty(((User)HttpContext.Current.Session["LogonUser"]).Account))
                        return ((User)HttpContext.Current.Session["LogonUser"]).Account;
                    else return "Anonymous";
                else
                    return "Anonymous";
            }
        }

        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string AccountName
        {
            get
            {
                if (HttpContext.Current.Session["LogonUser"] != null)
                    if (!string.IsNullOrEmpty(((User)HttpContext.Current.Session["LogonUser"]).Name))
                        return ((User)HttpContext.Current.Session["LogonUser"]).Name;
                    else return "Anonymous";
                else
                    return "Anonymous";
            }
        }

        /// <summary>
        /// 部門
        /// </summary>
        public string BU
        {
            get
            {
                if (HttpContext.Current.Session["LogonUser"] != null)
                    if (!string.IsNullOrEmpty(((User)HttpContext.Current.Session["LogonUser"]).BU))
                        return ((User)HttpContext.Current.Session["LogonUser"]).BU;
                    else return "Anonymous";
                else
                    return "Anonymous";
            }
        }

        /// <summary>
        /// 權限
        /// </summary>
        public string ActiveRule
        {
            get
            {
                if (HttpContext.Current.Session["LogonUser"] != null)
                    if (!string.IsNullOrEmpty(((User)HttpContext.Current.Session["LogonUser"]).ActiveRole))
                        return ((User)HttpContext.Current.Session["LogonUser"]).ActiveRole;
                    else return "Anonymous";
                else
                    return "Anonymous";
            }
        }



        #endregion

        #region 構造函數

        public CommonBIZ()
            : base()
        {

        }

        public CommonBIZ(string connectstring)
            : base(connectstring)
        {

        }

        public CommonBIZ(AppController appController)
            : base(appController)
        {
            _applController = appController;
        }

        #endregion

        #region Common共用Function

        /// <summary>
        /// 取得某個CodeType之下的所有(啟用+未啟用)的PARMCode資料
        /// </summary>
        /// <param name="codeType">CodeType欄位</param>
        public IList<PARMCode> GetCodeData(string codeType)
        {
            //可從Cache中將參數資料,透過ParmCode這個Key值直接取出
            var parmCodeList = (IEnumerable<PARMCode>)AppCache.Get("PARMCode");

            var query = from item in parmCodeList
                        where item.CodeType == codeType
                        orderby item.SortOrder
                        select item;

            return query.ToList();
        }

        public IList<PARMCode> GetCaseTrsCodeData(string codeType)
        {
            //可從Cache中將參數資料,透過ParmCode這個Key值直接取出
            var parmCodeList = (IEnumerable<PARMCode>)AppCache.Get("PARMCode");

            var query = from item in parmCodeList
                        where (item.CodeType == codeType) && ((item.CodeNo == "01") || (item.CodeNo == "02") || (item.CodeNo == "03") || (item.CodeNo == "04"))
                        orderby item.SortOrder
                        select item;

            return query.ToList();
        }
        /// <summary>
        /// 取得某個CodeType之下的所有啟用(或未啟用)的PARMCode資料
        /// </summary>
        /// <param name="codeType">CodeType欄位</param>
        /// <param name="enable">Enable欄位:true/false</param>
        public IList<PARMCode> GetCodeData(string codeType, bool enable)
        {
            //可從Cache中將參數資料,透過ParmCode這個Key值直接取出
            var parmCodeList = (IEnumerable<PARMCode>)AppCache.Get("PARMCode");

            var query = from item in parmCodeList
                        where item.CodeType == codeType && item.Enable == enable
                        select item;

            query = query.Where(m => m.Enable == enable);

            return query.ToList();
        }

        public string GetPARMCodeByCodeTypeAndCodeDesc(string CodeType, string codedesc)
        {
            string returncode = "";
            try
            {
                string sql = "select * from PARMCode where CodeType = @CodeType and CodeDesc = @CodeDesc";
                Parameter.Clear();
                Parameter.Add(new CommandParameter("@CodeType", CodeType));
                Parameter.Add(new CommandParameter("@CodeDesc", codedesc));
                IList<PARMCode> list = SearchList<PARMCode>(sql);
                if (list.Count > 0)
                {
                    returncode = list.First().CodeNo;
                }
                else
                {
                    returncode = "";
                }
                return returncode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<PARMCode> GetPARMCodeByCodeType(string CodeType, string codeNo)
		{
			try
			{
				string sql = "select * from PARMCode where CodeType = @CodeType and CodeNo = @CodeNo";
				Parameter.Clear();
				Parameter.Add(new CommandParameter("@CodeType", CodeType));
				Parameter.Add(new CommandParameter("@CodeNo", codeNo));
				IList<PARMCode> list = SearchList<PARMCode>(sql);
				return list;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        /// <summary>
        ///  表單類型
        /// </summary>
        /// <param name="codeType">代碼類別</param>
        /// <param name="codeNo">代碼</param>
        /// <returns></returns>
        /// <remarks>Add By Niking 2011/12/05</remarks>
        public IList<PARMCode> GetCodeData(string codeType, string codeNo)
        {
            #region 參數

            //可從Cache中將參數資料,透過ParmCode這個Key值直接取出
            var parmCodeList = (IEnumerable<PARMCode>)AppCache.Get("PARMCode");

            var query = from item in parmCodeList
                        where item.Enable == true
                        select item;

            // 代碼
            if (!string.IsNullOrEmpty(codeType))
            {
                query = query.Where(m => m.CodeType == codeType).OrderBy(x => x.SortOrder); //20130604 Tom加order by SortOrder
            }

            // 參數細項代碼
            if (!string.IsNullOrEmpty(codeNo))
            {
                query = query.Where(m => m.CodeNo.Contains(codeNo));
            }

            #endregion

            return query.ToList();
        }
        public PARMCode GetFirstCodeDataByDesc(string codeType, string codeDesc)
        {
            #region 參數

            var parmCodeList = (IEnumerable<PARMCode>)AppCache.Get("PARMCode");

            var query = from item in parmCodeList
                        where item.Enable == true
                        select item;

            // 代碼
            if (!string.IsNullOrEmpty(codeType))
            {
                query = query.Where(m => m.CodeType == codeType).OrderBy(x => x.SortOrder); //20130604 Tom加order by SortOrder
            }

            // 參數細項代碼
            if (!string.IsNullOrEmpty(codeDesc))
            {
                query = query.Where(m => m.CodeDesc.Contains(codeDesc));
            }
            #endregion
            
            return query.FirstOrDefault();
        }

        public DateTime GetNowDateTime()
        {
            DateTime dtRet = DateTime.Now;

            try
            {
                string sql = @"SELECT convert(varchar(100),getdate(),121)";
                // 清空容器
                base.Parameter.Clear();
                DataTable dtResult = base.Search(sql);
                if (dtResult.Rows.Count > 0)
                {
                    dtRet = DateTime.Parse(dtResult.Rows[0][0].ToString());
                }
            }
            catch (Exception)
            {
                return dtRet;
            }

            return dtRet;
        }


        /// <summary>
        /// 讀取人員資料
        /// </summary>
        /// <returns>人員資料</returns>
        /// <remarks> add by mel 2013/09/10</remarks>
        public IList<CSFSEmployee> GetEmployeeList()
        {
            try
            {
                string sql = "";

                sql = @"SELECT 
	                        CSFSEmployee.EmpID
	                        ,CSFSEmployee.EmpName  
	                        ,CSFSEmployee.EmpID + '/' + CSFSEmployee.EmpName  AS EmDesc
                        From 
	                        CSFSEmployee   order by CSFSEmployee.EmpID  ";

                // 清空容器
                base.Parameter.Clear();

                return base.SearchList<CSFSEmployee>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region AddBy Ge.Song
        public static MemoryStream ExcelExport<TType>(IList<TType> list, string[] headerColumns,
                                                      Action<HSSFRow, TType> setExcelRow)
        {
            return ExcelExport(new HSSFWorkbook(), list, headerColumns, setExcelRow);
        }


        /// <summary>
        /// excel export.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="workbook"></param>
        /// <param name="list"></param>
        /// <param name="headerColumns"></param>
        /// <param name="setExcelRow"></param>
        /// <returns></returns>
        public static MemoryStream ExcelExport<TType>(HSSFWorkbook workbook, IList<TType> list, string[] headerColumns, Action<HSSFRow, TType> setExcelRow)
        {
            var ms = new MemoryStream();

            #region set column style
            var headerStyle = workbook.CreateCellStyle() as HSSFCellStyle;
            var fonts = workbook.CreateFont() as HSSFFont;
            fonts.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            headerStyle.SetFont(fonts);
            #endregion
            
            int iPage = 0;
            int iTotal = 0;
            int pageSize = 65534;

            if (!list.Any())
            {
                var sheet = workbook.CreateSheet() as HSSFSheet;
                var dataRow = sheet.CreateRow(0) as HSSFRow;
                //set header
                for (var j = 0; j < headerColumns.Length; j++)
                {
                    dataRow.CreateCell(j).SetCellValue(headerColumns[j]);
                    dataRow.GetCell(j).CellStyle = headerStyle;
                }
            }
            else
            {
                while (iTotal < list.Count)
                {
                    iTotal = iPage * pageSize;
                    int iCurrentCount = 0;
                    if (list.Count - iTotal > pageSize)
                    {
                        iCurrentCount = pageSize;
                    }
                    else
                    {
                        iCurrentCount = list.Count - iTotal;

                    }

                    var sheet = workbook.CreateSheet() as HSSFSheet;
                    var dataRow = sheet.CreateRow(0) as HSSFRow;
                    //set header
                    for (var j = 0; j < headerColumns.Length; j++)
                    {
                        dataRow.CreateCell(j).SetCellValue(headerColumns[j]);
                        dataRow.GetCell(j).CellStyle = headerStyle;
                    }

                    for (var i = iTotal; i < iTotal + iCurrentCount; i++)
                    {
                        //*set row data
                        dataRow = (HSSFRow)sheet.CreateRow(i - iTotal + 1);
                        setExcelRow(dataRow, list[i]);

                    }
                    sheet = null;
                    iTotal += iCurrentCount;
                    iPage++;
                }
            }

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            workbook = null;
            return ms;
        }

        #endregion

        #region APLog Redis ader 2022-03-01
        private static DateTime _lastExceptionTime = DateTime.MinValue;
        public static void InitRedis()
        {
            CTCB.APLog.APLogUtil.CheckRedisStatus();
        }

        private void SaveAPLog(string cusIDs, string parameters = null, string txnCode = null, string userID = null, string userIP = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cusIDs))
                {
                    return;
                }

                SetAPLogDefaultValue(ref parameters, ref txnCode, ref userID, ref userIP);

                APLogVO vo = new APLogVO()
                {
                    CusIDs = cusIDs,
                    DataTimestamp = DateTime.Now,
                    Parameters = parameters,
                    TxnCode = txnCode,
                    IP = userIP,
                    LogonUser = userID,
                };
                CTCB.APLog.APLogUtil.SaveAPLog(vo, CTCB.APLog.APLogUtil.SYSID_CSFS);
            }
            catch (Exception ex)
            {
                //萬一發生異常(例如 Redis 掛了)，一小時通知一次系統管理者，避免過於頻繁通知
                if (DateTime.Now.Subtract(_lastExceptionTime).TotalMinutes > 60)
                {
                    _lastExceptionTime = DateTime.Now;
                    CSFSLogBIZ.WriteLog(new CSFSLog()
                    {
                        Title = "APLog",
                        Message = "SaveAPLog Error, Exception: " + ex.ToString()
                    });
                }
            }
        }

        private static void SetAPLogDefaultValue(ref string parameters, ref string txnCode, ref string userID, ref string userIP)
        {
            var httpContext = HttpContext.Current;

            if (httpContext != null)
            {
                if (string.IsNullOrWhiteSpace(userID))
                {
                    userID = (httpContext.Session["UserAccount"] == null) ? "" : ((CTBC.FrameWork.Platform.User)httpContext.Session["LogonUser"]).Account;
                }
                if (string.IsNullOrWhiteSpace(userIP))
                {
                    userIP = (httpContext.Session["LogonUser"] == null) ? "" : ((CTBC.FrameWork.Platform.User)httpContext.Session["LogonUser"]).SessionIP;
                }

                if (string.IsNullOrWhiteSpace(txnCode))
                {
                    //var routeValues = httpContext.Request?.RequestContext?.RouteData?.Values;
                    var routeValues = (httpContext.Request != null && httpContext.Request.RequestContext != null && httpContext.Request.RequestContext.RouteData != null) ? httpContext.Request.RequestContext.RouteData.Values : null;
                    if (routeValues != null)
                    {
                        string controller = routeValues.ContainsKey("controller") ? (string)routeValues["controller"] : "";
                        string action = routeValues.ContainsKey("action") ? (string)routeValues["action"] : "";
                        txnCode = !string.IsNullOrWhiteSpace(controller) && !string.IsNullOrWhiteSpace(action)
                            ? controller + "." + action
                            : !string.IsNullOrWhiteSpace(controller) ? controller : action;
                    }
                }
            }

            txnCode = string.IsNullOrWhiteSpace(txnCode) ? "UNKNOWN" : txnCode;
            userID = string.IsNullOrWhiteSpace(txnCode) ? "UNKNOWN" : userID;
            userIP = string.IsNullOrWhiteSpace(txnCode) ? "UNKNOWN" : userIP;

            if (parameters == null && httpContext != null && httpContext.Request != null)
            {
                System.Collections.Specialized.NameValueCollection requestParameters = ("POST".Equals(httpContext.Request.HttpMethod)) ? httpContext.Request.Form : httpContext.Request.QueryString;
                parameters = string.Join("|", requestParameters.AllKeys
                    .Where(x => x != "X-Requested-With")
                    .Select(x => string.Format("{0}={1}", x, requestParameters[x])));
            }
        }

        public void SaveAPLog(IEnumerable<string> enumerable, string parameters = null, string txnCode = null, string userID = null, string userIP = null)
        {
            if (enumerable == null)
            {
                return;
            }
            SaveAPLog(string.Join("|", enumerable.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x => x.Replace("|", "_")).Distinct()), parameters, txnCode, userID, userIP);
        }

        #endregion APLog Redis ader 2022-03-01
    }
}
