using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using System.Xml;


namespace CTBC.FrameWork.Platform
{
	public class AppValidator
	{
		// properties
		public string TypeName = "";
		public AttributeValidatorFactory fAttributeValidator;
		public ConfigurationValidatorFactory fConfigurationValidator;
		//public static Type fValidator = ValidationFactory;
		public string RuleSetsName = "";

		//---------------------------------------
		//			Public Method
		//---------------------------------------
		
		// constructor
		public AppValidator()
		{
		}

		// object validate
		public ValidationResults Validate(object o, ValidationResults results)
		{
			return null;
		}

		// XML validate
		public ValidationResults Validate(XmlDocument xmlDoc, string xPath, Type T, ValidationResults results)
		{
			return null;
		}
		
	}
}
