
using CTBC.CSFS.Pattern;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CTBC.CSFS.BackupHistoryFile
{
    public class BackupFileBiz : BaseBusinessRule
    {
        public List<BackupSettingFile> getHistory_BackupSettingFile()
        {
            List<BackupSettingFile> result = new List<BackupSettingFile>();

            using (IDbConnection dbConnection = OpenConnection())
            {
                string sqlStr = @"SELECT * FROM [dbo].[History_BackupSettingFile]";

                result = base.SearchList<BackupSettingFile>(sqlStr).ToList();

            };
            return result;
        }


        public void makeArchive(string baseDir)
        {
            if(! Directory.Exists(baseDir + "\\Archive"))
            {
                Directory.CreateDirectory(baseDir + "\\Archive");
            }
        }

        public List<FileInfo> TraverseTree(string root, DateTime sDate, DateTime eDate, string IngoreFoler)
        {
            List<FileInfo> Result = new List<FileInfo>();
            // Data structure to hold names of subfolders to be
            // examined for files.
            Stack<string> dirs = new Stack<string>(20);

            if (!System.IO.Directory.Exists(root))
            {
                throw new ArgumentException();
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir);
                }

                catch (UnauthorizedAccessException e)
                {

                    Console.WriteLine(e.Message);
                    continue;
                }

                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                foreach (string file in files)
                {
                    try
                    {
                        // Perform whatever action is required in your scenario.
                        System.IO.FileInfo fi = new System.IO.FileInfo(file);
                        //Console.WriteLine("{0}: {1}, {2}", fi.Name, fi.Length, fi.CreationTime);
                        if (fi.LastWriteTime >= sDate && fi.LastWriteTime < eDate)
                            Result.Add(fi);
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
                foreach (string str in subDirs.Where(x => !x.EndsWith(IngoreFoler)))
                    dirs.Push(str);
            }

            return Result;
        }


        /// <summary>
        /// log 記錄
        /// </summary>
        /// <param name="msg"></param>
        public void WriteLog(string msg)
        {
            if (Directory.Exists(@".\Log") == false)
            {
                Directory.CreateDirectory(@".\Log");
            }
            LogManager.Exists("DebugLog").Debug(msg);
        }

        internal void DeleteTemp(string baseDir)
        {
            string tempPath = Path.Combine(baseDir, "Temp");
            DirectoryInfo directory = new DirectoryInfo(tempPath);
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            directory.Delete();
        }

        internal List<string> Move2Temp(string baseDir, List<FileInfo> files, bool isDelete)
        {
            List<string> message = new List<string>();

            string tempPath = Path.Combine(baseDir, "Temp");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            var di = new DirectoryInfo(baseDir);
            foreach (FileInfo fi in files)
            {
                try
                {
                    // 判斷目錄若不是, 則當場新增...
                    string newDir = fi.Directory.FullName.Replace(baseDir,tempPath);
                    if (!Directory.Exists(newDir))
                        Directory.CreateDirectory(newDir);
                    if (isDelete)
                        File.Move(fi.FullName, newDir + "\\" + fi.Name);
                    else
                        File.Copy(fi.FullName, newDir + "\\" + fi.Name);
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("搬移檔案至Temp目錄失敗 {0}, {1}", baseDir, ex.Message.ToString()));
                    message.Add(string.Format("搬移檔案至Temp目錄失敗 {0}, {1}", baseDir, ex.Message.ToString()));
                }
            }
            return message;
        }
    }
}
