/// <summary>
/// 程式說明：維護PARMScheduleSetting - 排程管理
/// 維護部門:資訊管理處
/// CSFS Version:v3.0
/// 中國信託銀行 版權所有  ©  All Rights Reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Pattern;

namespace CTBC.CSFS.Models
{
    public class PARMScheduleSettingBIZ : CommonBIZ
    {
        public PARMScheduleSettingBIZ(AppController appController)
            : base(appController)
        {}

        public PARMScheduleSettingBIZ()
        {}

        public IEnumerable<PARMScheduleSettingVO> GetScheduleList()
        {
            try
            {
                string sql = @"select * from PARMScheduleSetting";
                base.Parameter.Clear();
                return base.SearchList<PARMScheduleSettingVO>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<PARMScheduleSettingVO> GetQueryList(PARMScheduleSettingVO qryCsfsVO,  int pageIndex)
        {
            try
            {
                base.PageIndex = pageIndex;
                string sqlStr = "";
                string sqlStrWhere = "";
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@pageS", (base.PageSize * (base.PageIndex - 1)) + 1));
                base.Parameter.Add(new CommandParameter("@pageE", base.PageSize * base.PageIndex));

                if (!string.IsNullOrEmpty(qryCsfsVO.Name)) {
                    sqlStrWhere += @" and Name like @Name ";
                    base.Parameter.Add(new CommandParameter("@Name", "%" + qryCsfsVO.Name + "%"));            
                }

                if (!string.IsNullOrEmpty(qryCsfsVO.Path))
                {
                    sqlStrWhere += @" and Path like @Path ";
                    base.Parameter.Add(new CommandParameter("@Path", "%" + qryCsfsVO.Path + "%"));
                }

                sqlStr += @";with T1 
                            as
                            (
	                            select ID,Name,Path,Arguments,Enabled,Status,OneTime,RegularHour,RegularMinute from PARMScheduleSetting with(nolock)
	                            where 1=1 " + sqlStrWhere + @" 
                            ),T2 as
                            (
	                            select *, row_number() over (order by Name) RowNum
	                            from T1
                            ),T3 as 
                            (
                                select *,(select max(RowNum) from T2) maxnum from T2 
                                where rownum between @pageS and @pageE
                            )
                            select * from T3";
                
                IList<PARMScheduleSettingVO> _ilsit = base.SearchList<PARMScheduleSettingVO>(sqlStr);

                if(_ilsit.Count > 0){
                    base.DataRecords = _ilsit[0].maxnum;
                }else{
                    base.DataRecords = 0;
                    _ilsit = new List<PARMScheduleSettingVO>();
                }
                return _ilsit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Create(PARMScheduleSettingVO model)
        {
            try
            {
                string sqlStr = @"insert into PARMScheduleSetting
                                        (
                                            Name,
                                            Path,
                                            Arguments,
                                            Enabled,
                                            Status,
                                            OneTime,
                                            RegularHour,
                                            RegularMinute,
                                            CreatedUser,
                                            ModifiedUser
                                        ) 
                                        VALUES
                                        (
                                            @Name,
                                            @Path,
                                            @Arguments,
                                            @Enabled,
                                            @Status,
                                            @OneTime,
                                            @RegularHour,
                                            @RegularMinute,
                                            @CreatedUser,
                                            @ModifiedUser
                                        )";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@Name", model.Name));
                base.Parameter.Add(new CommandParameter("@Path", model.Path));
                base.Parameter.Add(new CommandParameter("@Arguments", model.Arguments));
                base.Parameter.Add(new CommandParameter("@Enabled", model.Enabled));
                base.Parameter.Add(new CommandParameter("@Status", model.Status));
                base.Parameter.Add(new CommandParameter("@OneTime", model.OneTime));
                base.Parameter.Add(new CommandParameter("@RegularHour", model.RegularHour));
                base.Parameter.Add(new CommandParameter("@RegularMinute", model.RegularMinute));
                base.Parameter.Add(new CommandParameter("@CreatedUser", Account));
                base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));

                return base.ExecuteNonQuery(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }        
        }

        public PARMScheduleSettingVO Select(Guid id)
        {
            try
            {
                string sqlStr = @"select * from PARMScheduleSetting where ID=@ID";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ID", id));

                IList<PARMScheduleSettingVO> list = base.SearchList<PARMScheduleSettingVO>(sqlStr);
                if(list !=null){
                    if(list.Count > 0)
                    {
                        return list[0];
                    }
                    else {
                        return new PARMScheduleSettingVO();
                    }    
                }else{                        
                    return new PARMScheduleSettingVO();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }                
        }

        public bool Edit(PARMScheduleSettingVO model)
        {
            string sqlStr = @"update PARMScheduleSetting
                                    set
                                        Name=@Name,
                                        Path=@Path,
                                        Arguments=@Arguments,
                                        Enabled=@Enabled,
                                        Status=@Status,
                                        OneTime=@OneTime,
                                        RegularHour=@RegularHour,
                                        RegularMinute=@RegularMinute,
                                        ModifiedUser=@ModifiedUser
                                    where ID = @ID";

            base.Parameter.Clear();
            base.Parameter.Add(new CommandParameter("@ID", model.ID));
            base.Parameter.Add(new CommandParameter("@Name", model.Name));
            base.Parameter.Add(new CommandParameter("@Path", model.Path));
            base.Parameter.Add(new CommandParameter("@Arguments", model.Arguments));
            base.Parameter.Add(new CommandParameter("@Enabled", model.Enabled));
            base.Parameter.Add(new CommandParameter("@Status", model.Status));
            base.Parameter.Add(new CommandParameter("@OneTime", model.OneTime));
            base.Parameter.Add(new CommandParameter("@RegularHour", model.RegularHour));
            base.Parameter.Add(new CommandParameter("@RegularMinute", model.RegularMinute));
            base.Parameter.Add(new CommandParameter("@ModifiedUser", Account));
            try
            {
                return base.ExecuteNonQuery(sqlStr) > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }         
        }

        public int Delete(Guid id) {
            try
            {
                string sqlStr = @"delete PARMScheduleSetting where ID = @ID";

                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@ID", id));

                return base.ExecuteNonQuery(sqlStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }                 
        }
    }
}
