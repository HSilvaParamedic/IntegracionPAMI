using System;
using System.ServiceProcess;

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
        }
	}
}
