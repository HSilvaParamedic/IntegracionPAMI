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

		public  void ReconocerNotification(string servicioId, int order)
		{
			JObject jsonObject = new JObject(new JProperty("serviceID", servicioId), new JProperty("order", order.ToString()));
			StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
			HttpResponseMessage response =  ApiHelper.ApiClient.PostAsync(ConfigurationManager.AppSettings.Get("API_Endpoint_AcknowledgeNotification"), content).Result;
		}

		public ServiceDto GetServicio(string id)
		{
			HttpResponseMessage response = ApiHelper.ApiClient.GetAsync($"{ConfigurationManager.AppSettings.Get("API_Endpoint_GetService")}{id}").Result;

			ServiceDto service = JsonConvert.DeserializeObject<ServiceDto>(response.Content.ReadAsStringAsync().Result);
			service.Id = id;

			return service;
		}
	}
}
