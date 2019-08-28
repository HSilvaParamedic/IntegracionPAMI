using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;
using System.Configuration;

namespace IntegracionPAMI.APIConsumer.Services
{
	public class ServicioServices
	{
		public IEnumerable<NotificationDto> GetNuevasNotifications()
		{
			HttpResponseMessage response = ApiHelper.ApiClient.GetAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_GetPendingNotifications")).Result;

			NotificationListDto notificationList = JsonConvert.DeserializeObject<NotificationListDto>(response.Content.ReadAsStringAsync().Result);

			return notificationList.Notifications.AsEnumerable();
		}

        public IEnumerable<OngoingServiceDto> GetServiciosEnCurso()
        {
            HttpResponseMessage response = ApiHelper.ApiClient.GetAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_GetOngoing")).Result;

            OngoingServicesListDto notificationList = JsonConvert.DeserializeObject<OngoingServicesListDto>(response.Content.ReadAsStringAsync().Result);

            return notificationList.Summary.AsEnumerable();
        }

        public  void ReconocerNotification(string servicioId, int order)
		{
			JObject jsonObject = new JObject(new JProperty("serviceID", servicioId), new JProperty("order", order.ToString()));
			StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
			HttpResponseMessage response =  ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_AcknowledgeNotification"), content).Result;
		}

		public ServiceDto GetServicio(string id)
		{
			HttpResponseMessage response = ApiHelper.ApiClient.GetAsync($"{ConfigurationManager.AppSettings.Get("API_Endpoint_GetService")}{id}").Result;

			ServiceDto service = JsonConvert.DeserializeObject<ServiceDto>(response.Content.ReadAsStringAsync().Result);
			service.Id = id;

			return service;
		}

		public void SetAssigmentState(string servicioId, string description)
		{
			JObject jsonObject = new JObject(new JProperty("serviceID", servicioId), new JProperty("description", description));
			StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
			HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetAssignmentState"), content).Result;
		}

        public void Finalize(string servicioId)
        {
            JObject jsonObject = new JObject(new JProperty("serviceID", servicioId));
            StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_Finalize"), content).Result;
        }

        public void SetDiagnosticUrgencyDegree(string servicioId, string DiagnosticCode, string UrgencyDegreeCode)
        {
            JObject jsonObject = new JObject(new JProperty("ServiceID", servicioId), new JProperty("DiagnosticCode", DiagnosticCode), new JProperty("UrgencyDegreeCode", UrgencyDegreeCode));
            StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = ApiHelper.ApiClient.PutAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_SetDiagnosticUrgencyDegree"), content).Result;
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