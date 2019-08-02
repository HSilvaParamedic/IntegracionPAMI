using System.Linq;
using System.Collections.Generic;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;

namespace IntegracionPAMI.Services
{
	public class IntegracionPAMIManager
	{
		private readonly IIntegracionServices _integracionServices;
		public IntegracionPAMIManager(IIntegracionServices servicioServices)
		{
			ApiHelper.InitializeClient();
			_integracionServices = servicioServices;
		}

		public static APIConsumer.Services.ServicioServices servicioServices = new APIConsumer.Services.ServicioServices();

		public async void GuardarNuevosServicios()
		{
			IEnumerable<NotificationDto> notificacions = await servicioServices.GetNuevasNotifications();

			foreach (NotificationDto notification in notificacions.Where(n=>n.NotificationType ==  "Nuevo"))
			{
				ServiceDto service = await servicioServices.GetServicio(notification.ServiceID);

				bool isSuccess = _integracionServices.AlmacenarEnBaseDedatos(service);

				if(isSuccess)
				{
					await servicioServices.ReconocerNotification(notification.ServiceID, notification.Order);
				}
				{

				}
			}
		}
	}
}
