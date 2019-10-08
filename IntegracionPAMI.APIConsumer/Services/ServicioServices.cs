using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Configuration;
using System.Collections.Generic;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;

namespace IntegracionPAMI.APIConsumer.Services
{
	public class ServicioServices
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();

		public IEnumerable<NotificationDto> GetNuevasNotifications()
		{
			try
			{
				_logger.Info("Obteniendo nuevas notificaciones desde API...");

				HttpResponseMessage response = ApiHelper.ApiClient.GetAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_GetPendingNotifications")).Result;

				NotificationListDto notificationList = JsonConvert.DeserializeObject<NotificationListDto>(response.Content.ReadAsStringAsync().Result);

				_logger.Info("Finalización de obtención de nuevas notificaciones desde API.");

				return notificationList.Notifications.AsEnumerable();
			}
			catch (Exception)
			{
				_logger.Info("Finalización CON ERRORES de obtención de nuevas notificaciones desde API.");
				throw;
			}
		}

		public IEnumerable<OngoingServiceDto> GetServiciosEnCurso()
        {
			try
			{
				_logger.Info("Obteniendo servicios em curso desde API...");

				HttpResponseMessage response = ApiHelper.ApiClient.GetAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_GetOngoing")).Result;

				OngoingServicesListDto notificationList = JsonConvert.DeserializeObject<OngoingServicesListDto>(response.Content.ReadAsStringAsync().Result);

				_logger.Info("Finalización de obtención de servicios en curso desde API.");

				return notificationList.Summary.AsEnumerable();
			}
			catch (Exception)
			{
				_logger.Info("Finalización CON ERRORES de obtención de servicios en curso desde API.");
				throw;
			}
        }

        public  void ReconocerNotification(string servicioId, int order)
		{
			try
			{
				_logger.Info($"Reconociendo notificación de servicio (ID {servicioId}) desde API...");

				JObject jsonObject = new JObject(new JProperty("serviceID", servicioId), new JProperty("order", order.ToString()));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_AcknowledgeNotification"), content).Result;

				_logger.Info($"Finalización de reconocimiento de notificación de servicio (ID {servicioId}) desde API.");
			}
			catch (Exception)
			{
				_logger.Info($"Finalización CON ERRORES de reconocimiento de notificación de servicio (ID {servicioId}) desde API.");
				throw;
			}
		}

		public ServiceDto GetServicio(string id)
		{
			try
			{
				_logger.Info($"Obteniendo servicio (ID {id}) desde API...");

				HttpResponseMessage response = ApiHelper.ApiClient.GetAsync($"{ConfigurationManager.AppSettings.Get("API_Endpoint_GetService")}{id}").Result;

				ServiceDto service = JsonConvert.DeserializeObject<ServiceDto>(response.Content.ReadAsStringAsync().Result);
				service.Id = id;

				_logger.Info($"Finalización de obtención de servicio (ID {id}) desde API.");

				return service;
			}
			catch (Exception)
			{
				_logger.Info($"Finalización CON ERRORES de obtención de servicio (ID {id}) desde API.");
				throw;
			}
		}

		public void SetAssigmentState(string servicioId, string description)
		{
			try
			{
				_logger.Info("Estableciendo estado de asignación desde API...");

				JObject jsonObject = new JObject(new JProperty("serviceID", servicioId), new JProperty("description", description));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetAssignmentState"), content).Result;

				_logger.Info("Finalización de establecimiento de estado de asignación desde API.");
			}
			catch (Exception ex)
			{
				_logger.Info(string.Format("ServiceId {0} - Suceso {1} - Error {0}", servicioId, description, ex.InnerException.Message));
				throw;
			}
		}

        public void Finalize(string servicioId)
        {
			try
			{
				_logger.Info("Finalizando servicio desde API...");

				JObject jsonObject = new JObject(new JProperty("serviceID", servicioId));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_Finalize"), content).Result;

				_logger.Info("Finalización del finalizado de servicio desde API.");
			}
			catch (Exception)
			{
				_logger.Info("Finalización CON ERRORES del finalizado de servicio desde API.");
				throw;
			}
        }

        public void SetDiagnosticUrgencyDegree(string servicioId, string DiagnosticCode, string UrgencyDegreeCode)
        {
			try
			{
				_logger.Info("Estableciendo grado de diagnostico desde API...");

				JObject jsonObject = new JObject(new JProperty("ServiceID", servicioId), new JProperty("DiagnosticCode", DiagnosticCode), new JProperty("UrgencyDegreeCode", UrgencyDegreeCode));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetDiagnosticUrgencyDegree"), content).Result;

				_logger.Info("Finalización de establecimiento de grado de diagnostico API.");
			}
			catch (Exception)
			{
				_logger.Info("Finalización CON ERRORES de establecimiento de grado de diagnostico API.");
				throw;
			}
        }

    }
}
//[
//    {
//        "description": "Informado al recurso"
//    },
//    {
//        "description": "Despachado"
//    },
//    {
//        "description": "Rumbo a incidente"
//    },
//    {
//        "description": "En incidente"
//    },
//    {
//        "description": "En espera de derivación"
//    },
//    {
//        "description": "Rumbo a lugar de derivación"
//    },
//    {
//        "description": "En lugar de derivación"
//    },
//    {
//        "description": "Rumbo a base/Deja al paciente"
//    },
//    {
//        "description": "Disponible"
//    }
//]

	//{"ServiceID":"0010004173", "Description":"Disponible"}