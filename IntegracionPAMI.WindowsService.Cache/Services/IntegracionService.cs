using System;
using System.Configuration;
using InterClientesC;
using IntegracionPAMI.Services;
using IntegracionPAMI.APIConsumer.Dto;
using NLog;
using System.Linq;

namespace IntegracionPAMI.WindowsService.Cache.Services
{
	public class IntegracionService : IIntegracionServices
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		private string cliCod = ConfigurationManager.AppSettings.Get("ServicioMap_pCliCod");
		private int nroAut = int.Parse(ConfigurationManager.AppSettings.Get("ServicioMap_pNroAut"));

		public bool AlmacenarEnBaseDedatos(ServiceDto serviceDto)
		{
			try
			{
				ConnectionStringCache connectionStringCache = GetConnectionStringCache();

				new GalenoServicios(connectionStringCache).SetServicio(
					cliCod,
					nroAut,
					serviceDto.Id,
					$"{serviceDto.Address.StreetName} {serviceDto.Address.FloorApt}",
					int.Parse(serviceDto.Address.HouseNumber),
					0,
					"",
					serviceDto.Address.BetweenStreet1,
					serviceDto.Address.BetweenSteet2,
					"",
					serviceDto.BeneficiaryName,
					serviceDto.Gender,
					MapEdad(serviceDto.Age, serviceDto.AgeUnit),
					serviceDto.Triage.Last().Reason,
					"",
					serviceDto.Address.City,
					serviceDto.BeneficiaryID,
					new DateTime(serviceDto.TimeRequested.Year, serviceDto.TimeRequested.Month, serviceDto.TimeRequested.Day),
					$"{serviceDto.TimeRequested.Hour}:{serviceDto.TimeRequested.Minute}",
					0,
					"",
					MapGrado(serviceDto.Clasification),
					"",
					serviceDto.OriginComments
				);

				return true;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				return false;
			}
		}

		private ConnectionStringCache GetConnectionStringCache()
		{
			string[] connectionStringCacheValues = ConfigurationManager.AppSettings.Get("ConnectionStringCache_Values").Split('|');
			return new ConnectionStringCache
			{
				Namespace = connectionStringCacheValues[0],
				Port = connectionStringCacheValues[1],
				Server = connectionStringCacheValues[2],
				Aplicacion = connectionStringCacheValues[3],
				Centro = connectionStringCacheValues[4],
				User = connectionStringCacheValues[5],
				Password = connectionStringCacheValues[6],
				UserID = connectionStringCacheValues[7]
			};
		}

		private string MapGrado(string grade)
		{
			switch (grade.Trim())
			{
				case "Verde":
					return "V";
				case "Consulta":
					return "V";
				case "Llamadas Grales":
					return "V";
				case "Amarillo":
					return "A";
				case "Traslado Amaril":
					return "A";
				case "Traslado Común":
					return "A";
				case "Rederiv media":
					return "A";
				case "Rederiv baja":
					return "A";
				case "Sin Clasificar":
					return "A";
				case "Super rojo":
					return "R";
				case "Rojo":
					return "R";
				case "Traslado Rojo":
					return "R";
				case "Traslado TaT":
					return "R";
				case "Rederiv alta":
					return "R";
				case "Oficio Psiquiát":
					return "R";
				case "Oficio Clinico":
					return "R";
				default:
					throw new Exception("No se reconoce el grado");
			}
		}

		private string MapEdad(int age, string ageUnit)
		{
			switch (ageUnit)
			{
				case "años":
					return age.ToString();
				case "Meses":
					return $"{age} M";
				case "Días":
					return $"{age} D";
				default:
					throw new Exception("No se reconoce la unidad de la edad");
			}
		}
	}
}
