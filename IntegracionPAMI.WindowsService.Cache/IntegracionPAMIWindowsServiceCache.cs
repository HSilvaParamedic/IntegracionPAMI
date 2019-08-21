using System;
using System.Timers;
using System.Diagnostics;
using System.Configuration;
using System.ServiceProcess;
using IntegracionPAMI.Services;
using IntegracionPAMI.WindowsService.Cache.Services;
using NLog;

namespace IntegracionPAMI.WindowsService.Cache
{
	public partial class IntegracionPAMIWindowsServiceCache : ServiceBase
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		private int eventId = 1;
		private Timer timer;
		private readonly IntegracionPAMIManager _integracionPAMIManager;

		public IntegracionPAMIWindowsServiceCache()
		{
			InitializeComponent();

			try
			{
				_integracionPAMIManager = new IntegracionPAMIManager(new IntegracionService());
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				throw ex;
			}
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				timer = new Timer();
				timer.Interval = int.Parse(ConfigurationManager.AppSettings.Get("IntervaloDeEjecucion_Mins")) * 10000;
				timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
				timer.Start();

				_logger.Info("Se inició el servicio");
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				throw ex;
			}
		}

		protected override void OnStop()
		{
			_logger.Info("Se detuvo el servicio");
		}

		private void OnTimer(object sender, ElapsedEventArgs args)
		{
			try
			{
				_integracionPAMIManager.GuardarNuevosServicios();
				_integracionPAMIManager.EnviarEstadosAsignacion();
                /// Envío de estados de Servicio
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}
		}
	}
}
