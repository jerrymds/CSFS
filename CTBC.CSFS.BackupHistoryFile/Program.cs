using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.BackupHistoryFile
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Log.config"));

            BackupFileBiz bfb = new BackupFileBiz();
            var Settings = bfb.getHistory_BackupSettingFile().Where(x=>x.Enable); //取出目前要執行的

            bfb.WriteLog(string.Format("\t 備份實體作業 , 共計{0} 目錄 ", Settings.Count().ToString()));

            foreach(var setting in Settings)
            {
                bfb.WriteLog("");
                bfb.WriteLog(string.Format("\t開始進行 , {0}", setting.DirPath));
                DateTime sDate = new DateTime(2010, 1, 1);
                DateTime thenow = DateTime.Now;
                DateTime eDate = thenow;
                switch (setting.TimeFreq.ToUpper())
                {
                    case "D":
                        eDate = thenow.AddDays(-1 * setting.TimeFreqValue);
                        break;
                    case "M":
                        eDate = thenow.AddMonths(-1 * setting.TimeFreqValue);
                        break;
                    case "Y":
                        eDate = thenow.AddYears(-1 * setting.TimeFreqValue);
                        break;
                }

                try
                {
                    // 產生Archive目錄
                    bfb.makeArchive(setting.DirPath);

                    // 找出檔案
                    var files = bfb.TraverseTree(setting.DirPath, sDate, eDate, "Archive").OrderBy(x=>x.LastWriteTime).ToList();
                    bfb.WriteLog(string.Format("\t\t取得{0} 份檔案", files.Count()));

                    if (files.Count() == 0)
                    {
                        bfb.WriteLog(string.Format("\t\t目前無檔案可以搬移"));
                        continue;
                    }

                    // 修正sDate
                    sDate = files.First().LastWriteTime;

                    bfb.WriteLog(string.Format("\t\t備份實體作業{0} , 起{1} ~ 迄{2}", setting.DirPath, sDate.ToShortDateString(), eDate.ToShortDateString()));

                    // 檔案搬到Temp目錄
                    List<string> moveMessage = bfb.Move2Temp(setting.DirPath, files, setting.isDelete);
                    if( moveMessage.Count()==0)
                        bfb.WriteLog(string.Format("\t\t檔案搬移至Temp成功, 共計{0}個檔", files.Count()));
                    

                    // 壓縮
                    try
                    {
                        string zipFileName = sDate.ToString("yyyyMMdd") + "-" + eDate.ToString("yyyyMMdd") + ".zip";
                        ZipHelper.CreateZip(setting.DirPath + "\\Temp", setting.DirPath + "\\Archive", setting.DirPath + "\\Archive\\" +zipFileName);
                        bfb.WriteLog(string.Format("\t\t壓縮成功, 檔名{0}", setting.DirPath + "\\Archive\\" + zipFileName));
                    }
                    catch (Exception exZip)
                    {
                        bfb.WriteLog(string.Format("\t\t壓縮時發生錯誤 {0} {1}", setting.DirPath, exZip.Message.ToString()));                        
                    }

                    // 刪除
                    if( setting.isDelete)
                    {
                        // 檔案刪除
                        //files.ForEach(x => x.Delete());
                        // Temp 目錄刪除
                        bfb.DeleteTemp(setting.DirPath);
                    }
                }
                catch(Exception ex)
                {
                    bfb.WriteLog(string.Format("\t\t發生錯誤 {0} {1}", setting.DirPath, ex.Message.ToString()));
                }

                bfb.WriteLog(string.Format("\t執行完成, {0}",setting.DirPath));
            }
        }
    }
}
