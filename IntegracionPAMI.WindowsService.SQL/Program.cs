using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace IntegracionPAMI.WindowsService.SQL
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
        {

            IntegracionPAMIWindowsServiceSQL service = new IntegracionPAMIWindowsServiceSQL();
            if (Environment.UserInteractive)
            {
                service.RunAsConsole(args);
            }
            else
            {
                service.SetFullControlPermissionsToEveryone(AppDomain.CurrentDomain.BaseDirectory);
                ServiceBase.Run(service);
            }

            //static void Main()
            //{
            //    ServiceBase[] ServicesToRun;
            //    ServicesToRun = new ServiceBase[]
            //    {
            //        new IntegracionPAMIWindowsServiceSQL()
            //    };
            //    ServiceBase.Run(ServicesToRun);
            //}
        }
	}
}
