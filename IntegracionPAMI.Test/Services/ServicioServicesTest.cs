﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntegracionPAMI.APIConsumer.Services;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;

namespace IntegracionPAMI.Test.Services
{
	/// <summary>
	/// Summary description for ServicioServicesTest
	/// </summary>
	[TestClass]
	public class ServicioServicesTest
	{
		public ServicioServicesTest()
		{
			ApiHelper.InitializeClient();
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
		public void GetNotificaciones()
		{
			IEnumerable<NotificationDto> notificaciones = new ServicioServices().GetNuevasNotifications().Result;

			Assert.IsNotNull(notificaciones);
		}

		[TestMethod]
		public void GetServicios()
		{
			ServiceDto servicio = new ServicioServices().GetServicio("2000000001").Result;

			Assert.IsNotNull(servicio);
		}
	}
}
