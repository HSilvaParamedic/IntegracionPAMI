using System;
using System.Linq;
using System.Collections.Generic;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;
using System.Data;
using NLog;

namespace IntegracionPAMI.Services
{
	public class IntegracionPAMIManager
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IIntegracionServices _integracionServices;

		public IntegracionPAMIManager(IIntegracionServices servicioServices)
		{
			ApiHelper.InitializeClient();
			_integracionServices = servicioServices;
		}

		public static APIConsumer.Services.ServicioServices servicioServices = new APIConsumer.Services.ServicioServices();

		public  void GuardarNuevosServicios()
		{
			_logger.Info("Obteniendo nuevas notificaciones desde la API...");

			IEnumerable<NotificationDto> notificaciones = servicioServices.GetNuevasNotifications();
			_logger.Info($"Notificaciones obtenidas {notificaciones.Count()}");

			notificaciones = notificaciones.Where(n => n.NotificationType == "Nuevo");
			_logger.Info($"Notificaciones nuevas obtenidas {notificaciones.Count()}");

			foreach (NotificationDto notification in notificaciones)
			{
				_logger.Info($"Obteniendo servicio ID {notification.ServiceID} desde la API...");
				ServiceDto service =  servicioServices.GetServicio(notification.ServiceID);

				_logger.Info("$Almacenando servicio ID {notification.ServiceID} en BD...");
				bool isSuccess = _integracionServices.AlmacenarEnBaseDedatos(service);

				if (isSuccess)
				{
					_logger.Info("$Reconociendo notifiación de servicio ID {notification.ServiceID} en la API...");
					servicioServices.ReconocerNotification(notification.ServiceID, notification.Order);
				}
				else
				{
					throw new Exception("Hubo un inconveniente al almacenar el servicio en la BD");
				}
			}
			_logger.Info($"Notifiaciones guardadas, total {notificaciones.Count()}");
		}

		public void GuardarNuevosServiciosDesdeGoing()
        {
            IEnumerable<OngoingServiceDto> notificacions = servicioServices.GetServiciosEnCurso();

            foreach (OngoingServiceDto notification in notificacions)
            {
                ServiceDto service = servicioServices.GetServicio(notification.Id);

                bool isSuccess = _integracionServices.AlmacenarEnBaseDedatos(service);

            }
        }

        public void EnviarEstadosAsignacion()
		{
			DataTable dt = _integracionServices.GetEstadosAsignacion();

            for (int i = 0; i< dt.Rows.Count; i++)
            {
                /// Envío a PAMI
                servicioServices.SetAssigmentState(dt.Rows[i]["NroServicioString"].ToString(), dt.Rows[i]["Evento"].ToString());
                /// Marco enviado en DB
                _integracionServices.SetEstadoAsignacionEnviado(Convert.ToDecimal(dt.Rows[i]["ID"].ToString()), Convert.ToInt32(dt.Rows[i]["EventoId"].ToString()));
                /// Finalizo en PAMI
                if ((Convert.ToInt32(dt.Rows[i]["EventoId"]) == 4)||(Convert.ToInt32(dt.Rows[i]["EventoId"]) == 22))
                {
                    servicioServices.SetDiagnosticUrgencyDegree(dt.Rows[i]["NroServicioString"].ToString(), dt.Rows[i]["Diagnostico"].ToString(), dt.Rows[i]["GradoOperativo"].ToString());
                    servicioServices.Finalize(dt.Rows[i]["NroServicioString"].ToString());
                }
            }
		}
	}
}
