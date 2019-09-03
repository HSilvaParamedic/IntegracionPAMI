using System;
using System.Timers;
using System.Diagnostics;
using System.Configuration;
using System.ServiceProcess;
using IntegracionPAMI.Services;
using IntegracionPAMI.WindowsService.SQL.Services;
using NLog;

using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace IntegracionPAMI.WindowsService.SQL
{
    public partial class IntegracionPAMIWindowsServiceSQL : ServiceBase
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        ///private int eventId = 1;
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
                throw ex;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {

                ElapsedHandler();

                timer = new Timer();
                timer.Elapsed += delegate { ElapsedHandler(); };
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

         //protected override void OnStart(string[] args)
        //{
        //    try
        //    {

        //        timer = new Timer();
        //        timer.Interval = int.Parse(ConfigurationManager.AppSettings.Get("IntervaloDeEjecucion_Mins")) * 10000;
        //        timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
        //        timer.Start();

        //        _logger.Info("Se inició el servicio");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex, ex.Message);
        //        throw ex;
        //    }
        //}

        protected override void OnStop()
        {
            _logger.Info("Se detuvo el servicio");
        }

        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                _integracionPAMIManager.GuardarNuevosServicios();
                ///_integracionPAMIManager.EnviarEstadosAsignacion();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
        }

        private void ElapsedHandler()
        {
            try
            {
                _integracionPAMIManager.GuardarNuevosServicios();
                ///_integracionPAMIManager.EnviarEstadosAsignacion();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
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
