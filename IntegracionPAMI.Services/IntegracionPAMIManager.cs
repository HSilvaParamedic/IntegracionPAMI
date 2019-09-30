using System;
using System.Linq;
using System.Collections.Generic;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;
using System.Data;
using NLog;
using System.Text;

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

		public void GuardarNuevosServicios()
		{
			int notificacionesObtenidasCount = 0;
			int notificacionesObtenidasNuevasCount = 0;
			int serviciosObtenidosAPICount = 0;
			int serviciosGuardadosCount = 0;
			int notifiacionesReconocidasCount = 0;
			int servicioProcesoCompletoCount = 0;

			List<string> servicioIdsAPIError = new List<string>();
			List<string> servicioIdsBDError = new List<string>();
			List<string> servicioIdsNotifacionesReconocidasAPIError = new List<string>();

			ServiceDto service;


			IEnumerable<NotificationDto> notificaciones = servicioServices.GetNuevasNotifications();
			notificacionesObtenidasCount = notificaciones.Count();

			notificaciones = notificaciones.Where(n => n.NotificationType == "Nuevo");
			notificacionesObtenidasNuevasCount = notificaciones.Count();

			_logger.Info($"Notificaciones nuevas obtenidas {notificacionesObtenidasNuevasCount} de {notificacionesObtenidasCount} notificaciones en total.");


			_logger.Info($"Comenzando guardado de servicios...");
			foreach (NotificationDto notification in notificaciones)
			{
				try
				{
					try
					{
						service = servicioServices.GetServicio(notification.ServiceID);
						serviciosObtenidosAPICount++;
					}
					catch (Exception)
					{
						servicioIdsAPIError.Add(notification.ServiceID);
						throw;
					}

					bool isSuccess = _integracionServices.AlmacenarEnBaseDedatos(service);

					if (isSuccess)
					{
						serviciosGuardadosCount++;
						try
						{
							servicioServices.ReconocerNotification(notification.ServiceID, notification.Order);
							notifiacionesReconocidasCount++;
							servicioProcesoCompletoCount++;
						}
						catch (Exception)
						{
							servicioIdsNotifacionesReconocidasAPIError.Add(notification.ServiceID);
							throw;
						}
					}
					else
					{
						servicioIdsBDError.Add(notification.ServiceID);
						throw new Exception($"Hubo un inconveniente al almacenar el servicio (ID {notification.ServiceID})  en la BD");
					}
				}
				catch (Exception ex)
				{
					_logger.Error(ex, ex.Message);
				}
			}

			StringBuilder sb = new StringBuilder($"Finalización de guardado de servicios. Total por guardar: {notificacionesObtenidasNuevasCount}.");

			if (notificacionesObtenidasNuevasCount != serviciosObtenidosAPICount)
			{
				sb.AppendLine($"				Total obtenidos API: {serviciosObtenidosAPICount};");
				sb.AppendLine($"				Total sin obtener API: {servicioIdsAPIError.Count()} ({string.Join("|", servicioIdsAPIError)});");
			}

			if (notificacionesObtenidasNuevasCount != serviciosGuardadosCount)
			{
				sb.AppendLine($"				Total guardados BD: {serviciosGuardadosCount};");
				sb.AppendLine($"				Total sin guardar BD: {servicioIdsBDError.Count()} ({string.Join("|", servicioIdsBDError)});");
			}

			if (notificacionesObtenidasNuevasCount != notifiacionesReconocidasCount)
			{
				sb.AppendLine($"				Total notificados API: {notifiacionesReconocidasCount};");
				sb.AppendLine($"				Total sin notificar API: {servicioIdsNotifacionesReconocidasAPIError.Count()} ({string.Join("|", servicioIdsNotifacionesReconocidasAPIError)});");
			}

			if(notificacionesObtenidasNuevasCount >0)
			{
				sb.AppendLine($"				TOTAL servicio proceso completo: {servicioProcesoCompletoCount}");
			}

			_logger.Info(sb.ToString());
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

			for (int i = 0; i < dt.Rows.Count; i++)
			{
				/// Envío a PAMI
				servicioServices.SetAssigmentState(dt.Rows[i]["NroServicioString"].ToString(), dt.Rows[i]["Evento"].ToString());
				/// Marco enviado en DB
				_integracionServices.SetEstadoAsignacionEnviado(Convert.ToDecimal(dt.Rows[i]["ID"].ToString()), Convert.ToInt32(dt.Rows[i]["EventoId"].ToString()));
				/// Finalizo en PAMI
				//if ((Convert.ToInt32(dt.Rows[i]["EventoId"]) == 4) || (Convert.ToInt32(dt.Rows[i]["EventoId"]) == 22))
				//{
				//	servicioServices.SetDiagnosticUrgencyDegree(dt.Rows[i]["NroServicioString"].ToString(), dt.Rows[i]["Diagnostico"].ToString(), dt.Rows[i]["GradoOperativo"].ToString());
				//	servicioServices.Finalize(dt.Rows[i]["NroServicioString"].ToString());
				//}
			}
		}
	}
}
