using System;
using System.Linq;
using System.Collections.Generic;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;
using System.Data;

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

		public  void GuardarNuevosServicios()
		{
			IEnumerable<NotificationDto> notificacions = servicioServices.GetNuevasNotifications();

			foreach (NotificationDto notification in notificacions.Where(n=>n.NotificationType ==  "Nuevo"))
			{
				ServiceDto service =  servicioServices.GetServicio(notification.ServiceID);

				bool isSuccess = _integracionServices.AlmacenarEnBaseDedatos(service);

				if(isSuccess)
				{
					 servicioServices.ReconocerNotification(notification.ServiceID, notification.Order);
				}
				else
				{
					throw new Exception("Hubo un inconveniente al almacenar el servicio en la BD");
				}
			}
		}

		public void EnviarEstadosAsignacion()
		{
			DataTable dt = _integracionServices.GetEstadosAsignacion();

            for (int i = 0; i< dt.Rows.Count; i++)
            {
                /// Envío a PAMI
                servicioServices.SetAssigmentState(dt.Rows[i]["NroServicio"].ToString(), dt.Rows[i]["Evento"].ToString());
                /// Marco enviado en DB
                _integracionServices.SetEstadoAsignacionEnviado(Convert.ToDecimal(dt.Rows[i]["NroServicio"].ToString()), Convert.ToInt32(dt.Rows[i]["EventoId"].ToString()));
                /// Finalizo en PAMI
                if (Convert.ToInt32(dt.Rows[i]["EventoId"]) == 4)
                {
                    servicioServices.Finalize(dt.Rows[i]["NroServicio"].ToString());
                }
            }
		}
	}
}
