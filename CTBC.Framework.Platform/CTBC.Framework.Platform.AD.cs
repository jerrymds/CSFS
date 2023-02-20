using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CTBC.FrameWork.Platform
{
	public class AD
	{
		// get account info from active directory and return a XML document
		public XmlDocument GetAccountInfo(string account)
		{
			XmlDocument AccountInfo = new XmlDocument();
			return AccountInfo;
		}
	}
}
