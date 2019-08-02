using System;
using IntegracionPAMI.Services;
using IntegracionPAMI.APIConsumer.Dto;


namespace IntegracionPAMI.WindowsService.Cache.Services
{
	public class IntegracionService : IIntegracionServices
	{
		public bool AlmacenarEnBaseDedatos(ServiceDto serviceFromAPI)
		{
			//TODO: realizar el mapeo y almacenamiento en la base de datos de cache
			throw new NotImplementedException();
		}
	}
}
