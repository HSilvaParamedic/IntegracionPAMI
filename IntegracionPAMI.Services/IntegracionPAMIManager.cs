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

		public void GuardarNuevosServicios(string strNotificationType = "Nuevo")
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

			/// Log Temporal
			foreach (NotificationDto notification in notificaciones)
			{
				if ((notification.NotificationType != "Nuevo") && (notification.NotificationType != "Anulación") && (notification.NotificationType != "Reiteración") && (notification.NotificationType != "Reclamo"))
                {
					_logger.Info($"Notificaciones New Type: {notification.NotificationType}");
				}
			}

			notificaciones = notificaciones.Where(n => n.NotificationType == strNotificationType);
			notificacionesObtenidasNuevasCount = notificaciones.Count();

			_logger.Info($"Notificaciones: {strNotificationType}");
			_logger.Info($"Notificaciones nuevas obtenidas {notificacionesObtenidasNuevasCount} de {notificacionesObtenidasCount} notificaciones en total.");
			
			foreach (NotificationDto notification in notificaciones)
			{
				try
				{

					bool isSuccess = false;

					switch (strNotificationType)
                    {
						case "Nuevo":
						case "Reiteración":
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

							isSuccess = _integracionServices.AlmacenarEnBaseDedatos(strNotificationType, service);

							break;

						case "Anulación":

							isSuccess = _integracionServices.AnulacionEnBaseDedatos(notification.ServiceID);

							break;

						case "Reclamo":

							isSuccess = _integracionServices.ReclamoEnBaseDedatos(notification.ServiceID);

							break;

					}

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

						if (strNotificationType != "Anulación")
                        {
							throw new Exception($"Hubo un inconveniente al almacenar el servicio (ID {notification.ServiceID}) en la BD");
						}
						else
                        {
							throw new Exception($"Hubo un inconveniente al intentar anular el servicio (ID {notification.ServiceID}) en la BD");
						}
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


            ServiceDto service = servicioServices.GetServicio("2003592026");

            bool isSuccess = _integracionServices.AlmacenarEnBaseDedatos("Nuevo", service);

            /*
            IEnumerable<OngoingServiceDto> notificacions = servicioServices.GetServiciosEnCurso();

			foreach (OngoingServiceDto notification in notificacions)
			{

                if (notification.Id == "2000170919")
                {

                    ServiceDto service = servicioServices.GetServicio(notification.Id);

                    bool isSuccess = _integracionServices.AlmacenarEnBaseDedatos(service);

                }


            }
            */
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
                if (dt.Rows[i]["LugarDerivacion"].ToString().Length > 3)
                {
                    servicioServices.SetHealthCareCenter(sNroServicio, dt.Rows[i]["LugarDerivacion"].ToString());
                }

				/// Evento
				
				DevOps evento = servicioServices.SetAssigmentState(sNroServicio, dt.Rows[i]["Evento"].ToString());

				if (evento.Resultado != DevOps.result.Error)
                {

                    /// Marco enviado en DB
                    _integracionServices.SetEstadoAsignacionEnviado(Convert.ToDecimal(dt.Rows[i]["ID"].ToString()), Convert.ToInt32(dt.Rows[i]["EventoId"].ToString()), evento.DescripcionError);

                    /// Finalizo en PAMI
                    if ((Convert.ToInt32(dt.Rows[i]["EventoId"]) == 8)||(Convert.ToInt32(dt.Rows[i]["EventoId"]) == 28))
                    {

                        /// Envío el disponible
						if (dt.Rows[i]["Evento"].ToString() != "Disponible")
                        {
							servicioServices.SetAssigmentState(sNroServicio, "Disponible");
						}

                        /// Marco enviado en DB
                        if (Convert.ToInt32(dt.Rows[i]["EventoId"]) == 8)
                        {
                            _integracionServices.SetEstadoAsignacionEnviado(Convert.ToDecimal(dt.Rows[i]["ID"].ToString()), 9);
                        }
                        else
                        {
                            _integracionServices.SetEstadoAsignacionEnviado(Convert.ToDecimal(dt.Rows[i]["ID"].ToString()), 29);
                        }

                        /// Cierre de Servicio
                        if (servicioServices.SetFinalDestination(sNroServicio, dt.Rows[i]["DestinoFinal"].ToString()).Resultado == DevOps.result.OK)
                        {
                            if (servicioServices.SetDiagnosticUrgencyDegree(sNroServicio, dt.Rows[i]["Diagnostico"].ToString(), dt.Rows[i]["GradoOperativo"].ToString()).Resultado == DevOps.result.OK)
                            {
								servicioServices.Finalize(sNroServicio);
                            }
                        }
                    }

				}
			}
        }
	}
}
