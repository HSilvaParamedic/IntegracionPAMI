using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using IntegracionPAMI.Services;
using IntegracionPAMI.WindowsService.SQL.Services;
using NLog;

namespace IntegracionPAMI.Test.Services
{
	/// <summary>
	/// Summary description for IntegracionPAMIManagerTest
	/// </summary>
	[TestClass]
	public class IntegracionPAMIManagerSQLTest
    {
		public IntegracionPAMIManagerSQLTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void GuardarNuevosServiciosTest()
		{
			Logger _logger = LogManager.GetCurrentClassLogger();
			try
			{
				IntegracionPAMIManager integracionPAMIManager = new IntegracionPAMIManager(new IntegracionService());
                //integracionPAMIManager.GuardarNuevosServicios();
                integracionPAMIManager.GuardarNuevosServiciosDesdeGoing();
            }
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}

			Assert.IsNotNull(false);
		}

		[TestMethod]
		public void EnviarEstadosAsignacionTest()
		{
			Logger _logger = LogManager.GetCurrentClassLogger();
			try
			{
				IntegracionPAMIManager integracionPAMIManager = new IntegracionPAMIManager(new IntegracionService());
				integracionPAMIManager.EnviarEstadosAsignacion();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, ex.Message);
			}

			Assert.IsNotNull(false);
		}
	}
}
