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

		public bool SetAssigmentState(string servicioId, string description)
		{
			try
			{
				_logger.Info(string.Format("SetAssigmentState: Estableciendo {0} para Servicio {1}", description, servicioId));

				JObject jsonObject = new JObject(new JProperty("serviceID", servicioId), new JProperty("description", description));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetAssignmentState"), content).Result;

                _logger.Info(string.Format("SetAssigmentState: Registrado Ok Servicio {0}", servicioId));

            }
			catch (Exception ex)
			{
				_logger.Info(string.Format("SetAssigmentState: Error {0}", ex.InnerException.Message));
                if (ex.InnerException.Message == "El incidente consultado no esta asignado al usuario indicado") { return false; };
			}
            return true;
        }

        public bool SetDiagnosticUrgencyDegree(string servicioId, string diagnosticCode, string urgencyDegreeCode)
        {
			try
			{
                _logger.Info(string.Format("SetDiagnosticUrgencyDegree: Estableciendo {0}, {1} para Servicio {2}", diagnosticCode, urgencyDegreeCode, servicioId));

                JObject jsonObject = new JObject(new JProperty("ServiceID", servicioId), new JProperty("DiagnosticCode", diagnosticCode), new JProperty("UrgencyDegreeCode", urgencyDegreeCode));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetDiagnosticUrgencyDegree"), content).Result;

                _logger.Info(string.Format("SetDiagnosticUrgencyDegree: Registrado Ok Servicio {0}", servicioId));
                return true;
            }
			catch (Exception ex)
			{
                _logger.Info(string.Format("SetDiagnosticUrgencyDegree: Error {0}", ex.InnerException.Message));
                return false;
			}
        }

		public void SetFinalDestination(string servicioId, string finalDestinationCode)
		{
			try
			{
                _logger.Info(string.Format("SetFinalDestination: Estableciendo {0} para Servicio {1}", finalDestinationCode, servicioId));

                JObject jsonObject = new JObject(new JProperty("ServiceID", servicioId), new JProperty("FinalDestinationCode", finalDestinationCode));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetFinalDestination"), content).Result;

                _logger.Info(string.Format("SetFinalDestination: Registrado Ok Servicio {0}", servicioId));
            }
            catch (Exception ex)
			{
                _logger.Info(string.Format("SetFinalDestination: Error {0}", ex.InnerException.Message));
                throw;
			}
		}

		public bool SetHealthCareCenter(string servicioId, string heathcareCenter)
		{
			try
			{
                _logger.Info(string.Format("SetHealthCareCenter: Estableciendo {0} para Servicio {1}", heathcareCenter, servicioId));

                JObject jsonObject = new JObject(new JProperty("ServiceID", servicioId), new JProperty("HeathcareCenter", heathcareCenter));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetHealthCareCenter"), content).Result;

                _logger.Info(string.Format("SetHealthCareCenter: Registrado Ok Servicio {0}", servicioId));
                return true;
			}
			catch (Exception ex)
			{
                _logger.Info(string.Format("SetHealthCareCenter: Error {0}", ex.InnerException.Message));
                return false;
            }
		}

		public bool SetAssignmentComment(string servicioId, string comments)
		{
			try
			{
                _logger.Info(string.Format("SetAssignmentComment: Estableciendo {0} para Servicio {1}", comments, servicioId));

                JObject jsonObject = new JObject(new JProperty("ServiceID", servicioId), new JProperty("Comments", comments));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetAssignmentComment"), content).Result;

                _logger.Info(string.Format("SetAssignmentComment: Registrado Ok Servicio {0}", servicioId));
                return true;
            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("SetHealthCareCenter: Error {0}", ex.InnerException.Message));
                return false;
            }
        }

		public bool Finalize(string servicioId)
		{
			try
			{
                _logger.Info(string.Format("Finalize: Estableciendo para Servicio {1}", servicioId));

                JObject jsonObject = new JObject(new JProperty("serviceID", servicioId));
				StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_Finalize"), content).Result;

                _logger.Info(string.Format("Finalize: Registrado Ok Servicio {0}", servicioId));
                return true;
			}
			catch (Exception ex)
			{
                _logger.Info(string.Format("Finalize: Error {0}", ex.InnerException.Message));
                return false;
            }
		}
	}
}
