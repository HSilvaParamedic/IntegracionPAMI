using System.Linq;
using System.Collections.Generic;
using IntegracionPAMI.APIConsumer.Dto;

namespace IntegracionPAMI.Services
{
	public static class IntegracionPAMIManager
	{
		public static APIConsumer.Services.ServicioServices servicioServices = new APIConsumer.Services.ServicioServices();

		public static async void GuardarNuevosServicios()
		{
			IEnumerable<NotificationDto> notificaciones = await servicioServices.GetNuevasNotifications();

			foreach (NotificationDto notificacion in notificaciones.Where(n=>n.NotificationType ==  "Nuevo"))
			{
				ServiceDto service = await servicioServices.GetServicio(notificacion.ServiceID);

				//TODO: GuardarServicio en BD

				await servicioServices.ReconocerNotification(notificacion.ServiceID, notificacion.Order);
			}
		}
	}
}
