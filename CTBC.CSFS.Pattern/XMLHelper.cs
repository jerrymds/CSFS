//******************************************************************
//*  author：tangym
//*  Function：操作XML公用類
//*  Create Date：2015-2-26
//*  Modify Record：
//*<author>            <time>            <TaskID>                <desc>
//*                
//*******************************************************************
using System;
using System.Web;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CTBC.CSFS.Pattern
{
    public class XMLHelper
    {
        XmlDocument xml = new XmlDocument();
        
        /// <summary>
        /// 加載Xml文件
        /// </summary>
        /// <param name="strPath">Xml文件名</param>
        public void LoadXml(string strFileName)
        {
            try
            {
                string strDataBaseType = "SqlServer";
                if (WebConfigurationManager.AppSettings["DataBaseType"] != null)
                {
                    strDataBaseType = WebConfigurationManager.AppSettings["DataBaseType"].ToString();
                }
                string strPath = AppDomain.CurrentDomain.BaseDirectory + "SqlXML\\" + strDataBaseType + "\\";
                strPath = strPath + strFileName;
                xml.Load(strPath);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 加載Xml文件
        /// </summary>
        /// <param name="strPath">Xml文件名</param>
        public static int GetPageSize(string strNode)
        {
            try
            {
                int iSize = 10;
                string strPath = AppDomain.CurrentDomain.BaseDirectory + "XML\\PageSize.xml";
                XmlDocument xmlnew = new XmlDocument();
                xmlnew.Load(strPath);
                string strValue = "";
                XmlNode xmlnode = xmlnew.SelectSingleNode(strNode);
                if (xmlnode != null)
                {
                    strValue = xmlnode.InnerText.ToString();
                }
                int.TryParse(strValue, out iSize);
                return iSize;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 獲取Xml文件單一節點的內容
        /// </summary>
        /// <param name="strPath">節點的路徑</param>
        public string GetSingleNode(string strPath)
        {
            string strValue = "";
            try
            {
                if (xml != null)
                {
                    XmlNode xmlnode = xml.SelectSingleNode(strPath);
                    if (xmlnode != null)
                    {
                        strValue = xmlnode.InnerText.ToString();
                    }
                }
                return strValue;
            }
            catch
            {
                throw;
            }
        }
    }
}
