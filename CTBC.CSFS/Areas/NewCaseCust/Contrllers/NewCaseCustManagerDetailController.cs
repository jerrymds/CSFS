using System.Collections.Generic;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using CTBC.CSFS.ViewModels;
using System.Web.Mvc;
using System;
using System.Text;
using CTBC.CSFS.Resource;
using System.IO;
using System.Configuration;
using CTBC.CSFS.Pattern;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web;
using CTBC.CSFS.WebService.OpenStream;
using Newtonsoft.Json;

namespace CTBC.CSFS.Areas.NewCaseCust.Contrllers
{
    public class NewCaseCustManagerDetailController : AppController
    {
        #region 全局變量
        NewCaseCustManagerBIZ _CaseCustManagerBIZ = new NewCaseCustManagerBIZ();
        PARMCodeBIZ _PARMCodeBIZ = new PARMCodeBIZ();
        #endregion

        #region 主管檢視放行明細頁面

        /// <summary>
        /// 主管檢視放行明細頁面
        /// </summary>
        /// <param name="DocNo">案件編號</param>
        /// <param name="pageFrom">頁碼</param>
        /// <param name="Flag">註記 Flag=1：主管檢視放行；Flag=2：歷史記錄查詢與重送 </param>
        /// <returns></returns>
        public ActionResult Index(Guid strKey, String pageFrom, string Flag)
        {
            string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            NewCaseCustCondition model = _CaseCustManagerBIZ.GetCaseCustQuery(strKey.ToString());

            // 頁面來源 1:主管放行;2:歷史記錄查詢與重送
            model.PageSource = Flag;

            // 前頁面進入前停留的頁數
            model.PageFrom = pageFrom;

            //OpenFileStream webService = new OpenFileStream();

            //webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            //dynamic stream = JsonConvert.DeserializeObject(webService.FileExist(strFilePath + model.ROpenFileName));

            //if (stream.Code != "0000")
            //{
            //    model.ROpenFileName = "";
            //}

            //stream = JsonConvert.DeserializeObject(webService.FileExist(strFilePath + model.RFileTransactionFileName));

            //if (stream.Code != "0000")
            //{
            //    model.RFileTransactionFileName = "";
            //}

            //// 回文首頁
            //stream = JsonConvert.DeserializeObject(webService.FileExist(strFilePath + model.DocNo + "_" + model.Version + "_001.pdf"));


            //if (stream.Code != "0000")
            //{
            //    model.ReturnFileTitle = "";
            //}
            //else
            //{
            //    model.ReturnFileTitle = model.DocNo + "_" + model.Version + "_001.pdf";
            //}

            //// 回文首頁
            //stream = JsonConvert.DeserializeObject(webService.FileExist(strFilePath + model.DocNo + "_" + model.Version + ".pdf"));

            //if (stream.Code != "0000")
            //{
            //    model.ReturnFilePDF = "";
            //}
            //else
            //{
            //    model.ReturnFilePDF = model.DocNo + "_" + model.Version + ".pdf";
            //}

            ViewData["txtReciveFilePath"] = ConfigurationManager.AppSettings["txtReciveFilePath"];

            if (!string.IsNullOrEmpty(model.ROpenFileName) || !string.IsNullOrEmpty(model.RFileTransactionFileName))
            {
                model.ReturnFileTitle = model.DocNo + "_" + model.Version + "_001.pdf";                
            }
            else
            {
                model.ReturnFileTitle = "";
            }

            if (!string.IsNullOrEmpty(model.ROpenFileName) && !string.IsNullOrEmpty(model.RFileTransactionFileName))
            {
                model.ReturnFilePDF = model.DocNo + "_" + model.Version + ".pdf";
                
            }
            else
            {
                model.ReturnFilePDF = "";
            }

            return View(model);
        }

        /// <summary>
        /// 主管檢視放行 detail清單頁面
        /// </summary>
        /// <param name="model">實體類</param>
        /// <param name="pageNum">當前頁面</param>
        /// <param name="strSortExpression">排序欄位</param>
        /// <param name="strSortDirection">排序方式</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _QueryResult(NewCaseCustCondition model, int pageNum = 1, string strSortExpression = "NewCaseCustQueryVersion.CustIdNo", string strSortDirection = "asc")
        {
            return PartialView("_QueryResult", DetailSearchList(model, strSortExpression, strSortDirection, pageNum));
        }

        #endregion

        #region 自定義方法
        /// <summary>
        /// 主管檢視放行明細頁面-清單查詢方法
        /// </summary>
        /// <param name="model"></param>
        /// <param name="strSortExpression"></param>
        /// <param name="strSortDirection"></param>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public NewCaseCustViewModel DetailSearchList(NewCaseCustCondition model, string strSortExpression, string strSortDirection, int pageNum = 1)
        {
            // 查詢清單資料
            IList<CaseCustMaster> result = _CaseCustManagerBIZ.GetDetailQueryList(model, pageNum, strSortExpression, strSortDirection);

            var viewModel = new NewCaseCustViewModel()
            {
                NewCaseCustCondition = model,
                NewCaseCustQueryList = result,
            };

            // 資料清單之每頁資料數、當前頁頁碼、資料總筆數賦值
            viewModel.NewCaseCustCondition.PageSize = _CaseCustManagerBIZ.PageSize;
            viewModel.NewCaseCustCondition.CurrentPage = _CaseCustManagerBIZ.PageIndex;
            viewModel.NewCaseCustCondition.TotalItemCount = _CaseCustManagerBIZ.DataRecords;
            viewModel.NewCaseCustCondition.SortExpression = strSortExpression;
            viewModel.NewCaseCustCondition.SortDirection = strSortDirection;

            return viewModel;
        }
        public ActionResult Export(string uploadkind, string id)
        {
            string filePath = "";
            string fileName = "";
            string fileServerName = "";
            if (id == null) return null;

            if (uploadkind == "xlsx")
            {
                Guid CaseId = new Guid(id);
                CaseAccountBiz caseAccount = new CaseAccountBiz();
                ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
                CaseEdocFile caseEdocFile = caseAccount.OpenExcel(CaseId);
                string text = string.Empty;
                //
                if (caseEdocFile != null)
                {
                    byte[] file = caseEdocFile.FileObject;
                    return File(file, "application/txt", caseEdocFile.FileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }

            }
            if (uploadkind == "pdf")
            {
                Guid CaseId = new Guid(id);
                CaseAccountBiz caseAccount = new CaseAccountBiz();
                ImportEDocBiz CaseEdocFile = new ImportEDocBiz();
                CaseEdocFile caseEdocFile = caseAccount.OpenPdf(CaseId);
                string text = string.Empty;
                //
                if (caseEdocFile != null)
                {
                    byte[] file = caseEdocFile.FileObject;
                    return File(file, "application/zip", caseEdocFile.FileName);
                }
                else
                {
                    return Content("<script>alert('" + Lang.csfs_no_data + "');</script>");
                }

            }

            //else
            //{
            //    return null;
            //}
            string absoluFilePath = Path.Combine(Server.MapPath(filePath), fileServerName);
            if (!System.IO.File.Exists(absoluFilePath))
            {
                return Content("<script>alert('" + Lang.csfs_FileNotFound + "');</script>");
            }
            return File(new FileStream(absoluFilePath, FileMode.Open), "application/octet-stream", Server.UrlEncode(fileName));

        }
        /// <summary>
        /// 來文txt 
        /// 因window.open 只能打開工程下的文件，不能指定目錄，故調整為下載
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <returns></returns>
        public ActionResult OpenTxtDoc(string FileName, string ConfigPath, string FileFormat)
        {
            // 文件路徑
            string FilePath = ConfigurationManager.AppSettings[ConfigPath] + @"\";

            string FileType = string.Format("application/{0}", FileFormat);

            OpenFileStream webService = new OpenFileStream();

            webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            //dynamic stream = JsonConvert.DeserializeObject(webService.OpenFile(FilePath + FileName));
            //if (FileFormat == "SBOX")
            //{
            //    if (FileName.Length > 10) FileName = FileName.Substring(0, FileName.Length - 8) + "Sbox.csv";


            //    dynamic dStream = JsonConvert.DeserializeObject(webService.FileExist(FilePath + FileName));
            //    rtnExistROpenFileName = (dStream.Code == "0000");
            //    if (dStream.Code == "0000")
            //    {
            //        byte[] stream = webService.OpenFile(FilePath + FileName);
            //        if (stream != null)
            //        {
            //            //return File(stream.Content.ToObject<byte[]>(), FileType, FileName);
            //            return File(stream, FileType, FileName);
            //        }
            //        else
            //        {
            //            return null;
            //        }

            //    }
            //    else
            //    {
            //        return null;                    
            //    }
            //}
            //else
            //{
                byte[] stream = webService.OpenFile(FilePath + FileName);
                if (stream != null)
                {
                    //return File(stream.Content.ToObject<byte[]>(), FileType, FileName);
                    return File(stream, FileType, FileName);
                }
                else
                {
                    //return Content("<script>alert('" + Lang.csfs_FileNotFound + "');</script>");                      
                    return null ;
                }
            //}

            //if (stream.Code == "0000")


            /*
            if (FileFormat == "txt")
            {
                return File(FilePath + FileName, "application/txt", FileName);
            }
            else if (FileFormat == "pdf")
            {
                return File(FilePath + FileName, "application/pdf", FileName);
            }
            else
            {
                return null;
            }
            */
        }
        #endregion

        #region 回文檢視

        /// <summary>
        ///  回文檔案是否存在
        /// </summary>
        /// <param name="strPk"></param>
        /// <param name="strDocNo"></param>
        /// <returns></returns>
        public string IsExistReturnfile(string strPk, string strDocNo)
        {
            string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            CaseCustMaster model = _CaseCustManagerBIZ.GetReturnFilesByPk(strPk);

            // 回文檔名稱（存款帳戶開戶資料）
            string strROpenFileName = strFilePath + model.ROpenFileName;

            // 回文檔名稱（存款往來明細資料）
            string strRFileTransactionFileName = strFilePath + model.RFileTransactionFileName;

            bool rtnExistROpenFileName = false;
            bool rtnExistRFileTransactionFileName = false;

            OpenFileStream webService = new OpenFileStream();

            webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            dynamic stream = JsonConvert.DeserializeObject(webService.FileExist(strROpenFileName));
            
            rtnExistROpenFileName = (stream.Code == "0000");

            stream = JsonConvert.DeserializeObject(webService.FileExist(strRFileTransactionFileName));

            rtnExistRFileTransactionFileName = (stream.Code == "0000");

            return (rtnExistROpenFileName || rtnExistRFileTransactionFileName ? "Y" : "N");

           /*
            // 回文檔名稱（存款帳戶開戶資料）
            string strROpenFileName = strFilePath + model.ROpenFileName;

            // 回文檔名稱（存款往來明細資料）
            string strRFileTransactionFileName = strFilePath + model.RFileTransactionFileName;

            if (!System.IO.File.Exists(strROpenFileName) && !System.IO.File.Exists(strROpenFileName))
            {

                return "N";
            }
            else
            {
                return "Y";
            }
            */
        }

        public string IsExistDownloadZip(string strPk, string strDocNo, string strFileName)
        {
            string strFilePath = ConfigurationManager.AppSettings["txtReciveFilePath"] + @"\";

            CaseCustMaster model = _CaseCustManagerBIZ.GetReceiveZipByPk(strPk);

            // 下載檔是否存在
            string strDownloadFileName = strFilePath + strFileName;

            bool rtnDownloadFileName = false;

            OpenFileStream webService = new OpenFileStream();

            webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            dynamic stream = JsonConvert.DeserializeObject(webService.FileExist(strDownloadFileName));

            rtnDownloadFileName = (stream.Code == "0000");

            return (rtnDownloadFileName ? "Y" : "N");

        }
        public string IsExistDownloadfile(string strPk, string strDocNo,string strFileName )
        {
            string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            CaseCustMaster model = _CaseCustManagerBIZ.GetReturnFilesByPk(strPk);

            // 下載檔是否存在
            string strDownloadFileName = strFilePath + strFileName;

            bool rtnDownloadFileName = false;

            OpenFileStream webService = new OpenFileStream();

            webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            dynamic stream = JsonConvert.DeserializeObject(webService.FileExist(strDownloadFileName));

            rtnDownloadFileName = (stream.Code == "0000");

            return (rtnDownloadFileName ? "Y" : "N");

        }
        /// <summary>
        ///  回文檢視
        /// </summary>
        /// <param name="strPk">主鍵</param>
        /// <param name="strDocNo">流水號</param>
        /// <returns></returns>
        public ActionResult ReturnViewByPk(string strPk, string strDocNo)
        {
            string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";
            string maxFileSize = _PARMCodeBIZ.GetCodeDescByCodeNo("FileSize");

            CaseCustMaster model = _CaseCustManagerBIZ.GetReturnFilesByPk(strPk);

            // 回文檔名稱（存款帳戶開戶資料）
            //string strROpenFileName = strFilePath + model.ROpenFileName;
            string strROpenFileName = strFilePath + strDocNo+"_"+model.Version.ToString()+ "_Base.txt";

            // 回文檔名稱（存款往來明細資料）
            //string strRFileTransactionFileName = strFilePath + model.RFileTransactionFileName;
            string strRFileTransactionFileName = strFilePath + strDocNo + "_" + model.Version.ToString() + "_Detail.txt";

            // 回文檔名稱（保管箱資料）
            //string strRFileTransactionFileName = strFilePath + model.RFileTransactionFileName;
            string strSBOXFileName = strFilePath + strDocNo + "_" + model.Version.ToString() + "_SBox.txt";

            string fileName = strDocNo + "_" + model.Version.ToString();

            double limitFileSize = Convert.ToDouble(string.IsNullOrEmpty(maxFileSize) ? "100" : maxFileSize) * 1024 * 1024;
            string returnFilename = string.Empty;
            string fileType = string.Empty;

            List<string> strList = new List<string>();
            strList.Add(strROpenFileName);
            strList.Add(strRFileTransactionFileName);
            strList.Add(strSBOXFileName);

            OpenFileStream webService = new OpenFileStream();

            webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            dynamic service = JsonConvert.DeserializeObject(webService.GetFileSize(strList.ToArray()));
            byte[] stream = null;

            if (service.Code == "0000")
            {
               if (service.FileSize > limitFileSize)
               {
                  // 檔案過大，改以zip檔案方式回傳
                  returnFilename = fileName + ".zip";
                  fileType = "application/zip";

                  //stream = JsonConvert.DeserializeObject(webService.OpenZipFile(strDocNo, strFilePath, strList.ToArray()));
                  stream = webService.OpenZipFile(strDocNo, strFilePath, strList.ToArray());
               }
               else
               {
                  returnFilename = fileName + ".pdf";
                  fileType = "application/pdf";

                  //stream = JsonConvert.DeserializeObject(webService.OpenFile(strFilePath + fileName + ".pdf"));
                  stream = webService.OpenFile(strFilePath + fileName + ".pdf");
               }

               //if (stream.Code == "0000")
               if (stream != null)
               {
                  return File(stream, fileType, returnFilename);
               }
               else
               {
                    //return null;
                    return Content("<script>alert('" + Lang.csfs_FileNotFound + "');</script>");
                }
            }
            else
            {
                return Content("<script>alert('" + Lang.csfs_FileNotFound + "');</script>");
                //return null;
            }


           #region Old soure
           /*
            // 臨時文件
            string TempPath = Server.MapPath("~/Template/Template/");

            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }

            // 兩個TXT 大小
            long fileSize = FrameWork.Util.UtlFileSystem.GetFileSize(strROpenFileName) + FrameWork.Util.UtlFileSystem.GetFileSize(strRFileTransactionFileName);

            // 文件大小限制
            if (fileSize > 100 * 1024 * 1024)
            {
                #region 若兩個回文大於100MB,則下載壓縮檔

                string zipFile = TempPath + fileName + ".zip";

                // 壓縮密碼
                string strPassWord = "822822" + strDocNo.Substring(1, strDocNo.Length - 1);

                _CaseCustManagerBIZ.CreateZip(strFilePath, zipFile, strList, strPassWord);

                return File(zipFile, "application/zip", fileName + ".zip");

                #endregion
            }
            else
            {
                #region 產生pdf

                string pdfFile = strFilePath + fileName + ".pdf";
                //var document = new Document(PageSize.A4, 30f, 30f, 30f, 30f);
                //PdfWriter.GetInstance(document, new FileStream(pdfFile, FileMode.Create));

                //document.Open();
                //var bfSun = BaseFont.CreateFont(@"C:\Windows\Fonts\simfang.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //var font = new iTextSharp.text.Font(bfSun, 12f);

                //foreach (string txtFile in strList)
                //{
                //    var objReader = new StreamReader(txtFile, Encoding.UTF8);
                //    var str = "";
                //    while (str != null)
                //    {
                //        str = objReader.ReadLine();
                //        if (str != null)
                //        {
                //            document.Add(new Paragraph(str, font));
                //        }
                //    }
                //    objReader.Close();
                //}

                //document.Close();

                return File(pdfFile, "application/pdf", fileName + ".pdf");

                #endregion
            }
            */
           #endregion
        }

        #endregion

        #region 回文檔案
        /// <summary>
        ///  回文檔案
        /// </summary>
        /// <param name="strPk">主鍵</param>
        /// <param name="strDocNo">流水號</param>
        /// <returns></returns>
        public ActionResult ReturnFileDownLoad(string strPk, string strDocNo
            , string strROpenFileName
            , string strRFileTransactionFileName
            , string strReturnFilePDF
            , string strImportMessage
            , string strVersion
            )
        {

            // 回文文檔存儲路徑
            string strFilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            string fileName = strDocNo + "_" + strVersion + "_Return";

            List<string> strList = new List<string>();

            // 回文檔名稱（存款帳戶開戶資料）
            if (strROpenFileName != "")
            { strList.Add(strFilePath + strROpenFileName); }

            if (strImportMessage != "")
            { strList.Add(strFilePath + strImportMessage); }

            // 回文檔名稱（存款往來明細資料）
            if (strRFileTransactionFileName != "")
            { strList.Add(strFilePath + strRFileTransactionFileName); }

            // 回文檔名稱(PDF)
            if (strReturnFilePDF != "")
            { strList.Add(strFilePath + strReturnFilePDF); }

            OpenFileStream webService = new OpenFileStream();

            webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            byte[] stream = webService.OpenZipFile(strDocNo, strFilePath, strList.ToArray());

            if (stream != null)
            {
               return File(stream, "application/zip", fileName + ".zip");
            }
            else
            {
                return Content("<script>alert('" + Lang.csfs_FileNotFound + "');</script>");
                //return null;
            }

           /*
            // 臨時文件
            string TempPath = Server.MapPath("~/Template/Template/");

            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }

            #region 下載壓縮檔

            string zipFile = TempPath + fileName + ".zip";

            // 壓縮密碼
            string strPassWord = "822822" + strDocNo.Substring(1, strDocNo.Length - 1);

            _CaseCustManagerBIZ.CreateZip(strFilePath, zipFile, strList, strPassWord);

            return File(zipFile, "application/zip", fileName + ".zip");

            #endregion
            */
        }

        
        /// <summary>
        /// 回文檔案下載PDF
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <returns></returns>
        public ActionResult OpenPDF(string FileName)
        {
            // 文件路徑
            string FilePath = ConfigurationManager.AppSettings["txtFilePath"] + @"\";

            OpenFileStream webService = new OpenFileStream();

            webService.Url = ConfigurationManager.AppSettings["ServiceURL"];

            //dynamic stream = JsonConvert.DeserializeObject(webService.OpenFile(FilePath + FileName));
            dynamic stream = webService.OpenFile(FilePath + FileName);

            //if (stream.Code == "0000")
            if (stream != null)
            {
               return File(stream.Content.ToObject<byte[]>(), "application/pdf", FileName);
            }
            else
            {
                return Content("<script>alert('" + Lang.csfs_FileNotFound + "');</script>");
                //return null;
            }

           //return File(FilePath + FileName, "application/pdf", FileName);

        }
        #endregion
    }
}
