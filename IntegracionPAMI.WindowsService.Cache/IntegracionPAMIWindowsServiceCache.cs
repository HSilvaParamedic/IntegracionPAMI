using System;
using System.Timers;
using System.Diagnostics;
using System.Configuration;
using System.ServiceProcess;
using IntegracionPAMI.Services;

namespace IntegracionPAMI.WindowsService.Cache
{
	public partial class IntegracionPAMIWindowsServiceCache : ServiceBase
	{
		private int eventId = 1;
		Timer timer;

		public IntegracionPAMIWindowsServiceCache()
		{
			InitializeComponent();
			//System.Diagnostics.EventLog.DeleteEventSource(this.ServiceName);
			eventLog = new EventLog();
			if (!EventLog.SourceExists(this.ServiceName))
			{
				EventLog.CreateEventSource(this.ServiceName, "Appication");
			}
			eventLog.Source = this.ServiceName;
			eventLog.Log = "Application";
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				int intervaloDeEjecucion = 6;//int.Parse(ConfigurationManager.AppSettings.Get("IntervaloDeEjecucion_Mins"));

				timer = new Timer();
				timer.Interval = intervaloDeEjecucion * 10000;
				timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
				timer.Start();
			}
			catch (Exception ex)
			{
				eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
			}
		}

		protected override void OnStop()
		{
		}

		private void OnTimer(object sender, ElapsedEventArgs args)
		{
			try
			{
				IntegracionPAMIManager.GuardarNuevosServicios();
			}
			catch (Exception ex)
			{
				eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
			}
		}
	}
}
