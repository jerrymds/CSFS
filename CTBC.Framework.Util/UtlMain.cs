using System;

using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Windows.Forms;

namespace CTBC.FrameWork.Util
{
	/// <summary>
	/// �`�Ψ禡���O
	/// </summary>
	/// <remarks>������� : 2006/08/12</remarks>
	public class UtlMain
	{

		/// <summary>
		/// �N������m�m��
		/// </summary>
		/// <param name="frmObject">��������</param>
        //public static void CenterForm(Form frmObject)
        //{
        //    int iXPos=(Screen.PrimaryScreen.Bounds.Width-frmObject.Size.Width)/2;
        //    int iYPos=(Screen.PrimaryScreen.Bounds.Height-frmObject.Size.Height)/2;
        //    frmObject.Location = new System.Drawing.Point(iXPos, iYPos);
        //}

		/// <summary>
		/// �s�����|�r��, ���׵����O�_�t���ؿ����j�Ÿ�
		/// </summary>
		/// <param name="psRootPath">�ڸ��|</param>
		/// <param name="psSubPath">�l���|</param>
		/// <returns>������|</returns>
		public static string JoinPath(string psRootPath, string psSubPath)
		{
			if (psRootPath[psRootPath.Length-1] == Path.DirectorySeparatorChar) 
				return psRootPath + psSubPath;
			else
				return psRootPath + Path.DirectorySeparatorChar + psSubPath;
		}

		/// <summary>
		/// ���o���ε{���W��(���t���ɦW)
		/// </summary>
		/// <returns>���ε{���W��</returns>
		public static string GetAppName()
		{
			string sAppName = AppDomain.CurrentDomain.FriendlyName;
			return UtlString.Left(sAppName, sAppName.Length - 4);
		}

		/// <summary>
		/// ���o���ε{�����|(1)
		/// </summary>
		/// <returns>���ε{�����|(���t�����ɦW)</returns>
		public static string GetAppPath()
		{
            //�T�w�A�Ȭ�������m
            if (Application.StartupPath.Contains("CTCB.WII.Service"))
            {
                System.IO.DirectoryInfo startUp = new DirectoryInfo(Application.StartupPath);

                string path = startUp.Parent.FullName + Path.DirectorySeparatorChar + "LOGS" + Path.DirectorySeparatorChar + GetAppName();

                if (!System.IO.Directory.Exists(path)) { System.IO.Directory.CreateDirectory(path); }

                return path + Path.DirectorySeparatorChar;
            }
            else
            {
                return Application.StartupPath + Path.DirectorySeparatorChar;
            }
		}
		/// <summary>
		/// ���o���ε{�����|(2)
		/// </summary>
		/// <param name="pbWithExeName">�O�_�t�����ɦW</param>
		/// <returns>���ε{�����|(�����ɦW)</returns>
		public static string GetAppPath(bool pbWithExeName)
		{
            if (GetString(AppDomain.CurrentDomain.DynamicDirectory).ToUpper().Contains("ASP.NET"))
            {
                string[] strFriendlyName = AppDomain.CurrentDomain.FriendlyName.Split(new string[] { "/" }, StringSplitOptions.None);
                string[] strAppName = strFriendlyName[strFriendlyName.Length - 1].Split(new string[] { "-" }, StringSplitOptions.None);

                return AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\" + strAppName[0] + "123";
            }
            else 
            {
                if (pbWithExeName)
                {
                    return GetAppPath() + AppDomain.CurrentDomain.FriendlyName;
                }
                else
                {
                    return GetAppPath();
                }
            }
		}

        public static string GetString(object objValue)
        {
            string strValue = "";

            if (objValue != null)
            {
                if (objValue != DBNull.Value)
                {
                    strValue = objValue.ToString().Trim();
                }
            }

            return strValue;
        }

		/// <summary>
		/// ���o���ε{�����|(3)
		/// </summary>
		/// <param name="psExtName">�۩w���ɦW</param>
		/// <returns>���ε{�����|+�۩w���ɦW</returns>
        /// �P�_�O�o�Ӥ�O�_���ɮ�
		public static string GetAppPath(string psExtName)
		{
            DateTime time = DateTime.Now;
			string sTemp = GetAppPath(true);
            string fileName = UtlString.Left(sTemp, sTemp.Length - 3) + "_" + time.ToString("dd") + "." + psExtName;
            FileInfo info = new FileInfo(fileName);

            if (info.Exists)
            {
                if (info.LastWriteTime.Month != time.Month) { info.Delete(); }
            }

            return fileName;
		}

		/// <summary>
		/// �C�L�����T��(1)
		/// </summary>
		/// <param name="poPrintObject">�T������</param>
		public static void DebugPrint(object poPrintObject)
		{
			// ���Listeners[0]�b��L�{���I�s�ɤ~�|���`��ܩ�Debug����
			Debug.Listeners[0].WriteLine(poPrintObject);
		}
		/// <summary>
		/// �C�L�����T��(2)
		/// </summary>
		/// <param name="poPrintObject">�T������</param>
		/// <param name="pbPrintTime">�O�_��ܮɶ�</param>
		public static void DebugPrint(object poPrintObject, bool pbPrintTime)
		{
			if (pbPrintTime)
				DebugPrint(DateTime.Now + " " + poPrintObject.ToString());
			else
				DebugPrint(poPrintObject);
		}

		/// <summary>
		/// ����~�����ε{��(1)
		/// </summary>
		/// <param name="psExeFilePath">�����ɦ�m</param>
        public static void Shell(string psExeFilePath)
        {
            try
            {
                Process.Start(psExeFilePath);
            }
            catch (Exception e)
            {
                throw e;
                //UtlDialog.ShowErrorMsg(e, "< ������ : > " + psExeFilePath);
            }
        }
		/// <summary>
		/// ����~�����ε{��(2)
		/// </summary>
		/// <param name="psExeFilePath">�����ɦ�m</param>
		/// <param name="psArguments">�ѼƦr��</param>
        public static void Shell(string psExeFilePath, string psArguments)
        {
            try
            {
                Process.Start(psExeFilePath, psArguments);
            }
            catch (Exception e)
            {
                throw e;
                //UtlDialog.ShowErrorMsg(e, "< ������ : > " + psExeFilePath + " < �Ѽ� : > " + psArguments);
            }
        }

		/// <summary>
		/// �ˬd�ۦP�����ε{���O�_�w�Ұ�
		/// </summary>
		/// <returns>True/False</returns>
		public static bool PreInstance()
		{
			Process oProcess = Process.GetCurrentProcess();
			string sProcName = oProcess.ProcessName;

			if (Process.GetProcessesByName(sProcName).Length > 1)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Ū�����ε{���]�w��
		/// </summary>
		/// <param name="psKey">�]�w���</param>
		/// <returns>�]�w��</returns>
		/// <remarks>Ū��App.config�ɮפ�[configuration][appSettings]���]�w��(�PDynamic Properties�@�γ]�w�϶�)</remarks>
		public static string GetAppSetting(string psKey)
		{
			return ConfigurationSettings.AppSettings.Get(psKey);
		}

		/// <summary>
		/// ��Exception����զ����~�T���r��
		/// </summary>
		/// <param name="e">Exception����</param>
		/// <returns>���~�T��</returns>
		public static string ComposeErrMsg(Exception e)
		{
			return e.GetHashCode().ToString() + " : " + e.ToString();
		}

	}


}