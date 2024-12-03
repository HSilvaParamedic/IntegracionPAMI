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
		private bool useSuperRojo = int.Parse(ConfigurationManager.AppSettings.Get("ServicioMap_SuperRojo")) == 0 ? false : true;

		public bool AlmacenarEnBaseDedatos(string strNotificationType, ServiceDto serviceDto)
		{
			try
			{

				switch (strNotificationType)
				{
					case "Nuevo":
						return this.CrearNuevo(serviceDto);
					case "Reiteración":
						return this.EstablecerReiteracion(serviceDto);
				}

			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				_logger.Info($"Finalización CON ERRORES de almacenamiento de servicio (ID {serviceDto.Id}) en BD.");
				
			}
			return false;
		}

		private bool CrearNuevo(ServiceDto serviceDto)
		{
			try
			{

				_logger.Info($"Almacenando servicio (ID {serviceDto.Id}) en BD...");

				ConnectionStringCache connectionStringCache = GetConnectionStringCache();

				/// Observaciones
				string sObs = serviceDto.OriginComments;

				sObs = sObs + " - Grado PAMI: " + serviceDto.Classification;

				//// Tratamiento Preferencial

				AttributeDto atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Tratamiento preferencial");
				if (atr != null && atr.Value.Length > 2)
				{
					sObs = sObs + " - Tratamiento Preferencial: " + atr.Value;
				}

				//// Módulo de internación

				atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Módulo de internación");
				if (atr != null && atr.Value.Length > 2)
				{
					sObs = sObs + " - Módulo de internación: " + atr.Value;
				}

				//// Documento

				string nroDocumento = "";

				atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Número de documento");
				if (atr != null)
				{
					sObs = sObs + " - Número de documento: " + atr.Value;
					nroDocumento = atr.Value;
				}

				/// Sintomas

				string sSintoma = "";

				try
				{
					sSintoma = serviceDto.Triage.Last().Reason.ToUpper();

					if (sSintoma.Contains("EXCLUYE"))
					{
						sSintoma = serviceDto.Triage.First().Reason.ToUpper();
					}
				}
				catch (Exception)
				{

				}

				string localidad = serviceDto.Address.City;
				string barrio = "";

				if (serviceDto.Address.Neighborhood != "")
                {
					barrio = serviceDto.Address.Neighborhood;
				}

				DevResultado vRdo = new GalenoServicios(connectionStringCache).SetServicio(
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
						sSintoma,
						serviceDto.phoneNumber,
						localidad,
						barrio,
						serviceDto.BeneficiaryID,
						nroDocumento,
						new DateTime(serviceDto.TimeRequested.Year, serviceDto.TimeRequested.Month, serviceDto.TimeRequested.Day),
						$"{serviceDto.TimeRequested.Hour}:{serviceDto.TimeRequested.Minute}",
						0,
						"",
						MapGrado(serviceDto.Classification),
						"",
						sObs,
						serviceDto.Address.LatLng.Latitude,
						serviceDto.Address.LatLng.Longitude,
						0,
						serviceDto.Address.AdditionalData
				);

				if (vRdo == null)
				{
					throw new Exception("Error inesperado en GalenoServicios SetServicio ShamanClases.");
				}
				else if (!string.IsNullOrEmpty(vRdo.AlertaError))
				{
					if (vRdo.AlertaError.ToUpper().Contains("SERVICIO EXISTENTE"))
						return true;
					else
						throw new Exception($"Error inesperado en GalenoServicios SetServicio ShamanClases: {vRdo.AlertaError}.");
				}

				_logger.Info($"Finalización de almacenamiento de servicio (ID {serviceDto.Id}) en BD.");

				return vRdo.Resultado;

			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				_logger.Info($"Finalización CON ERRORES de almacenamiento de servicio (ID {serviceDto.Id}) en BD.");

			}
			return false;

		}

		public bool AnulacionEnBaseDedatos(string serviceID)
		{
			try
			{
				_logger.Info($"Anulación de servicio (ID {serviceID}) en BD...");
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				_logger.Info($"Finalización CON ERRORES de anulación de servicio (ID {serviceID}) en BD.");
			}
			return false;
		}

		private bool EstablecerReiteracion(ServiceDto serviceDto)
		{
			try
			{

				_logger.Info($"Reiteración de servicio (ID {serviceDto.Id}) en BD...");

			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				_logger.Info($"Finalización CON ERRORES de reiteración de servicio (ID {serviceDto.Id}) en BD.");
			}
			return false;
		}

		public bool ReclamoEnBaseDedatos(string serviceID)
		{
			try
			{

				ConnectionStringCache connectionStringCache = GetConnectionStringCache();

				decimal nroServicio = 0;
				string nroServicioString = "";

				try
                {
					nroServicio = Convert.ToDecimal(serviceID);
                }
				catch
                {
					nroServicioString = serviceID;
                }

				return new PamiServicios(connectionStringCache).SetReclamo(cliCod, nroServicio, nroServicioString);

			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				_logger.Info($"Finalización CON ERRORES del reclamo de servicio (ID {serviceID}) en BD.");
			}
			return false;

		}

		public DataTable GetEstadosAsignacion()
		{
			ConnectionStringCache connectionStringCache = GetConnectionStringCache();
			DataTable dt = new GalenoServicios(connectionStringCache).GetPamiEstadosAsignacionPendientes(cliCod);
			return dt;
		}

		public bool SetEstadoAsignacionEnviado(decimal pGalenoId, int pEventoId, string pWarning = "")
		{
			ConnectionStringCache connectionStringCache = GetConnectionStringCache();
			return new GalenoServicios(connectionStringCache).SetPamiEventoEnviado(pGalenoId, pEventoId, pWarning);
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
				case "Verde Teleconsulta":
					return "V";
				case "Consulta":
					return "V";
				case "Llamadas Grales":
					return "V";
				case "Amarillo":
				case "Amarillo Teleconsulta":
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
					return useSuperRojo ? "SR" : "R";
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
				case "Teleconsulta":
					return "TA";
				default:
					return "R";
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
