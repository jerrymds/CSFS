using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace CTBC.FrameWork.Util
{
    public class JsonHelper
    {
        /// <summary>
        /// 將對象轉換為Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 將Json字符串轉換為對象，如果字符串為{...}則轉換為單個對象，如果字符串為[{...},{...}]則轉換為IList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string json)
        {
           return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
