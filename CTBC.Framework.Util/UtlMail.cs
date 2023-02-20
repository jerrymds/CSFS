/// <summary>
/// 郵件
/// </summary>

using System.Collections;
using System.Net.Mail;
using System;
using CTBC.CSFS.Pattern;

namespace CTBC.FrameWork.Util
{
	public class UtlMail
	{
		#region 全域變數


		/// <summary>
		/// From email address
		/// </summary>
		public string _FromEmail;

		/// <summary>
		/// From name
		/// </summary>
		public string _FromName;

		/// <summary>
		/// To email address and ToName ,split with '/',Type of strign Array
		/// </summary>
		public string[] _ToEmailAndName;

		/// <summary>
		/// Email subject
		/// </summary>
		public string _Subject;

		/// <summary>
		/// Email message body
		/// </summary>
		public string _EmailBody;

		/// <summary>
		/// Is email message body HTML
		/// </summary>
		public bool _IsBodyHtml;

		/// <summary>
		/// Email message file attachments,Type of strign Array
		/// </summary>
		public string[] _Attachments;

		/// <summary>
		/// SMTP email server address
		/// </summary>
		public string _EmailServer;

		/// <summary>
		/// SMTP email server login name
		/// </summary>
		public string _LoginName;

		/// <summary>
		/// SMTP email server login password
		/// </summary>
		public string _LoginPassword;

		#endregion

		#region 屬性設置(Get,Set)

		#endregion

		#region Public Method

		/// <summary>
		/// 發送郵件
		/// </summary>
		/// <param name="strMessage">信息</param>
		/// <param name="htblSendMailSetting">郵件設置</param>
		/// <returns>是否成功</returns>
		/// <remarks>add by sky 2011/12/08</remarks>
		public static bool SendEmail(string strMessage, Hashtable htblSendMailSetting)
		{
			// Service名稱
			string strServiceName = (string)htblSendMailSetting["ServiceName"] + " - ";

			try
			{
				SmtpClient mailClient = new SmtpClient();

				// 取得或設定用於 SMTP 交易的主機名稱或 IP 位址。
				mailClient.Host = (string)htblSendMailSetting["MailServer"];

				// E-Mail主旨
				string strMailTitle = (string)htblSendMailSetting["MailEnv"];

				// E-Mail信息
				string strMailBody = strServiceName + strMessage + "--" + (string)htblSendMailSetting["MailEnv"];

				mailClient.Send((string)htblSendMailSetting["MailFrom"], (string)htblSendMailSetting["MailTo"], strMailTitle, strMailBody);
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// MailHelper建構子
		/// </summary>
		/// <remarks>add by mel 20120118</remarks>
		public UtlMail()
		{
			//參數初始
			_FromEmail = "";
			_FromName = "";
			_Subject = "";
			_EmailBody = "";
			_IsBodyHtml = false;
			_EmailServer = "";
			_LoginName = "";
			_LoginPassword = "";
		}

		/// <summary>
		///  MailHelper建構子
		/// </summary>
		/// <param name="m_FromEmail">From email address</param>
		/// <param name="m_FromName">From name</param>
		/// <param name="m_ToEmailAndName">To email address and ToName ,split with '/',Type of strign Array</param>
		/// <param name="m_Subject">Email subject</param>
		/// <param name="m_EmailBody">Email message body</param>
		/// <param name="m_IsBodyHtml">Is email message body HTML</param>
		/// <param name="m_Attachments">Email message file attachments</param>
		/// <param name="m_EmailServer">SMTP email server address</param>
		/// <param name="m_LoginName">SMTP email server login name</param>
		/// <param name="m_LoginPassword">SMTP email server login password</param>
		public UtlMail(
			string m_FromEmail
			, string m_FromName
			, string[] m_ToEmailAndName
			, string m_Subject
			, string m_EmailBody
			, bool m_IsBodyHtml
			, string[] m_Attachments
			, string m_EmailServer
			, string m_LoginName
			, string m_LoginPassword)
		{
			_FromEmail = m_FromEmail;
			_FromName = m_FromName;
			_ToEmailAndName = m_ToEmailAndName;
			_Subject = m_Subject;
			_EmailBody = m_EmailBody;
			_IsBodyHtml = m_IsBodyHtml;
			_Attachments = m_Attachments;
			_EmailServer = m_EmailServer;
			_LoginName = m_LoginName;
			_LoginPassword = m_LoginPassword;


		}

		/// <summary>
		/// MailHelper建構子
		/// </summary>
		/// <param name="m_FromEmail">From email address</param>
		/// <param name="m_FromName">From name</param>
		/// <param name="m_ToEmailAndName">To email address and ToName ,split with '/',Type of strign Array</param>
		/// <param name="m_Subject">Email subject</param>
		/// <param name="m_EmailBody">Email message body</param>
		/// <param name="m_IsBodyHtml">Is email message body HTML</param>
		/// <param name="m_Attachments">Email message file attachments</param>
		/// <param name="m_EmailServer">SMTP email server address</param>
		public UtlMail(
			string m_FromEmail
			, string m_FromName
			, string[] m_ToEmailAndName
			, string m_Subject
			, string m_EmailBody
			, bool m_IsBodyHtml
			, string[] m_Attachments
			, string m_EmailServer)
		{
			_FromEmail = m_FromEmail;
			_FromName = m_FromName;
			_ToEmailAndName = m_ToEmailAndName;
			_Subject = m_Subject;
			_EmailBody = m_EmailBody;
			_IsBodyHtml = m_IsBodyHtml;
			_Attachments = m_Attachments;
			_EmailServer = m_EmailServer;
			_LoginName = "";
			_LoginPassword = "";


		}



		/// <summary>
		/// 執行送email
		/// </summary>
		/// <returns>TRUE if the email sent successfully, FALSE otherwise</returns>
		public bool SendMail()
		{
			try
			{
				// setup email header
				System.Net.Mail.MailMessage _MailMessage = new System.Net.Mail.MailMessage();

				// Set the message sender
				// sets the from address for this e-mail message. 
				_MailMessage.From = new System.Net.Mail.MailAddress(_FromEmail, _FromName);

				// Sets the address collection that contains the recipients of this e-mail message.
				if (_ToEmailAndName != null && _ToEmailAndName.Length > 0)
				{
					foreach (string emailstring in _ToEmailAndName)
						_MailMessage.To.Add(new System.Net.Mail.MailAddress(
							emailstring.Split(new char[] { '/' })[0].ToString()
							, emailstring.Split(new char[] { '/' })[1].ToString()));
				}
				else
				{
					throw new Exception("no  the To address for this e-mail message.");
				}


				// sets the message subject.
				_MailMessage.Subject = _Subject;
				// sets the message body. 
				_MailMessage.Body = _EmailBody;
				// sets a value indicating whether the mail message body is in Html. 
				// if this is false then ContentType of the Body content is "text/plain". 
				_MailMessage.IsBodyHtml = _IsBodyHtml;

				// add all the file attachments if we have any
				if (_Attachments != null && _Attachments.Length > 0)
					foreach (string _Attachment in _Attachments)
						_MailMessage.Attachments.Add(new System.Net.Mail.Attachment(_Attachment));

				// SmtpClient Class Allows applications to send e-mail by using the Simple Mail Transfer Protocol (SMTP).
				System.Net.Mail.SmtpClient _SmtpClient = new System.Net.Mail.SmtpClient(_EmailServer);

				//Specifies how email messages are delivered. Here Email is sent through the network to an SMTP server.
				_SmtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

				//若沒輸入_LoginName則無須建立NetworkCredential
				if (_LoginName.Length != 0)
				{
					// Some SMTP server will require that you first authenticate against the server.
					// Provides credentials for password-based authentication schemes such as basic, digest, NTLM, and Kerberos authentication.
					System.Net.NetworkCredential _NetworkCredential = new System.Net.NetworkCredential(_LoginName, _LoginPassword);
					_SmtpClient.UseDefaultCredentials = false;
					_SmtpClient.Credentials = _NetworkCredential;
				}

				//Let's send it
				_SmtpClient.Send(_MailMessage);

				// Do cleanup
				_MailMessage.Dispose();
				_SmtpClient = null;

			}
			catch (Exception _Exception)
			{
				// Error
				Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
			}
			return true;

		}

		public static bool SendMail(string[] toEmailAndName, string message, string[] attachments)
		{
			try
			{
				// setup email header
				System.Net.Mail.MailMessage _MailMessage = new System.Net.Mail.MailMessage();

				// Set the message sender
				// sets the from address for this e-mail message. 
				_MailMessage.From = new System.Net.Mail.MailAddress(Config.GetValue("Mail_FromEmail"), Config.GetValue("Mail_FromName"));

				// Sets the address collection that contains the recipients of this e-mail message.
				if (toEmailAndName != null && toEmailAndName.Length > 0)
				{
					foreach (string emailstring in toEmailAndName)
						_MailMessage.To.Add(new System.Net.Mail.MailAddress(
							emailstring.Split(new char[] { '/' })[0].ToString()
							, emailstring.Split(new char[] { '/' })[1].ToString()));
				}
				else
				{
					throw new Exception("no  the To address for this e-mail message.");
				}


				// sets the message subject.
				_MailMessage.Subject = Config.GetValue("Mail_Subject");
				// sets the message body. 
				_MailMessage.Body = message;
				// sets a value indicating whether the mail message body is in Html. 
				// if this is false then ContentType of the Body content is "text/plain". 
				_MailMessage.IsBodyHtml = bool.Parse(Config.GetValue("Mail_IsBodyHtml"));

				// add all the file attachments if we have any
				if (attachments != null && attachments.Length > 0)
					foreach (string _Attachment in attachments)
						_MailMessage.Attachments.Add(new System.Net.Mail.Attachment(_Attachment));

				// SmtpClient Class Allows applications to send e-mail by using the Simple Mail Transfer Protocol (SMTP).
				System.Net.Mail.SmtpClient _SmtpClient = new System.Net.Mail.SmtpClient(Config.GetValue("Mail_EmailServer"));

				//Specifies how email messages are delivered. Here Email is sent through the network to an SMTP server.
				_SmtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

				//若沒輸入_LoginName則無須建立NetworkCredential
				if (Config.GetValue("Mail_LoginName").Length != 0)
				{
					// Some SMTP server will require that you first authenticate against the server.
					// Provides credentials for password-based authentication schemes such as basic, digest, NTLM, and Kerberos authentication.
					System.Net.NetworkCredential _NetworkCredential = new System.Net.NetworkCredential(Config.GetValue("Mail_LoginName"), Config.GetValue("Mail_LoginPassword"));
					_SmtpClient.UseDefaultCredentials = false;
					_SmtpClient.Credentials = _NetworkCredential;
				}

				//Let's send it
				_SmtpClient.Send(_MailMessage);

				// Do cleanup
				_MailMessage.Dispose();
				_SmtpClient = null;

			}
			catch (Exception e)
			{
				throw e;
			}

			return true;
		}

        public static bool SendEmail(string mailFrom, string[] mailFromTo, string subject, string body, string host)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                MailAddress mailAddress = new MailAddress(mailFrom, "", System.Text.Encoding.UTF8);
                mailMessage.From = mailAddress;
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = false;
                foreach (string mail in mailFromTo)
                {
                    mailMessage.To.Add(mail);
                }
                //配置發送服務器
                SmtpClient smtpClient = new SmtpClient(host);
                //發送郵件
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// add by spring 20171130
        /// </summary>
        /// <param name="mailFrom"></param>
        /// <param name="mailFromTo"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static bool SendEmailIsBodyHtml(string mailFrom, string[] mailFromTo, string subject, string body, string host)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                MailAddress mailAddress = new MailAddress(mailFrom, "", System.Text.Encoding.UTF8);
                mailMessage.From = mailAddress;
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                foreach (string mail in mailFromTo)
                {
                    mailMessage.To.Add(mail);
                }
                //配置發送服務器
                SmtpClient smtpClient = new SmtpClient(host);
                //發送郵件
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
		#endregion

		#region Private Method

		#endregion
	}
}
