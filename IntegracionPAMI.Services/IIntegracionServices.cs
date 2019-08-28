

using IntegracionPAMI.APIConsumer.Dto;
using System.Data;

namespace IntegracionPAMI.Services
{
	public interface IIntegracionServices
	{
		bool AlmacenarEnBaseDedatos(ServiceDto serviceFromAPI);

		DataTable GetEstadosAsignacion();

        bool SetEstadoAsignacionEnviado(decimal pGalenoId, int pEventoId);

    }

}
