using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.SendGSSDoc
{

    public class SendDocBase
    {
        public string CompanyId { get; set; }
        public string TransferType { get; set; }
        public string BatchNo { get; set; }
        public string BatchDate { get; set; }
        public string TotalNumber { get; set; }
        public Docinfo[] DocInfo { get; set; }
    }

    public class Docinfo
    {
        public string CnoNo { get; set; }
        public string DeptId { get; set; }
        public string DeptName { get; set; }
        public string HdlUser { get; set; }
        public string HdlUserName { get; set; }
        public string DeciDeptId { get; set; }
        public string DeciDeptName { get; set; }
        public string DeciUser { get; set; }
        public string DeciUserName { get; set; }
        public string DeciDate { get; set; }
        public string EndDeptId { get; set; }
        public string EndDeptName { get; set; }
        public string EndUser { get; set; }
        public string EndUserName { get; set; }
        public string EndDate { get; set; }
    }


    //參數	型態	長度上限	參數值說明
    //CompanyId	nvarchar	20	公司代碼，例銀行U00021931、金控U00021800
    //TransferType	nvarchar	2	轉換類型，1為「轉新外來文」與2為「轉線上投單」
    //BatchNo	nvarchar	20	批次批號，外來文系統產生
    //格式：yyyymmddhhmmss
    //例20190628133045
    //BatchDate	datatime		批次執行日期時間，例2019-05-14 17:00:00.000
    //TotalNumber	int		同步文號總數，例5表示此次同步有5筆

    //DocInfo	物件陣列	每筆公文的資料定義如下	公文結案資訊，多筆資料
    //CnoNo	nvarchar	20	公文文號，例1081000368
    //DeptId	nvarchar	50	公文主辦單位代碼，請回傳部
    //DeptName	nvarchar	50	公文主辦單位名稱
    //HdlUser	nvarchar	20	主辦人員代碼
    //HdlUserName	nvarchar	50	主辦人員名稱
    //DeciDeptId	nvarchar	50	決行單位代碼
    //DeciDeptName	nvarchar	50	決行單位名稱
    //DeciUser	nvarchar	20	決行人員代碼
    //DeciUserName	nvarchar	50	決行人員名稱
    //DeciDate	datatime		決行日期
    //EndDeptId	nvarchar	50	結案單位代碼
    //EndDeptName	nvarchar	50	結案單位名稱
    //EndUser	nvarchar	20	結案人員代碼
    //EndUserName	nvarchar	50	結案人員名稱
    //EndDate	datatime		結案日期





}
