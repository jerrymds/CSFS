namespace CTBC.CSFS.ViewModels
{
    public class JsonReturn
    {
        /// <summary>
        /// 結果0-失敗,1-成功,其他的根據具體邏輯定
        /// </summary>
        public string ReturnCode { get; set; }
        public string ReturnMsg { get; set; }
    }
}