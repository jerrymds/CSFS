using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.CSFS.NewHistoryFile
{
    public class NewFileLog
    {
        // Fields
        private string m_name = "";
        private string m_path = "";
        private string threadId = "";

        // Methods
        public NewFileLog(string path, string name, string _threadId)
        {
            this.m_path = path;
            this.m_name = name;
            this.threadId = _threadId;
        }

        private void CheckPath()
        {
            DirectoryInfo info = new DirectoryInfo(this.m_path);
            if (!info.Exists)
            {
                info.Create();
            }
        }

        private string GetErrorType(ErrorType errorType)
        {
            switch (errorType)
            {
                case ErrorType.None:
                    return "－－";
                case ErrorType.Information:
                    return "信息";
                case ErrorType.Warning:
                    return "警告";

                case ErrorType.Error:
                    return "錯誤";
            }
            return "";
        }

        private string GetWorkType(WorkType workType)
        {
            switch (workType)
            {
                case WorkType.Work:
                    return "工作";

                case WorkType.Data:
                    return "資料";
            }
            return "";
        }

        public void Write(WorkType workType, ErrorType errorType, string message)
        {
            if ((this.m_path != "") && (this.m_name != ""))
            {
                FileStream stream = null;
                StreamWriter writer = null;
                try
                {
                    this.CheckPath();
                    DateTime time = DateTime.Now;
                    string log = "";
                    log = ((((log + time.ToString("yyyy/MM/dd")) + "  " + time.ToString("HH:mm:ss.fff")) + "  " + GetWorkType(workType)) + "  " + GetErrorType(errorType)) + "  " + message;
                    string fileName = (this.m_path.EndsWith("\\") ? this.m_path : (this.m_path + @"\")) + this.m_name + "_" + time.ToString("dd")+ "_" + threadId + ".log";
                    FileMode mode = FileMode.Append;
                    FileInfo info = new FileInfo(fileName);
                    if (info.Exists && (info.LastWriteTime.Month != time.Month))
                    {
                        mode = FileMode.Create;
                    }
                    stream = File.Open(fileName, mode);
                    writer = new StreamWriter(stream);
                    writer.WriteLine(log);
                }
                catch
                {
                }
                try
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="errorType"></param>
        /// <param name="fileName">完整路徑，如c:\1\1.log</param>
        public void WriteLog(string message, ErrorType errorType, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                FileStream stream = null;
                StreamWriter writer = null;
                try
                {
                    FileInfo info = new FileInfo(fileName);
                    if (!info.Exists)
                    {
                        info.Create();
                    }
                    DateTime time = DateTime.Now;
                    string log = "";
                    log = (((log + time.ToString("yyyy/MM/dd")) + "  " + time.ToString("HH:mm:ss")) + "  " + GetErrorType(errorType)) + "  " + message;
                    FileMode mode = FileMode.Append;

                    if (info.Exists && (info.LastWriteTime.Month != time.Month))
                    {
                        mode = FileMode.Create;
                    }
                    stream = File.Open(fileName, mode);
                    writer = new StreamWriter(stream);
                    writer.WriteLine(log);
                }
                catch
                {
                }
                try
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
                catch
                {
                }
            }
        }

        // Nested Types
        public enum ErrorType
        {
            None,
            Information,
            Warning,
            Error
        }

        public enum WorkType
        {
            /// <summary>
            /// 工作
            /// </summary>
            Work,
            /// <summary>
            /// 數據
            /// </summary>
            Data
        }
    }
}
