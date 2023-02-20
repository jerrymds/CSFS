using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.CSFS.Pattern;
using System.Data;
using System.Net.Mail;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.BussinessLogic
{
    public class CSFSModificationNoticeBIZ : CommonBIZ
    {
        #region 全域變數
        public CSFSModificationNoticeBIZ(AppController appController)
            : base(appController)
        {
        }

        public CSFSModificationNoticeBIZ()
            : base()
        {
        }

        public CSFSModificationNoticeBIZ(string _conectionstring)
            : base(_conectionstring)
        {
        }
        #endregion

        private DataTable _beforeData; //修改前的資訊

        private DataTable _afterData; //修改前的資訊

        /// <summary>
        /// 儲存修改前的資訊
        /// </summary>
        public void SaveBeforeData(string tablename, string wherestring, CommandParameterCollection Parameters, IDbTransaction dbTransaction = null)
        {
            try
            {
                string sql = "select * from " + tablename + " " + wherestring;

                base.Parameter.Clear();
                base.Parameter = Parameters;

                if (dbTransaction == null)
                {
                    _beforeData = base.Search(sql);
                }
                else
                {
                    _beforeData = base.Search(sql, dbTransaction);
                }
            }catch
            {

            }
        }

        /// <summary>
        /// 儲存修改後的資訊
        /// </summary>
        public void SaveAfterData(string tablename, string wherestring, CommandParameterCollection Parameters, IDbTransaction dbTransaction = null)
        {
            try
            {
                string sql = "select * from " + tablename + " " + wherestring;

                base.Parameter.Clear();
                base.Parameter = Parameters;

                if (dbTransaction == null)
                {
                    _afterData = base.Search(sql);
                }
                else
                {
                    _afterData = base.Search(sql, dbTransaction);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 寄送Mail
        /// </summary>
        public void SendMail(string Subject,string Action)
        {
            MailMessage message = new MailMessage();
            message.Subject = "【" + Subject + "】異動通知 - " + Action;
            message.IsBodyHtml = true;
            message.BodyEncoding = Encoding.UTF8;
            #region Before內容
            string beforeHead = "";
            string beforeBody = "";
            if (_beforeData != null && _beforeData.Rows.Count > 0)
            {
                beforeHead = "<thead style=\"font-size: 11px;font-family: Arial;background-color: #DAECC6;height: 30;text-align: center;\"><tr>";
                for (int j = 0; j < _beforeData.Columns.Count; j++)
                {
                    beforeHead += "<th style=\"height: 25px;white-space: nowrap;\">" + _beforeData.Columns[j].ColumnName + "</th>";
                }
                beforeHead += "</tr></thead>";

                beforeBody = "<tbody>";
                for (int i = 0; i < _beforeData.Rows.Count; i++)
                {
                    beforeBody += "<tr style=\"font-size: 11px;font-family: Arial;background-color: #FFFFEE;text-align: center;\">";
                    for (int j = 0; j < _beforeData.Columns.Count; j++)
                    {
                        beforeBody += "<td style=\"font-size: 11px;font-family: Arial;background-color: #FFFFEE;height: 30;\">" + _beforeData.Rows[i][j].ToString() + "</td>";
                    }
                    beforeBody += "</tr>";
                }
                beforeBody += "</tbody>";
                message.Body += "<span style=\"font-size: 14px;font-family: Arial;font-weight:bold;\">異動前</span><br/><table style =\"font-size: 11px;font-family: Arial;color: #000000;background-color: #DAECC6;border-color: #8FBC8B;height: 30;width: 100%;\" border=\"1\" cellpadding=\"3\" cellspacing=\"0\">" + beforeHead + beforeBody + "</table><br/>";
            }
            #endregion

            #region After內容
            string afterHead = "";
            string afterBody = "";
            if (_afterData != null && _afterData.Rows.Count > 0)
            {
                afterHead = "<thead style=\"font-size: 11px;font-family: Arial;background-color: #DAECC6;height: 30;text-align: center;\"><tr>";
                for (int j = 0; j < _afterData.Columns.Count; j++)
                {
                    afterHead += "<th style=\"height: 25px;white-space: nowrap;\">" + _afterData.Columns[j].ColumnName + "</th>";
                }
                afterHead += "</tr></thead>";

                afterBody = "<tbody>";
                for (int i = 0; i < _afterData.Rows.Count; i++)
                {
                    afterBody += "<tr style=\"font-size: 11px;font-family: Arial;background-color: #FFFFEE;text-align: center;\">";
                    for (int j = 0; j < _afterData.Columns.Count; j++)
                    {
                        afterBody += "<td style=\"font-size: 11px;font-family: Arial;background-color: #FFFFEE;height: 30;\">" + _afterData.Rows[i][j].ToString() + "</td>";
                    }
                    afterBody += "</tr>";
                }
                afterBody += "</tbody>";
                message.Body += "<span style=\"font-size: 14px;font-family: Arial;font-weight:bold;\">異動後</span><br/><table style =\"font-size: 11px;font-family: Arial;color: #000000;background-color: #DAECC6;border-color: #8FBC8B;height: 30;width: 100%;\" border=\"1\" cellpadding=\"3\" cellspacing=\"0\">" + afterHead + afterBody + "</table>";
            }
            #endregion
            if (!string.IsNullOrEmpty(message.Body))
            {
                message.Body += "<br/><br/><br/>資安等級：□極機密/□機密/▓內部/□公開<br/>解密日期：0 年 0月 0日"; //20141030 for 20141108

                message.From = new MailAddress("CSFS@ctbcbank.com", "外來文系統");

                string _maillist = ReturnMailList();

                string[] _mailarray = _maillist.Split(';');

                if (_mailarray.Length > 0)
                {
                    foreach (string _s in _mailarray)
                    {
                        message.To.Add(new MailAddress(_s));
                    }
                }
                else
                {
                    message.To.Add(new MailAddress("CSFS@ctbcbank.com"));
                }

                using (SmtpClient client = new SmtpClient())
                {
                    try
                    {
                        client.Host = Config.Mail_FromName;
                        client.Port = Config.Mail_EmailPort;
                        client.Send(message);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 回傳放在PARMCODE的設定
        /// </summary>
        /// <returns></returns>
        public static string ReturnMailList()
        {
            PARMCodeBIZ parm = new PARMCodeBIZ();
            IList<PARMCode> _list = parm.GetCodeData("ParamModificationMail");
            string FN = "";
            if (_list.Count > 0)
            {
                FN = _list[0].CodeDesc;
            }

            return FN;
        }
    }
}
