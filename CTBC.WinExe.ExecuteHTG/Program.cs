using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcuteHTG
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new HtgBatch60491() 
            //};
            //ServiceBase.Run(ServicesToRun);

            //測試 console program
            if (string.IsNullOrEmpty(args[0]))
                return;
            HtgBatch60491 f = new HtgBatch60491();
            f.StartProgram(args[0]);

        }
    }
}
