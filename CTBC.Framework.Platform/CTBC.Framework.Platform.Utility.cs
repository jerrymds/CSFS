using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Web;

namespace CTBC.FrameWork.Platform
{
	public class Utility
	{
		public static string AppendFolderChar(string psPath)
		{
			int ln_Length = psPath.Length;
			string ls_NewPath = psPath;
			string ls_LastChar = psPath.Substring(ln_Length - 1);
			if (ls_LastChar != "\\" && ls_LastChar != "/") ls_NewPath += "\\";
			return ls_NewPath;
		}

		public static string Escape(string inString)
		{ return HttpUtility.UrlDecode(inString); }

	}
}