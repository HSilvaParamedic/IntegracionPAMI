using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;

namespace IntegracionPAMI.APIConsumer.Services
{
	public class ServicioServices
	{
		public async Task<IEnumerable<NotificationDto>> GetNuevasNotifications()
		{
			HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync("/GeoAPI/api/Notifications/GetPending");

			NotificationListDto notificationList = JsonConvert.DeserializeObject<NotificationListDto>(await response.Content.ReadAsStringAsync());

			return notificationList.Notifications.AsEnumerable();
		}

		public async Task ReconocerNotification(string servicioId, int order)
		{
			JObject jsonObject = new JObject(new JProperty("serviceID", servicioId), new JProperty("order", order.ToString()));
			StringContent content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
			HttpResponseMessage response = await ApiHelper.ApiClient.PostAsync("/GeoAPI/api/Notifications/Acknowledge", content);
		}

		public async Task<ServiceDto> GetServicio(string id)
		{
			HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync($"/GeoAPI/api/Services/GetById/{id}");

			ServiceDto service = JsonConvert.DeserializeObject<ServiceDto>(await response.Content.ReadAsStringAsync());

			return service;
		}
	}
}
