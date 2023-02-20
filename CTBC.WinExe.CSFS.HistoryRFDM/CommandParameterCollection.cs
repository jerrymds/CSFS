using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.CSFS.HistoryRFDM
{


    public class CommandParameterCollection : IEnumerable
    {
        /// <summary>
        /// 容器，盛放參數之用
        /// </summary>
        private ArrayList AContainer = new ArrayList();

        /// <summary>
        /// 取得容器中參數的數量
        /// </summary>
        public int Count
        {
            get
            {
                return AContainer.Count;
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="mCommandParamenter">參數實體</param>
        /// <returns></returns>
        public CommandParameterCollection Add(CommandParameter mCommandParamenter)
        {
            this.AContainer.Add(mCommandParamenter);
            return this;
        }

        /// <summary>
        /// 清空容器
        /// </summary>
        /// <returns></returns>
        public CommandParameterCollection Clear()
        {
            this.AContainer.Clear();
            return this;
        }

        /// <summary>
        /// 取得參數實體
        /// </summary>
        /// <param name="iIndex"></param>
        /// <returns></returns>
        public CommandParameter this[int iIndex]
        {
            get
            {
                return (CommandParameter)this.AContainer[iIndex];
            }
        }

        #region 實現IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class MyEnumerator : IEnumerator
    {
        private int index;
        private CommandParameterCollection collection;

        /// <summary>
        /// 預設建構子
        /// </summary>
        /// <param name="coll"></param>
        public MyEnumerator(CommandParameterCollection coll)
        {
            collection = coll;
            index = -1;
        }

        /// <summary>
        /// 重設索引
        /// </summary>
        public void Reset()
        {
            index = -1;
        }

        /// <summary>
        /// 移向下一個元素
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            index++;

            return (index < collection.Count);
        }

        /// <summary>
        /// 取得當前元素
        /// </summary>
        public CommandParameter Current
        {
            get { return (CommandParameter)collection[index]; }
        }

        /// <summary>
        /// 當前元素屬性
        /// </summary>
        object IEnumerator.Current
        {
            get { return (Current); }
        }
    }


    public class CommandParameter
    {
        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string ColumnName { set; get; }

        /// <summary>
        /// 欄位類型
        /// </summary>
        public FieldType DBType { set; get; }

        /// <summary>
        /// 欄位值
        /// </summary>
        public object Value { set; get; }

        public SqlDbType Type { get; set; }

        public int Length { get; set; }

        /// <summary>
        /// 初始化參數
        /// </summary>
        /// <param name="sColumnName">欄位名稱</param>
        /// <param name="value">欄位值</param>
        public CommandParameter(string sColumnName, object value)
        {
            this.ColumnName = sColumnName;
            this.Value = value;
            this.DBType = FieldType.Object;
        }

        /// <summary>
        /// 初始化參數
        /// </summary>
        /// <param name="sColumnName">欄位名稱</param>
        /// <param name="value">欄位值</param>
        /// <param name="fFieldType">欄位數據類型</param>
        public CommandParameter(string sColumnName, object value, FieldType fFieldType)
        {
            this.ColumnName = sColumnName;
            this.Value = value;
            this.DBType = fFieldType;
        }

        /// <summary>
        /// 初始化參數
        /// </summary>
        /// <param name="sColumnName">欄位名稱</param>
        /// <param name="value">欄位值</param>
        /// <param name="fFieldType">欄位數據類型</param>
        public CommandParameter(string sColumnName, object value, SqlDbType dbType, int length)
        {
            this.ColumnName = sColumnName;
            this.Value = value;
            this.Type = dbType;
            this.Length = length;
        }
    }

    /// <summary>
    /// 欄位類型
    /// </summary>
    public enum FieldType
    {
        DateTime,
        Decimal,
        Double,
        Int,
        Object,
        StringArray,
        String,
        NVarchar
    }
}
