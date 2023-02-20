/// <summary>
/// 程式說明:公共方法
/// </summary>

using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;
using CTCB.ImageUtil;
using CTBC.CSFS.Pattern;
//using Microsoft.Office.Interop.Excel;
using iTextSharp.text.pdf;
using System.Diagnostics;

using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;//20130704 horace


namespace CTBC.FrameWork.Util
{
	public class Common
	{
		/// <summary>
		/// Dialog - 對話種類
		/// </summary>
		public enum MsgType
		{
			/// <summary>訊息</summary>
			Information,
			/// <summary>警告</summary>
			Warning,
			/// <summary>錯誤</summary>
			Error,
			/// <summary>詢問</summary>
			Question,
			/// <summary>驚歎</summary>
			Exclamation
		}

		/// <summary>
		/// Provider種類
		/// </summary>
		public enum ProviderType
		{
			/// <summary>Jet Engine</summary>
			JetEngine,
			/// <summary>SQL Server</summary>
			SQLServer,
			/// <summary>Oracle</summary>
			Oracle
		}

		/// <summary>
		/// Table種類
		/// </summary>
		public enum TableType
		{
			/// <summary>資料表</summary>
			Table,
			/// <summary>檢視表</summary>
			View,
			/// <summary>系統資料表</summary>
			SystemTable,
			/// <summary>系統檢視表</summary>
			SystemView
		}

		/// <summary>
		/// Dialog - 訊息視窗回應種類
		/// </summary>
		public enum RspType
		{
			/// <summary>是</summary>
			Yes,
			/// <summary>否</summary>
			No,
			/// <summary>取消</summary>
			Cancel
		}

		/// <summary>
		/// AppLog - 記錄種類
		/// </summary>
		public enum LogType
		{
			/// <summary>訊息</summary>
			Information,
			/// <summary>警告</summary>
			Warning,
			/// <summary>錯誤</summary>
			Error
		}

		/// <summary>
		/// RegFunction - 註冊區種類
		/// </summary>
		public enum RegType
		{
			/// <summary>Root</summary>
			ClassesRoot,
			/// <summary>CurrentUser</summary>
			CurrentUser,
			/// <summary>LocalMachine</summary>
			LocalMachine,
			/// <summary>Users</summary>
			Users,
			/// <summary>CurrentConfig</summary>
			CurrentConfig
		}

        /// <summary>
        /// Added By Smallzhi
        /// ReportKind - 報表種類
        /// </summary>
        public enum ReportKind
        {
            /// <summary>房貸批覆書</summary>
            RptDisposal,
            /// <summary>保人資料</summary>
            RptGuarantor,
            /// <summary>股票批覆書</summary>
            RptStock,
            /// <summary>其他個無批覆書</summary>
            RptOther,
            /// <summary>C表</summary>
            RptCTable,
            /// <summary>房貸批覆書附件</summary>
            RptHouseAttach,
            /// <summary>股票批覆書附件</summary>
            RptStockAttach,
            /// <summary>其他個無批覆書附件</summary>
            RptOtherAttach,
            /// <summary>審查歷程</summary>
            RptAudit,
            /// <summary>變簽批覆書</summary>
            RptChange,
            /// <summary>變簽批覆書保人資料</summary>
            RptChgGuarantor,
            /// <summary>變簽批覆書審查歷程</summary>
            RptChgAudit,
            /// <summary>產品變簽批覆書</summary>
            RptChgApplProd
        }

        /// <summary>
        /// UDWR FLOW的Step - 徵信Flow Step Code, added by smallzhi
        /// </summary>
        public struct UDWRFlowStep
        {
            /// <summary>
            /// 徵信作業階段
            /// </summary>
            public static string NonGroup = "2320";

            /// <summary>
            /// 變簽徵信作業階段
            /// </summary>
            public static string DISPNonGroup = "3310";

            /// <summary>
            /// 徵信中止階段
            /// </summary>
            public static string TerminatedGroup = "2330";

            /// <summary>
            /// 變簽徵信中止階段
            /// </summary>
            public static string DISPTerminatedGroup = "3330";

            /// <summary>
            /// CO主管階段
            /// </summary>
            public static string COGroup = "2610";

            /// <summary>
            /// 變簽CO主管階段
            /// </summary>
            public static string DISPCOGroup = "3610";

            /// <summary>
            /// 授信CO階段
            /// </summary>
            public static string CreditCOGroup = "2630";

            /// <summary>
            /// 變簽授信CO階段
            /// </summary>
            public static string DISPCreditCOGroup = "3630";

            /// <summary>
            /// 高階主管簽核階段
            /// </summary>
            public static string HLSupervisor = "2620";

            /// <summary>
            /// 變簽高階主管簽核階段
            /// </summary>
            public static string DISPHLSupervisor = "3620";
        }

        /// <summary>
        /// 變簽-徵信 FLOW的Step - 徵信Flow Step Code, added by horace
        /// </summary>
        public struct DISPUDWRFlowStep
        {
            /// <summary>
            /// 徵信作業階段
            /// </summary>
            public static string NonGroup = "3310";

            /// <summary>
            /// 徵信中止階段
            /// </summary>
            public static string TerminatedGroup = "3330";

            /// <summary>
            /// CO主管階段
            /// </summary>
            public static string COGroup = "3610";

            /// <summary>
            /// 授信CO階段
            /// </summary>
            public static string CreditCOGroup = "3630";

            /// <summary>
            /// 高階主管簽核階段
            /// </summary>
            public static string HLSupervisor = "3620";

        }

        /// <summary>
        /// CO層級, added by smallzhi
        /// </summary>
        public struct CreditLevel
        {
            /// <summary>
            /// CO4
            /// </summary>
            public static string CO4 = "03";

            /// <summary>
            /// CO3
            /// </summary>
            public static string CO3 = "04";

            /// <summary>
            /// CO2
            /// </summary>
            public static string CO2 = "05";

            /// <summary>
            /// CO1
            /// </summary>
            public static string CO1 = "06";

            /// <summary>
            /// SCO3
            /// </summary>
            public static string SCO3 = "07";

            /// <summary>
            /// SCO2
            /// </summary>
            public static string SCO2 = "08";

            /// <summary>
            /// SCO1
            /// </summary>
            public static string SCO1 = "09";

            /// <summary>
            /// CreditCO
            /// </summary>
            public static string CreditCO = "00";

            /// <summary>
            /// NonCO
            /// </summary>
            public static string NonCO = "01";

            /// <summary>
            /// CreditSigner
            /// </summary>
            public static string CreditSigner = "02";

        }

        /// <summary>
        /// 徵信審核結果代碼, added by smallzhi
        /// </summary>
        public enum UDWRAudit
        {
            /// <summary>
            /// 核准
            /// </summary>
            A,
            /// <summary>
            /// 婉拒
            /// </summary>
            D,
            /// <summary>
            /// 中止
            /// </summary>
            S,
            /// <summary>
            /// 補件
            /// </summary>
            P,
            /// <summary>
            /// 重查
            /// </summary>
            I,
            /// <summary>
            /// 重起
            /// </summary>
            R,
            /// <summary>
            /// 退回
            /// </summary>
            B
        }

        /// <summary>
        /// 業務類別代碼, added by smallzhi
        /// </summary>
        public enum BusClassType
        {
            /// <summary>
            /// 股票
            /// </summary>
            D,
            /// <summary>
            /// 房貸
            /// </summary>
            E,
            /// <summary>
            /// 其他個人
            /// </summary>
            F
        }

        /// <summary>
        /// 案件類型代碼, added by smallzhi
        /// </summary>
        public enum ApplTypeCode
        {
            /// <summary>
            /// 新件
            /// </summary>
            I,
            /// <summary>
            /// 變簽件
            /// </summary>
            J,
            /// <summary>
            /// 申覆件
            /// </summary>
            L
        }

        /// <summary>
        /// 變簽類型, added by smallzhi
        /// </summary>
        public enum DISBStatus
        {
            /// <summary>
            /// 貸前
            /// </summary>
            B,
            /// <summary>
            /// 貸後
            /// </summary>
            A
        }

        /// <summary>
        /// 無指派人員代碼
        /// </summary>
        public static string NonOwner
        {
            get { return "Error"; }
        }

        /// <summary>
        /// 選人類型, added by smallzhi
        /// </summary>
        public enum OwnerType
        {
            /// <summary>
            /// 不需選擇Owner[0]
            /// </summary>
            NoSelection,
            /// <summary>
            /// 徵信作業->徵信主管[1]
            /// </summary>
            Type1,
            /// <summary>
            /// 授信CO->徵信主管[2]
            /// </summary>
            Type2,
            /// <summary>
            /// 徵信主管->徵信作業員(中止與補件)[3]
            /// </summary>
            Type3,
            /// <summary>
            /// 徵信主管->授信CO[4]
            /// </summary>
            Type4,
            /// <summary>
            /// 徵信主管->徵信主管[5]
            /// </summary>
            Type5,
            /// <summary>
            /// 退回[6]
            /// </summary>
            Type6,
            /// <summary>
            /// 重查[7]
            /// </summary>
            Type7,
            /// <summary>
            /// 中止補件重啟[8]
            /// </summary>
            Type8,
            /// <summary>
            /// 徵信主管由RuleEngine計算[9]
            /// </summary>
            Type9,
            /// <summary>
            /// 改派[10]
            /// </summary>
            Type10,
            /// <summary>
            /// 最高核決層級是SCO2以上, 且案件尚未Close, 直接給SYS, 並流入2620[11]
            /// </summary>
            Type11,
            /// <summary>
            /// 自選
            /// </summary>
            Type12
        }

		/// <summary>
		/// 將圖片存於固定的路徑
		/// </summary>
		/// <param name="path">路徑</param>
		/// <param name="buffer">從數據庫中讀取的圖片</param>
		/// <remarks> add by sky 2011/12/16</remarks>
		public void SaveImg(string path, byte[] buffer)
		{
			// 判斷文件是否存在,存在則刪除
			if (System.IO.File.Exists(path))
			{
				System.IO.File.Delete(path);
			}

			FileStream fstmObj = new FileStream(path, FileMode.Create);

			// 遍歷寫入文件
			for (int i = 0; i <= buffer.Length - 1; i++)
			{
				fstmObj.WriteByte(buffer[i]);
			}

			// 釋放資源
			fstmObj.Flush();
			fstmObj.Close();
			GC.Collect();
		}

		/// <summary>
		/// 將tif轉為jpg
		/// </summary>
		/// <param name="newPath">jpg路徑</param>
		/// <param name="path">tif路徑</param>
		/// <returns>jpg路徑</returns>
		/// <remarks>add by sky 2011/12/16</remarks>
		public string ConvertImage(string newPath, string path)
		{
			string jengFirstParth = "";

			// 路徑不為空時
			if (!string.IsNullOrEmpty(path))
			{
				TIFUtil tim = new TIFUtil(path);

				// 遍歷當前影像的總頁數，并轉換為jpg格式
				for (int i = 1; i <= tim.PageCount; i++)
				{
					string jegFirstPath = newPath.ToUpper().Replace(".TIF", Guid.NewGuid().ToString() + ".jpg");

					tim.ConvertTo((uint)i, jegFirstPath);

					jengFirstParth = jengFirstParth + Path.GetFileName(jegFirstPath) + ',';
				}
			}

			GC.Collect();

			return jengFirstParth.TrimEnd(',');
		}

		/// <summary>
		/// 刪除檔案
		/// </summary>
		/// <param name="path">刪除檔案路徑</param>
		/// <param name="tempPath">臨時目錄</param>
		/// <remarks>add by sky 2011/12/08</remarks>
		public bool DeleteFile(string path, string tempPath)
		{
			// 如果為空或null時返回
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}

			try
			{
				// 得到所有文件
				string[] strPath = path.Split(',');

				// Temp文件夾路徑
				string strTmpPath = "";

				// 遍歷刪除文件
				for (int i = 0; i < strPath.Length; i++)
				{
					// 判斷是否包含目錄
					if (!strPath[i].Contains(@"\Content\Temp\"))
					{
						strTmpPath = tempPath;
					}

					// 如果存在則刪除
					if (System.IO.File.Exists(strTmpPath + strPath[i]))
					{
						System.IO.File.Delete(strTmpPath + strPath[i]);
					}

					// 清空
					strTmpPath = "";
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return true;
		}

		/// <summary>
		/// 讀取影像
		/// </summary>
		/// <param name="filepath">存放影像的路徑</param>
		/// <returns>影像的二進制流</returns>
		/// <remarks>add by Ruina 2011/12/13</remarks>
		public byte[] GetImageByte(string filePath)
		{
			// 創建文件流
			FileStream fls = new FileStream(filePath, FileMode.Open, FileAccess.Read);

			int FileLength = Convert.ToInt32(fls.Length);

			byte[] blob = new byte[fls.Length];

			// 寫入byte[]
			fls.Read(blob, 0, System.Convert.ToInt32(fls.Length));

			fls.Flush();
			fls.Close();

			return blob;
		}

		/// <summary>
		/// 獲取Excel列
		/// </summary>
		/// <param name="intColumnIndex">列數</param>
		/// <returns></returns>
		/// <remarks>Add by Emily 2011/12/13</remarks>
		private string GetColumnChar(int intColumnIndex)
		{
			int intQuot = intColumnIndex / 26;
			int intComp = intColumnIndex % 26;

			char chrSecond = (char)(intComp + 65);

			string strColumn = chrSecond.ToString();

			if (intQuot != 0)
			{
				char chrFirst = (char)(intQuot + 65 - 1);

				strColumn = chrFirst.ToString() + strColumn;
			}

			return strColumn;
		}

		/// <summary>
		/// 匯出Excel
		/// </summary>
		/// <param name="result">匯出結果table</param>
		/// <param name="excelTitle">Excel標題</param>
		/// <param name="fieldName">Excel文件名字</param>  
		/// <param name="path">路徑</param>
		/// <returns></returns>
		/// <remarks>Add by Emily 2011/12/29</remarks>
        //public void ExportToExcel(System.Data.DataTable result, string excelTitle, string fileName, string path)
        //{
        //    Application objExcel = new Application();

        //    // 新增工作簿
        //    Workbooks objWorkBooks;
        //    objWorkBooks = objExcel.Workbooks;

        //    Workbook objWorkBook;
        //    objWorkBook = objWorkBooks.Add(Missing.Value);

        //    // 新增Sheet
        //    Worksheet objSheet;
        //    objSheet = (Worksheet)objWorkBook.Worksheets[1];
        //    objSheet.Name = fileName.Substring(0, fileName.Length - 4); ;

        //    //刪除不用的Sheet
        //    Worksheet m_objSheet_3;
        //    m_objSheet_3 = (Worksheet)objWorkBook.Worksheets[3];
        //    m_objSheet_3.Delete();
        //    Worksheet m_objSheet_2;
        //    m_objSheet_2 = (Worksheet)objWorkBook.Worksheets[2];
        //    m_objSheet_2.Delete();

        //    int cl = 1;
        //    int begRow = 2;

        //    // 添加表頭
        //    foreach (string title in excelTitle.Split(','))
        //    {
        //        objSheet.Cells[1, cl] = title;
        //        objSheet.Cells[1, cl].Font.Bold = true;
        //        cl++;
        //    }

        //    // 欄標題置中
        //    objSheet.get_Range("A1", GetColumnChar(excelTitle.Split(',').Length - 1) + "1").HorizontalAlignment = HorizontalAlign.Center;
        //    objSheet.Columns.AutoFit();

        //    //設定欄標題的背景顏色
        //    objSheet.get_Range("A1", GetColumnChar(excelTitle.Split(',').Length - 1) + "1").Interior.ColorIndex = 15;

        //    objSheet.Cells.Borders.LineStyle = 1;
        //    objSheet.Cells.Font.Size = 12;
        //    objSheet.Cells.Font.Name = "Times New Roman";

        //    // 向Excel中添加數據--行遍歷
        //    for (int i = 0; i < result.Rows.Count; i++)
        //    {
        //        // 列遍歷
        //        for (int j = 0; j < result.Columns.Count; j++)
        //        {
        //            // 欄位個是否為字符型，是保持格式不變
        //            if (result.Rows[i][j].GetType().Name == "String")
        //            {
        //                objSheet.Cells[i + begRow, j + 1] = "'" + result.Rows[i][j].ToString();
        //            }
        //            else
        //            {
        //                objSheet.Cells[i + begRow, j + 1] = result.Rows[i][j].ToString();
        //            }
        //        }
        //    }

        //    // 判斷路徑是否存在,不存在則創建
        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }

        //    // 保存文件完整名
        //    string strFilePath = path + Guid.NewGuid().ToString() + ".xls";

        //    // 是否存在，存在刪除excel文件
        //    if (System.IO.File.Exists(strFilePath))
        //    {
        //        System.IO.File.Delete(strFilePath);
        //    }

        //    try
        //    {
        //        // 保存
        //        objExcel.ActiveWorkbook.SaveCopyAs(strFilePath);

        //        // 釋放資源
        //        objWorkBook = null;
        //        objWorkBooks = null;
        //        objExcel.Quit();
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(objExcel);
        //        objExcel = null;

        //        // 強制進行柆圾收集
        //        GC.Collect();

        //        // File(strFilePath, "application/ms-excel", HttpUtility.UrlEncode(fileName));  

        //        //System.IO.FileStream fileStream = new System.IO.FileStream(strFilePath, System.IO.FileMode.Open,
        //        //            System.IO.FileAccess.Read,FileShare.ReadWrite);

        //        // System.IO.BinaryReader br = new System.IO.BinaryReader(fileStream);

        //        //this.Response.AddHeader("content-disposition", "attachment; filename=" +
        //        //        HttpUtility.UrlEncode("test" + ".xls", System.Text.Encoding.UTF8));

        //        //this.Response.ContentType = "mine";
        //        //this.Response.BinaryWrite(br.ReadBytes((int)(fileStream.Length)));
        //        //Response.Flush();
        //        //Response.Close();


        //        //fileStream.Close();
        //        System.IO.File.Delete(strFilePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        // 釋放資源
        //        objExcel.Quit();
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(objExcel);
        //        objExcel = null;
        //        System.IO.File.Delete(strFilePath);
        //        GC.Collect();

        //        // 拋出異常
        //        throw ex;
        //    }
        //}

		/// <summary>
		/// 發郵件
		/// </summary>
		/// <param name="mailFrom">mail來源</param>
		/// <param name="mailTo">mail目的地</param>
		/// <param name="mailSubject">mail主題</param>
		/// <param name="mailContent">mail內容</param>
		public void SendMail(string mailFrom, string mailTo, string mailSubject, string mailContent)
		{

		}

		/// <summary>
		/// 解壓縮ZIP檔案
		/// </summary>
		/// <param name="fileName">文件路徑及文件名</param>
		/// <param name="waitMillSeconds">等待時間</param>
		public void FileUnZip(string fileName, int waitMillSeconds)
		{

		}

		/// <summary>
		///  Pdf轉換Tif
		/// </summary>
		/// <param name="mpath">路徑</param>
		/// <param name="strFileName">文件名</param>
		/// <param name="PdfFile">pdf二進制</param>
		/// <param name="strPath">pdftotiff.exe轉換工具路徑</param>
		/// <returns></returns>
		/// <remarks>Add By Niking 2011/12/13</remarks>
		public bool PdfToTif(string mpath, string strFileName, byte[] PdfFile, string strPath)
		{
			//將PDF轉成TIF檔案
			PdfReader readEach = new PdfReader(new RandomAccessFileOrArray(PdfFile), null);
			PdfStamper stamper = new PdfStamper(readEach, new FileStream(mpath + strFileName + ".PDF", FileMode.Create));
			stamper.Close();

			//將PDF檔案轉存TIF檔案
			Process.Start(strPath + "pdftotiff.exe",
			 "-i \"" + mpath + strFileName + ".PDF" + "\" -o" + mpath + " -b 1 -c GROUP4 -m -x 200 -y 200");
			while (Process.GetProcessesByName("pdftotiff").Length > 0)
			{
			}

			// 檔案尚未生成           
			return true;
		}

		/// <summary>
		/// 讀取文件tif--轉換為二進制
		/// </summary>
		/// <param name="strImageFileName">文件路徑</param>
		/// <returns>二進制流</returns>
		/// <remarks>Add By Niking 2011/12/13</remarks>
		public byte[] GetFile(string strImageFileName)
		{
			FileStream fls;

			int iFileLength = 0;

			// 創建文件流
			fls = new FileStream(strImageFileName, FileMode.Open, FileAccess.Read);

			iFileLength = Convert.ToInt32(fls.Length);

			byte[] blob = new byte[fls.Length];

			// 寫入byte[]
			fls.Read(blob, 0, System.Convert.ToInt32(fls.Length));

			fls.Close();

			return blob;
		}

		#region add by mel 20120301 start

		/// <summary>
		/// Xml 轉 HashTable
		/// </summary>
		/// <param name="xmlNode"></param>
		public Hashtable xmlParsing(XmlNode xmlInput)
		{
			Hashtable htInputParms = new Hashtable();
			XmlDocument xmldoc = new XmlDocument();

			xmldoc.LoadXml(xmlInput.OuterXml);
			XmlElement xmlelRoot = xmldoc.ChildNodes[0] as XmlElement;
			for (int i = 0; i < xmlelRoot.ChildNodes.Count; i++)
			{
				htInputParms.Add(xmlelRoot.ChildNodes[i].Name, xmlelRoot.ChildNodes[i].InnerText);
			}
			return htInputParms;
		}

		#endregion add by mel 20120301 end

        /// <summary>
        /// 身分證輸入10位時，檢核8碼數字+2碼英文字
        /// 身分證輸入11位時，檢核8碼數字+3碼英文字
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckForeignID(string id)
        {
            string pattern1 = @"^[0-9]{8}[a-zA-Z]{2}$";
            string pattern2 = @"^[0-9]{8}[a-zA-Z]{3}$";
            Regex rgx1 = new Regex(pattern1, RegexOptions.IgnoreCase);
            Match match1 = rgx1.Match(id);
            Regex rgx2 = new Regex(pattern2, RegexOptions.IgnoreCase);
            Match match2 = rgx2.Match(id);
            if (match1.Success || match2.Success) return true;
            else return false;
        }

        // add by mel 20131107
        //檢查ID是否為外國人=true/本國人=false
        public bool  CSFSCheckIsForeign(string str)      
        {
            string pattern = @"^[0-9]{1}$";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = rgx.Match(str.Substring(0, 1));

            if (match.Success) return true;
            else return false;

        }

        /// <summary>
        /// 產生MD5 Hash Value
        /// </summary>
        /// <returns></returns>
        public string GetMd5Hash(string _source)
        {
            using (System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash. 
                byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_source));

                // Create a new Stringbuilder to collect the bytes 
                // and create a string.
                System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();

                // Loop through each byte of the hashed data  
                // and format each one as a hexadecimal string. 
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string. 
                return sBuilder.ToString();
            }
        }

	}
}
