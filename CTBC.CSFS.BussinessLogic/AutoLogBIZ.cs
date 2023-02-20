using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using CTBC.CSFS.Pattern;
using System.Security.Cryptography;
using System.IO;

namespace CTBC.CSFS.BussinessLogic
{
    public class AutoLogBIZ : CommonBIZ
    {
        public int addAutoLog(Guid CaseID, string SendUser, string DocNo, string ObligorNo, string SeviceNo, int Status, string ErrorMessage)
        {
            IDbTransaction trans = null;
            string sql = @" insert into [dbo].[AutoLog]  ([CaseId], [SendUser], [DocNo], [ObligorNo], [ServiceName], [SendDate], [Status], [ErrorMsg], [CreateDatetime]) 
                                        values ( @CaseId, @SendUser, @DocNo, @ObligorNo, @ServiceName, GETDATE(), @Status, @ErrorMessage , GETDATE());";

            Parameter.Clear();
            //MyString.Substring(MyString.Length-6)
            // 添加參數
            Parameter.Add(new CommandParameter("@CaseId", CaseID));
            Parameter.Add(new CommandParameter("@SendUser", SendUser));
            Parameter.Add(new CommandParameter("@DocNo", DocNo.Substring(DocNo.Length - 12)));
            Parameter.Add(new CommandParameter("@ObligorNo", ObligorNo));
            Parameter.Add(new CommandParameter("@Status", Status));
            Parameter.Add(new CommandParameter("@ErrorMessage", ErrorMessage));
            Parameter.Add(new CommandParameter("@ServiceName", SeviceNo));

            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }

        public  long addAutoLog(Guid CaseID, string SendUser, string DocNo, string ObligorNo, string SeviceNo)
        {
            IDbTransaction trans = null;
            string sql = @" insert into [dbo].[AutoLog]  ([CaseId], [SendUser], [DocNo], [ObligorNo], [ServiceName], [SendDate], [Status], [ErrorMsg], [CreateDatetime]) 
                                        values ( @CaseId, @SendUser, @DocNo, @ObligorNo, @ServiceName, GETDATE(), 0, NULL , GETDATE());";

            Parameter.Clear();
            //MyString.Substring(MyString.Length-6)
            // 添加參數
            Parameter.Add(new CommandParameter("@CaseId", CaseID));
            Parameter.Add(new CommandParameter("@SendUser", SendUser));
            Parameter.Add(new CommandParameter("@DocNo", DocNo.Substring(DocNo.Length-12)));
            Parameter.Add(new CommandParameter("@ObligorNo", ObligorNo));
            Parameter.Add(new CommandParameter("@ServiceName", SeviceNo));           
            
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }

        public long updateAutoLog(long id, int Status, string ErrorMessage)
        {
            IDbTransaction trans = null;
            string sql = @" update [dbo].[AutoLog]  set [Status] = @Status, [ErrorMsg] = @ErrorMessage where id=@id";
                                        
             
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@Status", Status));
            Parameter.Add(new CommandParameter("@ErrorMessage", ErrorMessage));
            Parameter.Add(new CommandParameter("@id", id));
            return trans == null ? ExecuteNonQuery(sql) : ExecuteNonQuery(sql, trans);
        }
    }
}
