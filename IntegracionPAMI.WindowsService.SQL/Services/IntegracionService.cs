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
		private string motAnulacion = ConfigurationManager.AppSettings.Get("ServicioMap_MotivoAnulacion");
		private int nroAut = int.Parse(ConfigurationManager.AppSettings.Get("ServicioMap_pNroAut"));
		private string digObito = ConfigurationManager.AppSettings.Get("ServicioMap_Obito");
		private string digIncontactable = ConfigurationManager.AppSettings.Get("ServicioMap_Incontactable");
		private string regUsuario = ConfigurationManager.AppSettings.Get("ServicioMap_Usuario");
		private bool useLocalidadesDesc = int.Parse(ConfigurationManager.AppSettings.Get("ServicioMap_UseLocalidadesDesc")) == 1 ? true : false;

		public bool AlmacenarEnBaseDedatos(string strNotificationType, ServiceDto serviceDto)
		{
			try
			{

				if (this.SQLConnect())
				{

					modFechas.InitDateVars();
					modNumeros.InitSepDecimal();
					modDeclares.shamanConfig = new conConfiguracion();
					modDeclares.shamanConfig.UpConfig();

					switch (strNotificationType)
                    {
						case "Nuevo":
							return this.CrearNuevo(serviceDto);
						case "Reiteración":
							return this.EstablecerReiteracion(serviceDto);
					}

				}

				_logger.Info($"Finalización de almacenamiento de servicio (ID {serviceDto.Id}) en BD ({serviceDto.Classification})");

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

				conConfiguracionesRegionales objConfigEquivalencias = new conConfiguracionesRegionales();

				conPreIncidentes preInc = new conPreIncidentes();
				conGradosOperativos objGrados = new conGradosOperativos();

				//// Existencia

				if (preInc.GetIDByClienteNroServicio(this.getClienteId(), serviceDto.Id) > 0)
				{
					_logger.Info($"Finalización de almacenamiento de servicio (ID {serviceDto.Id}) en BD ({serviceDto.Classification})");
					return true;
				}

				preInc.CleanProperties(preInc);

				preInc.Telefono = serviceDto.phoneNumber;
				preInc.ClienteId.SetObjectId(this.getClienteId().ToString());
				preInc.NroServicio = serviceDto.Id;
				preInc.Domicilio.dmCalle = serviceDto.Address.StreetName + ' ' + serviceDto.Address.HouseNumber + ' ' + serviceDto.Address.FloorApt;
				preInc.Domicilio.dmEntreCalle1 = serviceDto.Address.BetweenStreet1;
				preInc.Domicilio.dmEntreCalle2 = serviceDto.Address.BetweenStreet2;
				preInc.Domicilio.dmLatitud = serviceDto.Address.LatLng.Latitude;
				preInc.Domicilio.dmLongitud = serviceDto.Address.LatLng.Longitude;

				if (serviceDto.Address.Neighborhood != "")
                {
					preInc.errLocalidad = serviceDto.Address.Neighborhood;
					preInc.Domicilio.dmReferencia = serviceDto.Address.City;
				}
				else
                {
					preInc.errLocalidad = serviceDto.Address.City;
				}

				if (serviceDto.Address.PointOfReference != "")
				{
					preInc.Domicilio.dmReferencia = preInc.Domicilio.dmReferencia != "" ? string.Format("{0} // {1}", preInc.Domicilio.dmReferencia, serviceDto.Address.PointOfReference) : serviceDto.Address.PointOfReference;
				}

				preInc.NroAfiliado = serviceDto.BeneficiaryID;
				preInc.Paciente = serviceDto.BeneficiaryName;
				if (serviceDto.Gender.Length > 1) { preInc.Sexo = serviceDto.Gender.Substring(0, 1); }
				preInc.Edad = serviceDto.Age.HasValue ? Convert.ToDecimal(MapEdad(serviceDto.Age.Value, serviceDto.AgeUnit)) : 0;

				try
				{
					preInc.Sintomas = serviceDto.Triage.Last().Reason;
				}
				catch
				{
					preInc.Sintomas = "";
				}

				if (preInc.Sintomas.Length > 100) { preInc.Sintomas = preInc.Sintomas.Substring(0, 100); }
				preInc.errGradoOperativo = serviceDto.Classification;

				//// Busco equivalencias

				long cnfId = objConfigEquivalencias.GetIDByClienteId(Convert.ToInt64(preInc.ClienteId.GetObjectId()));
				int metodoIngreso = 0;

				conConfiguracionesRegionalesMetodos objMetodos = new conConfiguracionesRegionalesMetodos();
				metodoIngreso = objMetodos.GetMetodoIngreso(cnfId, 103);

				//// Grado

				long gdoId = this.getGradoOperativoId(cnfId, serviceDto.Classification, metodoIngreso);
				if (gdoId == 0)
				{
					gdoId = this.getGradoOperativoId(cnfId, MapGrado(serviceDto.Classification), metodoIngreso);
				}
				preInc.GradoOperativoId.SetObjectId(gdoId.ToString());

				//// Grados Operativos

				if (objGrados.Abrir(gdoId.ToString()))
				{
					if (objGrados.ClasificacionId == modDeclares.gdoClasificacion.gdoTraslado)
					{

						preInc.DERFecHorServicio = serviceDto.TimeRequested.AddMinutes(1);

						conBasesOperativas objBases = new conBasesOperativas();

						if (objBases.Abrir(objBases.GetDefault().ToString()))
						{
							preInc.DERDomicilio.DERdmCalle = objBases.Domicilio.dmCalle;
							preInc.DERDomicilio.DERdmAltura = objBases.Domicilio.dmAltura;
							preInc.DERLocalidadId.SetObjectId(objBases.LocalidadId.ID.ToString());
						}

						objBases = null;
					}
				}

				preInc.LocalidadId.SetObjectId(this.getLocalidadId(cnfId, preInc.errLocalidad, metodoIngreso).ToString());

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

				if (savOk)
                {
					_logger.Info($"Finalización de almacenamiento de servicio (ID {serviceDto.Id}) en BD ({serviceDto.Classification})");
				}
				else
                {
					_logger.Info($"Finalización de almacenamiento de servicio (ID {serviceDto.Id}) en BD ({serviceDto.Classification}) con error: {preInc.MyLastExec.ErrorDescription}");
				}

				return savOk;

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

				bool devRdo = false;

				if (this.SQLConnect())
                {

					conConfiguracion shamanConfig = new conConfiguracion();

					if (shamanConfig.UpConfig())
                    {

						if (shamanConfig.opeAnuladoPantalla == 1)
                        {

							conPreIncidentes objPreIncidente = new conPreIncidentes();

							if (objPreIncidente.Abrir(objPreIncidente.GetIDByClienteNroServicio(this.getClienteId(), serviceID).ToString()))
							{

								if (objPreIncidente.flgStatus == 1)
								{

									if (objPreIncidente.IncidenteId.ID > 0)
									{

										conIncidentesViajes objViaje = new conIncidentesViajes();

										long viajeId = objViaje.GetIDByIndex(objPreIncidente.IncidenteId.ID);

										DevValidacion valAnulacion = objViaje.CanAnular(viajeId, modDeclares.nodAccess.sEscritura, true);

										if (valAnulacion.Resultado == modDeclares.devValidacionResultado.rdoOK)
										{
											devRdo = objViaje.Anular(viajeId, this.getMotivoAnulacionId(), "ANULADO POR MESA OPERATIVA PAMI");
										}
										else
                                        {
											if (valAnulacion.Mensaje.Contains("ya se encontraba anulado"))
                                            {
												devRdo = true;
											}
											else
                                            {
												_logger.Info($"Anulación de servicio (ID {serviceID}) : {valAnulacion.Mensaje}...");
											}
                                        }

										objViaje = null;

									}

								}

								else

								{

									if (objPreIncidente.flgStatus != 2)
									{

										objPreIncidente.flgStatus = 2;
										devRdo = objPreIncidente.Salvar(objPreIncidente);

									}

								}

							}

							objPreIncidente = null;

						}

					}

				}

				_logger.Info($"Finalización de anulación de servicio (ID {serviceID})");

				return devRdo;

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

				conPreIncidentes preInc = new conPreIncidentes();

				if (preInc.Abrir(preInc.GetIDByClienteNroServicio(this.getClienteId(), serviceDto.Id).ToString()))
				{

					string diferences = "";
					string difLocalidad = "";

					if (preInc.Telefono != serviceDto.phoneNumber)
                    {
						preInc.Telefono = serviceDto.phoneNumber;
						diferences = diferences == "" ? "Teléfono" : diferences + ", Teléfono";
					}

					if (preInc.Domicilio.dmCalle != serviceDto.Address.StreetName + ' ' + serviceDto.Address.HouseNumber + ' ' + serviceDto.Address.FloorApt)
                    {
						preInc.Domicilio.dmCalle = serviceDto.Address.StreetName + ' ' + serviceDto.Address.HouseNumber + ' ' + serviceDto.Address.FloorApt;
						preInc.Domicilio.dmEntreCalle1 = serviceDto.Address.BetweenStreet1;
						preInc.Domicilio.dmEntreCalle2 = serviceDto.Address.BetweenStreet2;
						preInc.Domicilio.dmLatitud = serviceDto.Address.LatLng.Latitude;
						preInc.Domicilio.dmLongitud = serviceDto.Address.LatLng.Longitude;
						diferences = diferences == "" ? "Domicilio" : diferences + ", Domicilio";
					}

					if (preInc.errLocalidad != serviceDto.Address.City)
					{
						difLocalidad = string.Format("Localidad Nueva: {0} - Localidad Anterior: {1}", serviceDto.Address.City, preInc.errLocalidad);
						preInc.errLocalidad = serviceDto.Address.City;
						diferences = diferences == "" ? difLocalidad : diferences + ", " + difLocalidad;
					}

					if ((preInc.NroAfiliado != serviceDto.BeneficiaryID) || (preInc.Paciente != serviceDto.BeneficiaryName))
					{
						preInc.NroAfiliado = serviceDto.BeneficiaryID;
						preInc.Paciente = serviceDto.BeneficiaryName;
						diferences = diferences == "" ? "Paciente" : diferences + ", Paciente";
					}

					if (serviceDto.Gender.Length > 1) { preInc.Sexo = serviceDto.Gender.Substring(0, 1); }
					preInc.Edad = serviceDto.Age.HasValue ? Convert.ToDecimal(MapEdad(serviceDto.Age.Value, serviceDto.AgeUnit)) : 0;

					try
					{
						if (!preInc.Sintomas.Contains(serviceDto.Triage.Last().Reason))
						{
							preInc.Sintomas = serviceDto.Triage.Last().Reason;
							if (preInc.Sintomas.Length > 100) { preInc.Sintomas = preInc.Sintomas.Substring(0, 100); }
							diferences = diferences == "" ? "Síntomas" : diferences + ", Síntomas";
						}

					}
					catch
					{
					}

					if (preInc.errGradoOperativo != serviceDto.Classification)
					{
						preInc.errGradoOperativo = serviceDto.Classification;
						diferences = diferences == "" ? "Grado Operativo" : diferences + ", Grado Operativo";
					}
					
					string obs = serviceDto.OriginComments;

					AttributeDto atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Tratamiento preferencial");
					if (atr != null && atr.Value.Length > 2)
					{
						obs += " - Tratamiento Preferencial: " + atr.Value;
					}

					atr = serviceDto.Attributes.SingleOrDefault(a => a.Name == "Módulo de internación");
					if (atr != null && atr.Value.Length > 2)
					{
						obs += " Módulo de internación: " + atr.Value;
					}

					if (preInc.Observaciones != obs)
					{
						preInc.Observaciones = obs;
						diferences = diferences == "" ? "Observaciones" : diferences + ", Observaciones";
					}

					_logger.Info($"Diferencias encontradas en {serviceDto.Id}: {diferences}");

					bool savOk = preInc.Salvar(preInc);

					if ((savOk) && (diferences !=""))
					{
						if (preInc.IncidenteId.ID > 0)
                        {
							savOk = this.setIncidenteObservaciones(serviceDto.Id, preInc.IncidenteId.ID, modDeclares.obsIncidentes.Observaciones, "PAMI Portal reitera los valores de " + diferences);

							if (savOk)
                            {

								typIncidentes oIncidente = preInc.IncidenteId;

								if (diferences.Contains("Teléfono"))
								{
									oIncidente.Telefono = preInc.Telefono;
								}

								if (diferences.Contains("Paciente"))
								{
									oIncidente.NroAfiliado = preInc.NroAfiliado;
									oIncidente.Paciente = preInc.Paciente;
								}

								oIncidente.Sexo = preInc.Sexo;
								oIncidente.Edad = preInc.Edad;

								if (diferences.Contains("Síntomas"))
								{
									oIncidente.Sintomas = preInc.Sintomas;
								}

								if (diferences.Contains("Grado"))
                                {

									//// Busco equivalencias

									conConfiguracionesRegionales objRegional = new conConfiguracionesRegionales();
									conConfiguracionesRegionales objConfigEquivalencias = new conConfiguracionesRegionales();

									long cnfId = objConfigEquivalencias.GetIDByClienteId(Convert.ToInt64(preInc.ClienteId.GetObjectId()));
									int metodoIngreso = 0;

									conConfiguracionesRegionalesMetodos objMetodos = new conConfiguracionesRegionalesMetodos();
									metodoIngreso = objMetodos.GetMetodoIngreso(cnfId, 103);

									long gdoId = this.getGradoOperativoId(cnfId, serviceDto.Classification, metodoIngreso);

									if (gdoId == 0)
									{
										gdoId = this.getGradoOperativoId(cnfId, MapGrado(serviceDto.Classification), metodoIngreso);
									}

									if (gdoId > 0)
                                    {
										oIncidente.GradoOperativoId.SetObjectId(gdoId.ToString());
									}

									objRegional = null;
									objConfigEquivalencias = null;

								}

								oIncidente.Aviso = oIncidente.Aviso == "" ? diferences : oIncidente.Aviso + " // " + diferences;

								savOk = oIncidente.Salvar(oIncidente);

								if ((savOk) && (diferences.Contains("Domicilio")))
                                {

									conIncidentesDomicilios oDomicilio = new conIncidentesDomicilios();
									
									if (oDomicilio.Abrir(oDomicilio.GetIDByIndex(oIncidente.ID).ToString()))
									{

										oDomicilio.Domicilio.dmCalle = preInc.Domicilio.dmCalle;
										oDomicilio.Domicilio.dmEntreCalle1 = preInc.Domicilio.dmEntreCalle1;
										oDomicilio.Domicilio.dmEntreCalle2 = preInc.Domicilio.dmEntreCalle2;
										oDomicilio.Domicilio.dmLatitud = preInc.Domicilio.dmLatitud;
										oDomicilio.Domicilio.dmLongitud = preInc.Domicilio.dmLongitud;

									}

									savOk = oDomicilio.Salvar(oDomicilio);

									if (!savOk)
									{
										_logger.Info($"Error al regrabrar domicilio en {serviceDto.Id} ({oIncidente.MyLastExec.ErrorDescription})");
									}

									oDomicilio = null;

								}
								else
                                {
									if (!savOk)
                                    {
										_logger.Info($"Error al regrabrar incidente en {serviceDto.Id} ({oIncidente.MyLastExec.ErrorDescription})");
									}
								}

							}

						}
                    }
					else
                    {
						if (!savOk)
                        {
							_logger.Info($"Error al regrabrar preincidente en {serviceDto.Id} ({preInc.MyLastExec.ErrorDescription})");
						}
                    }

					preInc = null;

					if (savOk)
                    {
						_logger.Info($"Finalización de reiteración de servicio (ID {serviceDto.Id}) en BD ({serviceDto.Classification})");
					}

					return savOk;

				}

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

				bool devRdo = false;

				conPreIncidentes objPreIncidente = new conPreIncidentes();

				if (objPreIncidente.Abrir(objPreIncidente.GetIDByClienteNroServicio(this.getClienteId(), serviceID).ToString()))
				{

					devRdo = this.setIncidenteObservaciones(serviceID, objPreIncidente.IncidenteId.ID, modDeclares.obsIncidentes.Reclamos, "Portal PAMI informa Reclamo");

				}

				objPreIncidente = null;

				return devRdo;

			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				_logger.Info($"Finalización CON ERRORES del reclamo de servicio (ID {serviceID}) en BD.");
			}
			return false;

		}

		private bool setIncidenteObservaciones(string serviceID, long incidenteId, modDeclares.obsIncidentes tipoObservacion, string observaciones)
        {
			try
			{

				_logger.Info($"Observaciones de incidente (ID {serviceID}) de tipo {tipoObservacion}");

				bool devRdo = false;

				if (incidenteId > 0)
				{

					conIncidentesObservaciones oObs = new conIncidentesObservaciones();
					conTiposObservacionesIncidentes oTip = new conTiposObservacionesIncidentes();
					conUsuarios oUsr = new conUsuarios();

					if (oTip.Abrir(oTip.GetIdByClasificacionId(tipoObservacion).ToString()))
					{

						oObs.CleanProperties(oObs);

						oObs.IncidenteId.SetObjectId(incidenteId.ToString());

						oObs.flgReclamo = (int)oTip.ClasificacionId;

						oObs.TipoObservacionIncidenteId.SetObjectId(oTip.ID.ToString());

						oObs.Observaciones = observaciones;

						oObs.regUsuarioId.SetObjectId(oUsr.GetIDByIndex(regUsuario).ToString());

						if (oObs.Salvar(oObs, true, false))
						{
							if (oObs.SetByIncidenteId(oObs.IncidenteId.ID))
							{
								devRdo = true;
							}
							else
                            {
								_logger.Info($"Error al agregar observaciones generales de servicio (ID {serviceID}) - ({oObs.MyLastExec.ErrorDescription})");
							}
						}

						else
                        {
							_logger.Info($"Error al agregar observaciones de servicio (ID {serviceID}) - ({oObs.MyLastExec.ErrorDescription})");
						}

					}

					oObs = null;
					oTip = null;

				}

				if (devRdo)
                {
					_logger.Info($"Finalización de observaciones de servicio (ID {serviceID}) en BD");
				}

				return devRdo;

			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
				_logger.Info($"Finalización CON ERRORES de observaciones de servicio (ID {serviceID}) en BD.");
			}
			return false;

		}

		private long getClienteId()
		{
			try
			{
				return new conClientes().GetIDByAbreviaturaId(cliCod);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}

			return 0;
		}

		private long getMotivoAnulacionId()
		{
			try
			{
				return new conMotivosNoRealizacion().GetIDByAbreviaturaId(motAnulacion);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}

			return 0;
		}

		private long getGradoOperativoId(long cnfId, string grado, int metodoIngreso = 0)
		{

			long gdo = 0;

			grado = grado.Trim();

			if (cnfId > 0)
			{

				conConfiguracionesRegionalesReglas objEquivalencias = new conConfiguracionesRegionalesReglas();
				string eqVal;

				if (metodoIngreso == 0)
                {
					eqVal = objEquivalencias.GetValor1(cnfId, grado, 103);
					if (eqVal != "") { gdo = Convert.ToInt64(eqVal); }
				}
				else
                {
					eqVal = objEquivalencias.GetValor2(cnfId, grado, 103);
					if (eqVal != "") { gdo = Convert.ToInt64(eqVal); }
				}

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

		private long getLocalidadId(long cnfId, string localidad, int metodoIngreso = 0)
		{
			try
			{
				long loc = 0;

				if (cnfId > 0)
				{

					conConfiguracionesRegionalesReglas objEquivalencias = new conConfiguracionesRegionalesReglas();

					string devStr;

					if (metodoIngreso == 0)
                    {
						devStr = objEquivalencias.GetValor1(cnfId, localidad, 104);
					}
					else
					{
						devStr = objEquivalencias.GetValor2(cnfId, localidad, 104);
					}

					if (!string.IsNullOrEmpty(devStr))
					{
						loc = Convert.ToInt64(devStr);
					}

				}

				if (loc == 0)
				{

					conLocalidades objLocalidades = new conLocalidades();
					loc = objLocalidades.GetIDByAbreviaturaId(localidad);

                    if ((loc == 0) && (useLocalidadesDesc))
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

		public DataTable GetEstadosAsignacion()
		{
			if (this.SQLConnect())
			{
                modFechas.InitDateVars();
				conPreIncidentes preMsg = new conPreIncidentes();
				return preMsg.GetPamiEventos(cliCod, digObito, digIncontactable);
			}
			return null;
		}

		public bool SetEstadoAsignacionEnviado(decimal pGalenoId, int pEventoId, string pWarning = "")
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

			string cnnStr = this.getConnectionString();

			if (objStartUp.AbrirConexion("Default", false, cnnStr))
			{
				if (modDeclares.shamanConfig == null)
                {
					modDeclares.shamanConfig = new conConfiguracion();
					modDeclares.shamanConfig.UpConfig();
				}
				return true;
			}

			return false;
		}

		private string getConnectionString()

        {

			string[] connectionStringSQLValues = ConfigurationManager.AppSettings.Get("ConnectionStringSQL_Values").Split('|');

			return string.Format("Data Source={0};multipleactiveresultsets=true;Initial Catalog={1};User Id={2};Password={3};", connectionStringSQLValues[0], connectionStringSQLValues[1], connectionStringSQLValues[2], connectionStringSQLValues[3]);

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
