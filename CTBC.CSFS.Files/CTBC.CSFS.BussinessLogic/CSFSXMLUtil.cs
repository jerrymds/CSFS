using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CTBC.CSFS.BussinessLogic
{
    public class CSFSXMLUtil
    {
        // 將物件 o 序列化成 Xml 格式 (字串型態)
        public static string Serialize(object o)
        {
            if (o == null)
            {
                return string.Empty;
            }
            try
            {
                XmlSerializer ser = new XmlSerializer(o.GetType());
                StringBuilder sb = new StringBuilder();
                using (StringWriter writer = new Utf8StringWriter())
                {
                    ser.Serialize(writer, o);
                    sb.Append(writer);
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 將 xml 字串反序列化成 T 型別的物件 
        public static T Deserialize<T>(string xml)
        {
            #region old process
            //XmlDocument xdoc = new XmlDocument();   

            //try  
            //{   
            //    xdoc.LoadXml(s);   
            //    XmlNodeReader reader = new XmlNodeReader(xdoc.DocumentElement);   
            //    XmlSerializer ser = new XmlSerializer(typeof(T));   
            //    object obj = ser.Deserialize(reader);   

            //    return (T)obj;   
            //}   
            //catch  
            //{   
            //    return default(T);   
            //}  
            #endregion

            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }
            try
            {
                XmlSerializer xmlSer = new XmlSerializer(typeof(T));
                using (var stringReader = new StringReader(xml))
                {
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        var data = (T)xmlSer.Deserialize(reader);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
    }
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}