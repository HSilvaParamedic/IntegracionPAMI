using System;
using System.IO;
using System.Timers;
using System.Configuration;
using System.ServiceProcess;
using System.Security.Principal;
using System.Security.AccessControl;
using IntegracionPAMI.Services;
using IntegracionPAMI.WindowsService.SQL.Services;
using NLog;
using System.Text;

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

				_logger.Info("Se inició el servicio");
				_logger.Info(string.Format("API_Username: {0}", ConfigurationManager.AppSettings.Get("API_Username")));

				ElapsedHandler();

				timer = new Timer();
                timer.Interval = int.Parse(ConfigurationManager.AppSettings.Get("IntervaloDeEjecucion_Segs")) * 1000;
                timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
				timer.Start();

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
				ElapsedHandler();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}
		}

		private void ElapsedHandler()
		{

			if (this.timer != null) { this.timer.Enabled = false; }

			try
			{

				// Informe de Eventos
				if (int.Parse(ConfigurationManager.AppSettings.Get("ServicioMap_infSucesos")) == 1)
                {
                    _logger.Info("Enviando estados de asignación");
                    _integracionPAMIManager.EnviarEstadosAsignacion();

                }

                // Nuevos Servicios
                _logger.Info("Ejecutando guardado de nuevos servicios...");
                _integracionPAMIManager.GuardarNuevosServicios("Nuevo");

				// Reiteración
				_logger.Info("Ejecutando reiteraciones de servicios...");
				_integracionPAMIManager.GuardarNuevosServicios("Reiteración");

				// Reclamo
				_logger.Info("Ejecutando reclamos de servicios...");
				_integracionPAMIManager.GuardarNuevosServicios("Reclamo");

				// Anulados
				_logger.Info("Ejecutando anulaciones de servicios...");
				_integracionPAMIManager.GuardarNuevosServicios("Anulación");

			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}

			if (this.timer != null) { this.timer.Enabled = true; }

		}

		#region Public Methods

		/// <summary>
		/// Para ejecutarlo por consola, para testear.
		/// </summary>
		/// <param name="args"></param>
		public void RunAsConsole(string[] args)
		{
			OnStart(args);
			Console.WriteLine("Presione cualquier tecla para salir...");
			Console.ReadLine();
			OnStop();
		}

		public void SetFullControlPermissionsToEveryone(string path)
		{
			const FileSystemRights rights = FileSystemRights.FullControl;

			var allUsers = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);

			// Add Access Rule to the actual directory itself
			var accessRule = new FileSystemAccessRule(
				allUsers,
				rights,
				InheritanceFlags.None,
				PropagationFlags.NoPropagateInherit,
				AccessControlType.Allow);

			var info = new DirectoryInfo(path);
			var security = info.GetAccessControl(AccessControlSections.Access);

			bool result;
			security.ModifyAccessRule(AccessControlModification.Set, accessRule, out result);

			if (!result)
			{
				throw new InvalidOperationException("Failed to give full-control permission to all users for path " + path);
			}

			// add inheritance
			var inheritedAccessRule = new FileSystemAccessRule(
				allUsers,
				rights,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly,
				AccessControlType.Allow);

			bool inheritedResult;
			security.ModifyAccessRule(AccessControlModification.Add, inheritedAccessRule, out inheritedResult);

			if (!inheritedResult)
			{
				throw new InvalidOperationException("Failed to give full-control permission inheritance to all users for " + path);
			}

			info.SetAccessControl(security);
		}

		#endregion

	}
}
