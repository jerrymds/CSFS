using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.CSFS.RPALDAPDataImport
{
   public class LDAPManager
   {
      /// <summary>
      /// 員編
      /// </summary>
      public string EmpID { get; set; }

      /// <summary>
      /// 部門代號
      /// </summary>
      public string DepID { get; set; }

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
