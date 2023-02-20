using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CTBC.WinExe.GetRate
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        static void Main()
        {
           ServiceBase[] ServicesToRun;
           ServicesToRun = new ServiceBase[] 
            { 
                new Service1() 
            };
           ServiceBase.Run(ServicesToRun);

           //Service1 s = new Service1();
           //s.getRate();
        }
    }
}
