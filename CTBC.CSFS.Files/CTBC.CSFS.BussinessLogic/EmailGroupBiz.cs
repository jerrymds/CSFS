using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;
using CTBC.CSFS.Models;

namespace CTBC.CSFS.BussinessLogic
{
    public class EmailGroupBiz : CommonBIZ
    {
        public EmailGroupBiz(AppController appController)
            : base(appController)
        {}

        public EmailGroupBiz()
        {}

        public IList<Email_Notice> GetQueryList()
        {
            try
            {
                string sql = @"select * from Email_Notice";
                base.Parameter.Clear();
                return base.SearchList<Email_Notice>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Create(Email_Notice model)
        {
            try
            {
                string sqlStr = @"insert into Email_Notice
                                        (
                                            Email,
                                            Unit,
                                            Employee
                                        ) 
                                        VALUES
                                        (
                                            @Email,
                                            @Unit,
                                            @Employee
                                        )";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Email", model.Email));
                base.Parameter.Add(new CommandParameter("@Unit", model.Unit));
                base.Parameter.Add(new CommandParameter("@Employee", model.Employee));
                return base.ExecuteNonQuery(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }        
        }

        public int ValiReEmail(Email_Notice model)
        {
            try
            {
                string sqlStr = @"select count(*) from Email_Notice where Email=@Email";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Email", model.Email));
                return (int)base.ExecuteScalar(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Email_Notice Select(string Email)
        {
            try
            {
                string sqlStr = @"select * from Email_Notice where Email=@Email";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Email", Email));

                IList<Email_Notice> list = base.SearchList<Email_Notice>(sqlStr);
                if(list !=null){
                    if(list.Count > 0)
                    {
                        return list[0];
                    }
                    else {
                        return new Email_Notice();
                    }    
                }else{
                    return new Email_Notice();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }                
        }

        public bool Edit(Email_Notice model, string EmailEdit)
        {
            string sqlStr = @"update Email_Notice
                                    set
                                        Email=@Email,
                                        Unit=@Unit,
                                        Employee=@Employee
                                    where Email = @EmailEdit";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@Email", model.Email));
            base.Parameter.Add(new CommandParameter("@Unit", model.Unit));
            base.Parameter.Add(new CommandParameter("@Employee", model.Employee));
            base.Parameter.Add(new CommandParameter("@EmailEdit", EmailEdit));
            try
            {
                return base.ExecuteNonQuery(sqlStr) > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }         
        }

        public int Delete(string Email) {
            try
            {
                string sqlStr = @"delete Email_Notice where Email = @Email";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Email", Email));

                return base.ExecuteNonQuery(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }                 
        }
    }
}
