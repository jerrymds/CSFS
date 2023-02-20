using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EncryptAppConfig
{
    public class Log
    {
        private string _logFile;
        private StringBuilder _allMessageForMail = new StringBuilder();
        public Log(string logFile)
        {
            _logFile = logFile;
        }

        public string AllMessageForMail
        {
            get { return _allMessageForMail.ToString(); }
        }

        public void WriteLogs(string msg)
        {
            using (StreamWriter sw = File.AppendText(_logFile))
            {
                sw.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " " + msg);
            }
            _allMessageForMail.Append(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " " + msg + "<br/>");
        }
    }
}
