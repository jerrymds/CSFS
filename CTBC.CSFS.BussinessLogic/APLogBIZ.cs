using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTBC.CSFS.Pattern;
using System.Data;

namespace CTBC.CSFS.BussinessLogic
{
    public class APLogBIZ : CommonBIZ
    {
        #region 構造函數

        public APLogBIZ()
            : base()
        {

        }

        public APLogBIZ(string connectstring)
            : base(connectstring)
        {

        }

        public APLogBIZ(AppController appController)
            : base(appController)
        {
            _applController = appController;
        }

        #endregion

        /// <summary>
        /// 查詢生成明細檔的資料
        /// </summary>
        /// <returns>返回查詢結果</returns>
        public DataTable GetAPLogRawData(string DateTimeString)
        {
            try
            {
                StringBuilder pSql = new StringBuilder(@"
                                                    SELECT 
                                                        CONVERT(varchar(24), DataTimestamp, 121) AS DataTimestamp
                                                        , Controller
                                                        , Action
                                                        , IP
                                                        , Parameters
                                                        , CUSID
                                                        , LogonUser AS Account
                                                    FROM APLogRawData 
                                                    WHERE DataTimeString = @DataTimeString and len(CUSID) > 0
                                                    ");
                base.Parameter.Clear();
                base.Parameter.Add(new CommandParameter("@DataTimeString", DateTimeString));

                return base.Search(pSql.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
