/// <summary>
/// 程式說明:驗證適配器與自定義驗證
/// </summary>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.WebPages;//20140619 horace,new include for MVC5
using CTBC.CSFS.Resource;


/// <summery> 後臺欄位驗證Function說明 </summery>
/// 使用方式： [Required]
///		       [Numericletter()]
///		       [Display(Name = "parm_lifeexpcode")]
///		       public string LifeExpCode { get; set; }
///		       
/// 注解：[Required] 必輸
///       [Numericletter()]  英文數字驗證，為本類中定義的驗證規則
///       [Display(Name = "parm_lifeexpcode")]   驗證欄位名稱，多語系中的key值
///       public string LifeExpCode { get; set; }    須驗證的欄位
///       
/// 該Class中定義的驗證規則說明：
///      DateExpression: 日期驗證
///      Pureinteger: 正整數驗證
///      Numeric: 整數驗證
///      Numericletter: 英文數字
///      SalerEmpNo: SalerEmpNo推廣員編特殊驗證首字母大寫共輸入10碼
///      Email:Email驗證
///      Letter：英文字符驗證(可是輸入空格)
///      Decimal3_0:驗證Decima(3,0)---正負
///      Decima3_1:驗證Decima(3,1)---正負
///      PlusDecima3_1:驗證Decima(3,1)--正
///      Decimal4_0:驗證Decima(4,0)---正負
///      Decima4_1:驗證Decima(4,1)---正負
///      PlusDecima4_1:驗證Decima(4,1)--正
///      Decimal6_0:驗證Decima(6,0)---正負
///      PlusDecima7_1:驗證Decima(7,1)--正
///      Decima8_1:驗證Decima(8,1)---正負
///      PlusDecima8_1:驗證Decima(8,1)--正
///      Decima8_2:驗證Decima(8,2)---正負
///      PlusDecima8_2:驗證Decima(8,2)--正
///      Decima8_4:驗證Decima(8,4)---正負
///      PulsDecima8_4:驗證Decima(8,4)--正
///      PlusDecima9_2:驗證Decima(9,2)--正
///      Decimal1_1:驗證Decima(11,1)---正負
///      Decimal2_2:驗證Decima(12,1)---正負
///      Decimal12_4:驗證Decima(12,4)---正負
///      Num1_100:驗證1-100之間的數
///      
/// <summery> End </summery>   

namespace CTBC.FrameWork.Util
{
	#region  適配器

	/// <summary>
	/// 必輸驗證適配器
	/// </summary>
	public class CTCBRequiredAttributeAdapter : System.Web.Mvc.RequiredAttributeAdapter
	{

		/// <summary>
		/// 構造函數
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="context"></param>
		/// <param name="attribute"></param>
		/// <param name="type"></param>
		public CTCBRequiredAttributeAdapter(ModelMetadata metadata, ControllerContext context, RequiredAttribute attribute)
			: base(metadata, context, attribute)
		{
		}

		public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
		{
			string errorMessage = string.Format(Lang.csfs_enter_parm0, string.IsNullOrEmpty(Metadata.DisplayName) ? Metadata.PropertyName : Lang.ResourceManager.GetString(Metadata.DisplayName));
			return new[] { new ModelClientValidationRequiredRule(errorMessage) };
		}
	}

	/// <summary>
	/// 範圍驗證適配器
	/// </summary>
	public class CTCBRangeAttributeAdapter : System.Web.Mvc.RangeAttributeAdapter
	{
		public CTCBRangeAttributeAdapter(ModelMetadata metadata, ControllerContext context, RangeAttribute attribute)
			: base(metadata, context, attribute)
		{
		}

		public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
		{
			string errorMessage = string.Format(Lang.csfs_enter_parm0, Metadata.DisplayName);
			return new[] { new ModelClientValidationRequiredRule(errorMessage) };
		}
	}

	/// <summary>
	/// 正則格式驗證適配器
	/// </summary>
	public class CTCBRegularAttributeAdapter : System.Web.Mvc.RegularExpressionAttributeAdapter
	{
		public CTCBRegularAttributeAdapter(ModelMetadata metadata, ControllerContext context, RegularExpressionAttribute attribute)
			: base(metadata, context, attribute)
		{
		}

		public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
		{
            string errorMessage = string.Format(Lang.csfs_enter_correct_format, Metadata.DisplayName);
			return new[] { new ModelClientValidationRequiredRule(errorMessage) };
		}
	}

	/// <summary>
	/// 範圍驗證適配器
	/// </summary>
	public class CTCBStringLengthAttributeAdapter : System.Web.Mvc.StringLengthAttributeAdapter
	{
		public CTCBStringLengthAttributeAdapter(ModelMetadata metadata, ControllerContext context, StringLengthAttribute attribute)
			: base(metadata, context, attribute)
		{
		}

		public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
		{
			string errorMessage = string.Format(Lang.csfs_enter_parm0, Metadata.DisplayName);
			return new[] { new ModelClientValidationRequiredRule(errorMessage) };
		}
	}

	#endregion

	#region 自定義驗證

	/// <summary>
	/// 日期驗證
	/// </summary>
	public class DateExpressionAttribute : RegularExpressionAttribute, IClientValidatable
	{
		public DateExpressionAttribute()
			: base(@"((^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(10|12|0?[13578])([-\/\._])(3[01]|[12][0-9]|0?[1-9])$)|(^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(11|0?[469])([-\/\._])(30|[12][0-9]|0?[1-9])$)|(^((1[8-9]\d{2})|([2-9]\d{3}))([-\/\._])(0?2)([-\/\._])(2[0-8]|1[0-9]|0?[1-9])$)|(^([2468][048]00)([-\/\._])(0?2)([-\/\._])(29)$)|(^([3579][26]00)([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][0][48])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][0][48])([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][2468][048])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][2468][048])([-\/\._])(0?2)([-\/\._])(29)$)|(^([1][89][13579][26])([-\/\._])(0?2)([-\/\._])(29)$)|(^([2-9][0-9][13579][26])([-\/\._])(0?2)([-\/\._])(29)$))")
		{
		}

		public override bool IsValid(object value)
		{
			return true;
		}

		public override string FormatErrorMessage(string name)
		{
			return String.Format(CultureInfo.CurrentCulture,
			 "{0} " + Lang.csfs_enter_correctdate, Lang.ResourceManager.GetString(name));
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
			var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
			yield return rule;
		}
	}

	/// <summary>
	/// 正整數
	/// </summary>
	public class PureintegerAttribute : RegularExpressionAttribute, IClientValidatable
	{

		public PureintegerAttribute()
			: base(@"^[0-9][0-9]+$|^[+_\-]?[0-9]$")
		{
		}

		public override bool IsValid(object value)
		{
			return true;
		}

		public override string FormatErrorMessage(string name)
		{
			return String.Format(CultureInfo.CurrentCulture,
			 "{0} " + Lang.csfs_enter_positiveint, Lang.ResourceManager.GetString(name));
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
			var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
			yield return rule;
		}
	}

	/// <summary>
	/// 整數
	/// </summary>
	public class NumericAttribute : RegularExpressionAttribute, IClientValidatable
	{

		public NumericAttribute()
            : base(@"^[+_\-]?[0-9]*(\.\d*)?$|^-?d^(\.\d*)?")
		{
		}

		public override bool IsValid(object value)
		{
			return true;
		}

		public override string FormatErrorMessage(string name)
		{
			return String.Format(CultureInfo.CurrentCulture,
			 "{0} " + Lang.csfs_enter_number, Lang.ResourceManager.GetString(name));
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
			var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
			yield return rule;
		}
	}

    /// <summary>
    /// 請輸入大於0的整數 
    /// </summary>
    /// 20130227 horace
    public class IntegerGreaterThanZeroAttribute : RegularExpressionAttribute, IClientValidatable
    {
        public IntegerGreaterThanZeroAttribute()
            : base(@"^[1-9][0-9]*$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_enter_greaterthanzero, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

	/// <summary>
	/// 英文數字
	/// </summary>
	public class NumericletterAttribute : RegularExpressionAttribute, IClientValidatable
	{
		public NumericletterAttribute()
			: base(@"^[A-Za-z0-9]+$")
		{
		}

		public override bool IsValid(object value)
		{
			return true;
		}

		public override string FormatErrorMessage(string name)
		{
			return String.Format(CultureInfo.CurrentCulture,
			 "{0} " + Lang.csfs_enter_letterornum, Lang.ResourceManager.GetString(name));
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
			var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
			yield return rule;
		}
	}

    /// <summary>
    /// 英文數字-特殊1-給CodeType,可輸入底線與減號符號
    /// </summary>
    public class NumericletterCodeTypeAttribute : RegularExpressionAttribute, IClientValidatable
    {
        public NumericletterCodeTypeAttribute()
            : base(@"^[A-Za-z0-9_]+$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0}" + Lang.csfs_enter_letterornum, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name) + Lang.csfs_codetype_underline, Pattern);
            yield return rule;
        }
    }


	/// <summary>
	/// SalerEmpNo推廣員編特殊驗證首字母大寫共輸入10碼
	/// </summary>
	public class SalerEmpNoAttribute : RegularExpressionAttribute, IClientValidatable
	{
		public SalerEmpNoAttribute()
			: base(@"^([A-Z]{1})([A-Za-z0-9]){9}$")
		{
		}

		public override bool IsValid(object value)
		{
			return true;
		}

		public override string FormatErrorMessage(string name)
		{
			return String.Format(CultureInfo.CurrentCulture,
			 "{0} " + Lang.csfs_enter_salerempno, Lang.ResourceManager.GetString(name));
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
			var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
			yield return rule;
		}
	}

    /// <summary>
    /// Email驗證
    /// </summary>
    public class EmailAttribute : RegularExpressionAttribute, IClientValidatable
    {
        //20131209 修正，原格式: @"^([a-zA-Z0-9._-])+@([a-zA-Z0-9_-])+((\.[a-zA-Z0-9_-]{2,3}){1,2})$"
        public EmailAttribute()
            : base(@"^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z]+$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_enter_email, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// 英文字符
    /// </summary>
    public class LetterAttribute : RegularExpressionAttribute, IClientValidatable
    {
        public LetterAttribute()
            : base(@"^[A-Za-z\s]+$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_enter_letter, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(3,0)
    /// </summary>
    public class Decimal3_0Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decimal3_0Attribute()
            : base(@"^[+_\-]?[0-9]{1,3}?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal3_0, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }
     
    /// <summary>
    /// Decimal(3,1)
    /// </summary>
    public class Decima3_1Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decima3_1Attribute()
            : base(@"^[+_\-]?\d{1,2}([.]\d{1})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal3_1, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(3,1)--正數
    /// </summary>
    public class PlusDecima3_1Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PlusDecima3_1Attribute()
            : base(@"^\d{1,2}([.]\d{1})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal3_1, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(4,0)
    /// </summary>
    public class Decimal4_0Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decimal4_0Attribute()
            : base(@"^[+_\-]?[0-9]{1,4}?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal4_0, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(4,1)
    /// </summary>
    public class Decima4_1Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decima4_1Attribute()
            : base(@"^[+_\-]?\d{1,3}([.]\d{1})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal4_1, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(4,1)--正數
    /// </summary>
    public class PlusDecima4_1Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PlusDecima4_1Attribute()
            : base(@"^\d{1,3}([.]\d{1})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal4_1, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(4,2)--正數
    /// </summary>
    public class PlusDecima4_2Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PlusDecima4_2Attribute()
            : base(@"^\d{1,3}([.]\d{0,2})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal4_2, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(6,0)
    /// </summary>
    public class Decimal6_0Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decimal6_0Attribute()
            : base(@"^[+_\-]?[0-9]{1,6}?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal6_0, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(7,1)--正
    /// </summary>
    public class PlusDecima7_1Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PlusDecima7_1Attribute()
            : base(@"^\d{1,6}([.]\d{1})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_plusdecima7_1, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(7,4)
    /// </summary>
    //20131203 Tom新增
    public class PulsDecima7_4Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PulsDecima7_4Attribute()
            : base(@"^\d{1,3}([.]\d{1,4})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_plusdecima7_4, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(8,0)--正負
    /// </summary>
    public class Decima8_0Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decima8_0Attribute()
            : base(@"^[+_\-]?[0-9]{1,8}?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal8_0, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(8,1)--正負
    /// </summary>
    public class Decima8_1Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decima8_1Attribute()
            : base(@"^[+_\-]?\d{1,7}([.]\d{1})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal8_1, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(8,1)--正
    /// </summary>
    public class PlusDecima8_1Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PlusDecima8_1Attribute()
            : base(@"^\d{1,7}([.]\d{1})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_plusdecima8_1, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(8,2)
    /// </summary>
    public class Decima8_2Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decima8_2Attribute()
            : base(@"^[+_\-]?\d{1,6}([.]\d{1,2})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal8_2, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(8,2)
    /// </summary>
    public class PlusDecima8_2Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PlusDecima8_2Attribute()
            : base(@"^\d{1,6}([.]\d{1,2})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal8_2, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(8,4)
    /// </summary>
    public class Decima8_4Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decima8_4Attribute()
            : base(@"^[+_\-]?\d{1,4}([.]\d{1,4})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal8_4, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(8,4)
    /// </summary>
    public class PulsDecima8_4Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PulsDecima8_4Attribute()
            : base(@"^\d{1,4}([.]\d{1,4})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_plusdecima8_4, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    
    /// <summary>
    /// Decimal(9,2)--正
    /// </summary>
    public class PlusDecima9_2Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PlusDecima9_2Attribute()
            : base(@"^\d{1,7}([.]\d{1,2})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_plusdecima9_2, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }


	/// <summary>
	/// Decimal(5,2)--正
	/// </summary>
	public class PlusDecima5_2Attribute : RegularExpressionAttribute, IClientValidatable
	{
		public PlusDecima5_2Attribute()
			: base(@"^\d{1,3}([.]\d{1,2})?$")
		{
		}

		public override bool IsValid(object value)
		{
			return true;
		}

		public override string FormatErrorMessage(string name)
		{
			return String.Format(CultureInfo.CurrentCulture,
			 "{0} " + Lang.csfs_plusdecima5_2, Lang.ResourceManager.GetString(name));
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
			var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
			yield return rule;
		}
	}

    /// <summary>
    /// Decimal(11,1)
    /// </summary>
    public class Decimal1_1Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decimal1_1Attribute()
            : base(@"^[+_\-]?\d{1,10}([.]\d{1})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal11_1, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(12,2)
    /// </summary>
    public class Decimal2_2Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decimal2_2Attribute()
            : base(@"^[+_\-]?\d{1,10}([.]\d{1,2})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal12_2, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(12,4)
    /// </summary>
    public class Decimal12_4Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Decimal12_4Attribute()
            : base(@"^[+_\-]?\d{1,8}([.]\d{1,4})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_decimal12_4, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(11,4)
    /// </summary>
    //20131203 Tom新增
    public class PulsDecima11_4Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PulsDecima11_4Attribute()
            : base(@"^\d{1,7}([.]\d{1,4})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_plusdecima11_4, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// Decimal(12,4)
    /// </summary>
    //20131203 Tom新增
    public class PulsDecima12_4Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public PulsDecima12_4Attribute()
            : base(@"^\d{1,8}([.]\d{1,4})?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_plusdecima12_4, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

    /// <summary>
    /// 1-100之間的數
    /// </summary>
    public class Num1_100Attribute : RegularExpressionAttribute, IClientValidatable
    {
        public Num1_100Attribute()
            : base(@"^([1-9]|[1-9][0-9]|100)?$")
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
             "{0} " + Lang.csfs_num1_100, Lang.ResourceManager.GetString(name));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var name = string.IsNullOrEmpty(metadata.GetDisplayName()) ? metadata.PropertyName : metadata.GetDisplayName();
            var rule = new ModelClientValidationRegexRule(FormatErrorMessage(name), Pattern);
            yield return rule;
        }
    }

	#endregion
}
