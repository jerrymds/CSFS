using CTBC.CSFS.BussinessLogic;
using CTBC.WinExe.WarningReturnFile.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.WarningReturnFile
{
    class Program
    {
        public string _taskname = "CTBC.WinExe.WarningReturnFile";
        ReturnFileBiz _biz = new ReturnFileBiz();
        static void Main(string[] args)
        {
            Program mainProgram = new Program();
            //mainProgram.test();
            mainProgram.Process();
        }

        private void Process()
        {

            try
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));
                // 取得目前待產的明細... WarningDetails.Status='Z01' AND FixSend<>'1'
                var details = _biz.getDetails();

                if (details.Count() > 0)
                {
                    _biz.WriteLog(string.Format("共有{0}個案件, 需要產出報表", details.Count().ToString()) );
                    _biz.WriteLog("將案件先行抓住, 將FixSend改為9");
                    _biz.holdDetails();
                }
                else
                {
                    _biz.WriteLog("目前無案件需要產生, 結束程式");
                    return;
                }



                DataTable dtParm = _biz.getParm();
                DateTime thenow = DateTime.Now;

                string outputDir = ConfigurationManager.AppSettings["OutputDir"].ToString();

                string filename = _biz.getFileName(outputDir);


                

                foreach (var d in details)
                {
                    try
                    {
                        _biz.WriteLog(string.Format("\t進行案件 {0}  ", d.DocNo));


                        string output = _biz.ouputTxt(d, dtParm);

                        File.AppendAllText(filename, output, Encoding.UTF8);
                        // 將WarningDetails.FixSend='1' , 表示產檔結束
                        _biz.updateFixSend(d.SerialID, "1");
                    }
                    catch (Exception)
                    {
                        
                        throw;
                    }

                }

                
                
            }
            catch (Exception ex)
            {
                
                throw;
            }

        }


    }
}
