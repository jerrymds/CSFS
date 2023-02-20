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
    public class ApprMsgKeyBiz : CommonBIZ
    {
        public string[] getLdapInfo(Guid caseid)
        {
            string strsql = @"SELECT TOP 1 * FROM [dbo].[ApprMsgKey] where versionnewid=@caseid ";
            Parameter.Clear();
            Parameter.Add(new CommandParameter("@caseid", caseid));
            DataTable Dt = Search(strsql);
            if (Dt.Rows.Count > 0)
            {
                var _ldapuid = Decode(Dt.Rows[0]["MsgKeyLU"].ToString());
                var _ldappwd = Decode(Dt.Rows[0]["MsgKeyLP"].ToString());
                var _racfuid = Decode(Dt.Rows[0]["MsgKeyRU"].ToString());
                var _racfowd = Decode(Dt.Rows[0]["MsgKeyRP"].ToString());
                var _racfbranch = Decode(Dt.Rows[0]["MsgKeyRB"].ToString());
                return new string[] { _ldapuid, _ldappwd, _racfuid, _racfowd, _racfbranch};
            }
            else
            {
                return new string[] { "", "", "", "", "" };
            }
        }

        public string Encode(string data)
        {
            string KEY_64 = "VavicApp";
            string IV_64 = "VavicApp";

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();

            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }


        public string Decode(string data)
        {
            string KEY_64 = "VavicApp";
            string IV_64 = "VavicApp";

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);
            byte[] byEnc;

            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);

            return sr.ReadToEnd();
        }
    }
}
