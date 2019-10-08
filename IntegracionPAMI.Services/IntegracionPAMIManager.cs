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

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Finalización de guardado de servicios.");
			sb.AppendLine($"Total notificaciones nuevas obtenidas: {notificacionesObtenidasNuevasCount}.");

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

			if (notificacionesObtenidasNuevasCount > 0)
			{
				sb.AppendLine($"				TOTAL de servicios procesados correctamente: {servicioProcesoCompletoCount}");
				sb.AppendLine($"				TOTAL de servicios NO procesados por error: {notificacionesObtenidasNuevasCount - servicioProcesoCompletoCount}");
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

                string sNroServicio = dt.Rows[i]["NroServicioString"].ToString();
                if (sNroServicio == "")
                {
                    sNroServicio = dt.Rows[i]["NroServicio"].ToString();
                }

                /// Centro de Derivación
                if (dt.Rows[i]["LugarDerivacion"].ToString() != "")
                {

                }

                //// Suceso de PAMI
                try
                {
                    servicioServices.SetAssigmentState(sNroServicio, dt.Rows[i]["Evento"].ToString());
                }
                catch (Exception)
                {
                    /// No hay acción por ahora
                }

                /// Marco enviado en DB
                _integracionServices.SetEstadoAsignacionEnviado(Convert.ToDecimal(dt.Rows[i]["ID"].ToString()), Convert.ToInt32(dt.Rows[i]["EventoId"].ToString()));


                /// Finalizo en PAMI
                if (Convert.ToInt32(dt.Rows[i]["EventoId"]) == 8)
                {
                    try
                    {
                        servicioServices.SetAssigmentState(sNroServicio, "9");
                    }
                    catch (Exception)
                    {
                        /// No hay acción por ahora
                    }
                    /// Marco enviado en DB
                    _integracionServices.SetEstadoAsignacionEnviado(Convert.ToDecimal(dt.Rows[i]["ID"].ToString()), 9);

                    try
                    {
                        servicioServices.SetDiagnosticUrgencyDegree(sNroServicio, dt.Rows[i]["Diagnostico"].ToString(), dt.Rows[i]["GradoOperativo"].ToString());
                    }
                    catch (Exception)
                    {
                        /// No hay acción por ahora
                    }

                    try
                    {
                        servicioServices.Finalize(sNroServicio);
                    }
                    catch (Exception)
                    {
                        /// No hay acción por ahora
                    }
                }
            }
        }
	}
}
