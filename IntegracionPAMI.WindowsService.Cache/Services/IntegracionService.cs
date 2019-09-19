using System;
using System.Configuration;
using InterClientesC;
using IntegracionPAMI.Services;
using IntegracionPAMI.APIConsumer.Dto;
using NLog;
using System.Linq;
using System.Data;

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

                /// Observaciones
                string sObs = serviceDto.OriginComments;

                AttributeDto atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Tratamiento preferencial");
                if (atr != null && atr.Value.Length > 2)
                {
                    sObs = sObs + " - Tratamiento Preferencial: " + atr.Value;
                }

                atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Módulo de internación");
                if (atr != null && atr.Value.Length > 2)
                {
                    sObs = sObs + " Módulo de internación: " + atr.Value;
                }

                DevSetServicio vRdo = new GalenoServicios(connectionStringCache).SetServicio(
						cliCod,
						nroAut,
						serviceDto.Id,
						serviceDto.Address.StreetName,
						int.Parse(serviceDto.Address.HouseNumber),
						0,
                        serviceDto.Address.FloorApt,
						serviceDto.Address.BetweenStreet1,
						serviceDto.Address.BetweenStreet2,
						"",
						serviceDto.BeneficiaryName,
						serviceDto.Gender,
						serviceDto.Age.HasValue ? MapEdad(serviceDto.Age.Value, serviceDto.AgeUnit) : "",
						serviceDto.Triage.Last().Reason,
						serviceDto.phoneNumber,
						serviceDto.Address.City,
						serviceDto.BeneficiaryID,
						new DateTime(serviceDto.TimeRequested.Year, serviceDto.TimeRequested.Month, serviceDto.TimeRequested.Day),
						$"{serviceDto.TimeRequested.Hour}:{serviceDto.TimeRequested.Minute}",
						0,
						"",
						MapGrado(serviceDto.Classification),
						"",
                        sObs,
                        serviceDto.Address.LatLng.Latitude,
                        serviceDto.Address.LatLng.Longitude
                );

                if (vRdo != null)
                {
                    return vRdo.Resultado;
                }
				
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}
            return false;
        }

        public DataTable GetEstadosAsignacion()
		{
			ConnectionStringCache connectionStringCache = GetConnectionStringCache();
			DataTable dt = new GalenoServicios(connectionStringCache).GetPamiEstadosAsignacionPendientes(cliCod);
			for (int i = 0; i < dt.Rows.Count - 1; i++)
			{
				if ((int.Parse(dt.Rows[i]["EventoId"].ToString()) == 4) && (int.Parse(dt.Rows[i]["EstadoCierre"].ToString()) == 1))
				{
					dt.Rows[i]["GradoOperativoId"] = this.MapGradoToRest(dt.Rows[i]["GradoOperativoId"].ToString());
				}
			}
			return dt;
		}

		public bool SetEstadoAsignacionEnviado(decimal pGalenoId, int pEventoId)
		{
			ConnectionStringCache connectionStringCache = GetConnectionStringCache();
			return new GalenoServicios(connectionStringCache).SetPamiEventoEnviado(pGalenoId, pEventoId);
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
					return "R";
			}
		}

		private string MapGradoToRest(string grade)
		{
			switch (grade.Trim())
			{
				case "V":
					return "Verde";
				case "A":
					return "Amarillo";
				case "R":
					return "Rojo";
				default:
					return "";
			}
		}

		private string MapEdad(int age, string ageUnit)
		{
			switch (ageUnit.ToLower())
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
