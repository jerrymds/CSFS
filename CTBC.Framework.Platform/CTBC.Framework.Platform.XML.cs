using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CTBC.FrameWork.Platform
{
	public class XML
	{
		// create XML DOM
		public static XmlDocument Create() {
			XmlDocument lo_DOM = new XmlDocument();
			return lo_DOM;
		}

		// create XML DOM and load remote XML file
		public static XmlDocument Create_Load(string asFilePath) {
			XmlDocument lo_DOM = new XmlDocument();
			try { lo_DOM.Load(asFilePath); } catch (Exception) { throw; };
			return lo_DOM;
		}

		// create XML DOM and load XML string
		public static XmlDocument Create_LoadXML(string asXML) {
			XmlDocument lo_DOM = new XmlDocument();
			try { lo_DOM.LoadXml(asXML); } catch (Exception) { throw; };
			return lo_DOM;
		}

		// append new node to XML node
		public static XmlNode AddNode(XmlNode aoNode, string asName, string asValue = null) {
			XmlNode lo_Node = null;
			try {
				lo_Node = aoNode.OwnerDocument.CreateElement(asName);
				aoNode.AppendChild(lo_Node);
				if (asValue != null) lo_Node.InnerText = asValue;
			} catch (Exception) { }
			return lo_Node;
		}

		// add new attribute to XML node
		public static XmlAttribute AddAttribute(XmlNode aoNode, string asName, string asText)
		{
			XmlAttribute lo_Attribute = null;
			if (asName != "") lo_Attribute = (XmlAttribute)aoNode.Attributes.GetNamedItem(asName);
			if (lo_Attribute == null)
			{
				lo_Attribute = aoNode.OwnerDocument.CreateAttribute(asName);
				aoNode.Attributes.SetNamedItem(lo_Attribute);
			}
			lo_Attribute.InnerText = asText;
			return lo_Attribute;
		}

		// get XML node attribute
		public static string GetAttribute(XmlNode aoNode, string asName, string asAlternateName = null)
		{
			XmlAttribute lo_Attribute = (asName == "" ? null : (XmlAttribute)aoNode.Attributes.GetNamedItem(asName));
			if (lo_Attribute == null) lo_Attribute = (asAlternateName == null || asAlternateName == "" ? null : (XmlAttribute)aoNode.Attributes.GetNamedItem(asAlternateName));
			if (lo_Attribute == null) return "";
			return lo_Attribute.InnerText;
		}

		// get XML excludes the root
		public static string GetChildXML(XmlNode aoNode) {
			string ls_XML = "";
			if (aoNode.ChildNodes.Count > 0)
				for (int i = 0; i < aoNode.ChildNodes.Count; i++) ls_XML += aoNode.ChildNodes.Item(i).OuterXml;
			return ls_XML;
		}

		// set node value
		public static XmlNode SetNodeValue(XmlNode aoParentNode, string asChildNode, string asText) {
			XmlNode lo_Node = aoParentNode.SelectSingleNode(asChildNode);
			if (lo_Node == null) {
				lo_Node = aoParentNode.OwnerDocument.CreateElement(asChildNode);
				aoParentNode.AppendChild(lo_Node);
			}
			lo_Node.InnerText = asText;
			return lo_Node;
		}

		// get node value
		public static string GetNodeValue(XmlNode aoParentNode, string asChildNode) {
			XmlNode lo_Node = aoParentNode.SelectSingleNode(asChildNode);
			if (lo_Node == null) return "";
			return lo_Node.InnerText;
		}

		// serialize object to XML
		public static string Object_2_XML(object o)
		{
			XmlDocument Doc = new XmlDocument();
			XmlSerializer S = new XmlSerializer(o.GetType());
			using (MemoryStream xmlStream = new MemoryStream())
			{
				S.Serialize(xmlStream, o);
				xmlStream.Position = 0;
				Doc.Load(xmlStream);
				foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
				{
					XmlNode EK = N.SelectSingleNode("EntityKey");
					if (EK != null) N.RemoveChild(EK);
				}
				string ls_XML = Doc.InnerXml;
                ls_XML = ls_XML.Replace(@"<?xml version=", "");
                ls_XML = ls_XML.Replace("\"1.0\"", "");
                return ls_XML.Replace(@"?>", "");
			}
		}

	}
}