

using IntegracionPAMI.APIConsumer.Dto;
using System.Data;

namespace IntegracionPAMI.Services
{
	public interface IIntegracionServices
	{
		bool AlmacenarEnBaseDedatos(string strNotificationType, ServiceDto serviceFromAPI);

		bool AnulacionEnBaseDedatos(string serviceID);

		bool ReclamoEnBaseDedatos(string serviceID);

		DataTable GetEstadosAsignacion();

        bool SetEstadoAsignacionEnviado(decimal pGalenoId, int pEventoId, string pWarning = "");

    }

}
