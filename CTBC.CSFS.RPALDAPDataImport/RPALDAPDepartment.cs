using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.RPALDAPDataImport
{
   public class LDAPDepartment
   {
      /// <summary>
      /// 部門名稱
      /// </summary>
      public string DepName { get; set; }

      /// <summary>
      /// 部門代號
      /// </summary>
      public string DepID { get; set; }

      /// <summary>
      /// 部門DN
      /// </summary>
      public string DepDN { get; set; }

      /// <summary>
      /// 部門類別(1#管理單位/2#分行/3#區域中心/0#其他)
      /// </summary>
      public string BusinessCategory { get; set; }

      /// <summary>
      /// 部門層級名稱(組/科/部/處)
      /// </summary>
      public string DepTitle { get; set; }

      /// <summary>
      /// 部門層級(數字)
      /// </summary>
      public string CtcbHrisOrgLevel { get; set; }

      /// <summary>
      /// 分行代碼
      /// </summary>
      public string CtcbBankingID { get; set; }

      /// <summary>
      /// 建立者
      /// </summary>
      public string CreatedUser { get; set; }

      /// <summary>
      /// 建立日
      /// </summary>
      public DateTime CreatedDate { get; set; }

      /// <summary>
      /// 修改者
      /// </summary>
      public string ModifiedUser { get; set; }

      /// <summary>
      /// 修改日
      /// </summary>
      public DateTime ModifiedDate { get; set; }
   }
}
