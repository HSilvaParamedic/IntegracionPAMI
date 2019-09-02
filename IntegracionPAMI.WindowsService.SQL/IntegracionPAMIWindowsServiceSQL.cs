using System;
using System.Timers;
using System.Diagnostics;
using System.Configuration;
using System.ServiceProcess;
using IntegracionPAMI.Services;
using IntegracionPAMI.WindowsService.SQL.Services;
using NLog;

namespace IntegracionPAMI.WindowsService.SQL
{
    public partial class IntegracionPAMIWindowsServiceSQL : ServiceBase
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private Timer timer;
        private readonly IntegracionPAMIManager _integracionPAMIManager;

        public IntegracionPAMIWindowsServiceSQL()
        {
            InitializeComponent();

            try
            {
                _integracionPAMIManager = new IntegracionPAMIManager(new IntegracionService());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
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
                throw;
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
				_logger.Info("Ejecutando guardado de nuevos servicios...");
				_integracionPAMIManager.GuardarNuevosServicios();
				_logger.Info("Finalización de guardado de nuevos servicios...");

				///_integracionPAMIManager.EnviarEstadosAsignacion();
			}
			catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
        }
    }
}
