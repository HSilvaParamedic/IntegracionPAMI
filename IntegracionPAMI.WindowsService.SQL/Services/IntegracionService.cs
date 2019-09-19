using System;
using System.Configuration;
using IntegracionPAMI.Services;
using IntegracionPAMI.APIConsumer.Dto;
using NLog;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using ShamanExpressDLL;

namespace IntegracionPAMI.WindowsService.SQL.Services
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

				if (this.SQLConnect())
				{

					modFechas.InitDateVars();
					modNumeros.InitSepDecimal();

					conConfiguracionesRegionales objConfigEquivalencias = new conConfiguracionesRegionales();

					conPreIncidentes preInc = new conPreIncidentes();
					conClientes objCliente = new conClientes();

					preInc.CleanProperties(preInc);

                    preInc.Telefono = serviceDto.phoneNumber;
                    preInc.ClienteId.SetObjectId(objCliente.GetIDByAbreviaturaId(cliCod).ToString());
					preInc.NroServicio = serviceDto.Id;
					preInc.Domicilio.dmCalle = serviceDto.Address.StreetName + ' ' + serviceDto.Address.HouseNumber + ' ' + serviceDto.Address.FloorApt;
                    preInc.Domicilio.dmEntreCalle1 = serviceDto.Address.BetweenStreet1;
					preInc.Domicilio.dmEntreCalle2 = serviceDto.Address.BetweenStreet2;

                    preInc.Domicilio.dmLatitud = serviceDto.Address.LatLng.Latitude;
                    preInc.Domicilio.dmLongitud = serviceDto.Address.LatLng.Longitude;

                    preInc.errLocalidad = serviceDto.Address.City;

					preInc.NroAfiliado = serviceDto.BeneficiaryID;
					preInc.Paciente = serviceDto.BeneficiaryName;
					if (serviceDto.Gender.Length > 1) { preInc.Sexo = serviceDto.Gender.Substring(0, 1); }
					preInc.Edad = serviceDto.Age.HasValue ? Convert.ToDecimal(MapEdad(serviceDto.Age.Value, serviceDto.AgeUnit)) : 0;
					preInc.Sintomas = serviceDto.Triage.Last().Reason;

                    if (preInc.Sintomas.Length > 100) { preInc.Sintomas = preInc.Sintomas.Substring(0, 100); }
					preInc.errGradoOperativo = serviceDto.Classification;

					//// Busco equivalencias

					long cnfId = objConfigEquivalencias.GetIDByClienteId(Convert.ToInt64(preInc.ClienteId.GetObjectId()));
					preInc.GradoOperativoId.SetObjectId(getGradoOperativoId(cnfId, MapGrado(serviceDto.Classification)).ToString());
					preInc.LocalidadId.SetObjectId(getLocalidadId(cnfId, serviceDto.Address.City).ToString());

					preInc.MetodoIngresoId = modDeclares.preIncidenteOrigen.RestServicePAMI;

                    //// Observaciones
                    preInc.Observaciones = serviceDto.OriginComments;

                    AttributeDto atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Tratamiento preferencial");
                    if (atr != null && atr.Value.Length > 2)
                    {
                        preInc.Observaciones = preInc.Observaciones + " - Tratamiento Preferencial: " + atr.Value;
                    }

                    atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Módulo de internación");
                    if (atr != null && atr.Value.Length > 2)
                    {
                        preInc.Observaciones = preInc.Observaciones + " Módulo de internación: " + atr.Value;
                    }

                    //// Documento
                    atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Número de documento");
                    if (atr != null)
                    {
                        preInc.NroDocumento = atr.Value;
                    }

                    preInc.FecHorServicio = serviceDto.TimeRequested;

					bool savOk = preInc.Salvar(preInc);

					preInc = null;
					objConfigEquivalencias = null;
					objCliente = null;

					return savOk;

				}

			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}
			return false;

		}

		private long getGradoOperativoId(long cnfId, string grado)
		{

			long gdo = 0;

			if (cnfId > 0)
			{

				conConfiguracionesRegionalesReglas objEquivalencias = new conConfiguracionesRegionalesReglas();
				string eqVal = objEquivalencias.GetValor1(cnfId, grado, 103);
				if (eqVal != "") { gdo = Convert.ToInt64(eqVal); }

			}

			if (gdo == 0)
			{

				conGradosOperativos objGrados = new conGradosOperativos();
				gdo = objGrados.GetIDByAbreviaturaId(grado);

				if (gdo == 0)
				{
					gdo = objGrados.GetIDByDescripcion(grado);
				}

			}

			return gdo;
		}

		private long getLocalidadId(long cnfId, string localidad)
		{
			try
			{
				long loc = 0;

				if (cnfId > 0)
				{

					conConfiguracionesRegionalesReglas objEquivalencias = new conConfiguracionesRegionalesReglas();
					string devStr = objEquivalencias.GetValor1(cnfId, localidad, 104);
					if (!string.IsNullOrEmpty(devStr))
					{
						loc = Convert.ToInt64(devStr);
					}

				}

				if (loc == 0)
				{

					conLocalidades objLocalidades = new conLocalidades();
					loc = objLocalidades.GetIDByAbreviaturaId(localidad);

					if (loc == 0)
					{
						loc = objLocalidades.GetIDByDescripcion(localidad);
					}

					if (loc == 0)
					{
						conLocalidadesSinonimos objSinonimo = new conLocalidadesSinonimos();
						loc = objSinonimo.GetLocalidadIdBySinonimo(localidad);
						objSinonimo = null;
					}

				}

				return loc;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				return 0;
			}
		}

		private long getDiagnosticoId(long cnfId, string abreviaturaId, string descripcion)
		{
			try
			{
				long dig = 0;

				if (cnfId > 0)
				{
					conConfiguracionesRegionalesReglas objEquivalencias = new conConfiguracionesRegionalesReglas();
					string devStr = objEquivalencias.GetValor1(cnfId, descripcion, 101);
					if (!string.IsNullOrEmpty(devStr))
					{
						dig = Convert.ToInt64(devStr);
					}
				}

				if (dig == 0)
				{
					conDiagnosticos objDiagnosticos = new conDiagnosticos();
					dig = objDiagnosticos.GetIDByAbreviaturaId(abreviaturaId);

					if (dig == 0)
					{
						dig = objDiagnosticos.GetIDByDescripcion(descripcion);
					}

				}

				return dig;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				return 0;
			}

		}

		private long getMotivoNoRealizacionId(long cnfId, string abreviaturaId, string descripcion)
		{
			try
			{
				long mot = 0;

				if (cnfId > 0)
				{

					conConfiguracionesRegionalesReglas objEquivalencias = new conConfiguracionesRegionalesReglas();
					string devStr = objEquivalencias.GetValor1(cnfId, descripcion, 105);
					if (!string.IsNullOrEmpty(devStr))
					{
						mot = Convert.ToInt64(devStr);
					}
				}

				if (mot == 0)
				{

					conMotivosNoRealizacion objMotivos = new conMotivosNoRealizacion();
					mot = objMotivos.GetIDByAbreviaturaId(abreviaturaId);

					if (mot == 0)
					{
						mot = objMotivos.GetIDByDescripcion(descripcion);
					}

				}

				return mot;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				return 0;
			}
		}

		public DataTable GetEstadosAsignacion()
		{
			if (this.SQLConnect())
			{
				conPreIncidentes preMsg = new conPreIncidentes();
				return preMsg.GetPamiEventos(cliCod);
			}
			return null;
		}

		public bool SetEstadoAsignacionEnviado(decimal pGalenoId, int pEventoId)
		{
			if (this.SQLConnect())
			{
				conPreIncidentes preInc = new conPreIncidentes();
				typPreIncidentesMensajes preMsg = new typPreIncidentesMensajes();

				preMsg.CleanProperties(preMsg);
				preMsg.PreIncidenteId.SetObjectId(pGalenoId.ToString());
				preMsg.MensajeId = (modDeclares.preIncidenteMensaje)pEventoId;

				if (preMsg.Salvar(preMsg))
				{
					return true;
				}

			}
			return false;
		}

		private bool SQLConnect()
		{
			if (modDatabase.cnnsNET.Count > 0)
			{
				if (modDatabase.cnnsNET[modDeclares.cnnDefault].State != ConnectionState.Open)
				{
					modDatabase.cnnsNET.Remove(modDeclares.cnnDefault);
				}
				else
				{
					return true;
				}
			}

			/// Conecto!

			StartUp objStartUp = new StartUp();

			string[] connectionStringSQLValues = ConfigurationManager.AppSettings.Get("ConnectionStringSQL_Values").Split('|');

			string cnnStr = string.Format("Data Source={0};multipleactiveresultsets=true;Initial Catalog={1};User Id={2};Password={3};", connectionStringSQLValues[0], connectionStringSQLValues[1], connectionStringSQLValues[2], connectionStringSQLValues[3]);

			if (objStartUp.AbrirConexion("Default", false, cnnStr))
			{
				return true;
			}

			return false;
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
