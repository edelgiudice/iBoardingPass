using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace AggiornaStatoVoli
{
    static class Program
    {
        private static ManualResetEvent m_daemonUp = new ManualResetEvent(false);
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool isConsole = false;
            System.Diagnostics.EventLog eventLogAggVoli=new System.Diagnostics.EventLog();

            if (!System.Diagnostics.EventLog.SourceExists("EasyGate_AggVoli"))
            {
                System.Diagnostics.EventLog.CreateEventSource("EasyGate_AggVoli", "AggVoliLog");
            }
            eventLogAggVoli.Source = "EasyGate_AggVoli";
            eventLogAggVoli.Log = "AggVoliLog";

            if (args != null && args.Length == 1 && args[0].StartsWith("-c"))
            {
                isConsole = true;
                Console.WriteLine("Daemon starting");

                AggVoliMng daemon = new AggVoliMng();

                Thread daemonThread = new Thread(new ParameterizedThreadStart(AggVoliMng.Start));
                daemonThread.Start(eventLogAggVoli);
                m_daemonUp.WaitOne();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                    { 
                        new ServiceAggStatoVoli(ref eventLogAggVoli) 
                    };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
