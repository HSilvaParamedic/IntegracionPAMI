

using IntegracionPAMI.APIConsumer.Dto;

namespace IntegracionPAMI.Services
{
	public interface IIntegracionServices
	{
		bool AlmacenarEnBaseDedatos(ServiceDto serviceFromAPI);

        bool EnviarEstadosAsignacion(ServiceDto serviceFromAPI);
    }


}
