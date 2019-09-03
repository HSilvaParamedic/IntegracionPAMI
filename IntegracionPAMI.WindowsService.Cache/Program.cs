using System;
using System.ServiceProcess;

namespace IntegracionPAMI.WindowsService.Cache
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{

			IntegracionPAMIWindowsServiceCache service = new IntegracionPAMIWindowsServiceCache();
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