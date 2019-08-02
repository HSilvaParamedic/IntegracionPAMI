using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntegracionPAMI.APIConsumer.Helpers;
using IntegracionPAMI.APIConsumer.Dto;
using System.Configuration;

namespace IntegracionPAMI.Test.APIConsumer.Helpers
{
	/// <summary>
	/// Summary description for AuthHelperTest
	/// </summary>
	[TestClass]
	public class AuthHelperTest
	{
		public AuthHelperTest()
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
		public  void GetTokenInfo_ValidCredentials()
		{
			string username = ConfigurationManager.AppSettings.Get("Username");
			string password = ConfigurationManager.AppSettings.Get("Password");

			TokenInfoDto tokenInfo =  AuthHelper.GetTokenInfo().Result;

			Assert.IsNotNull(tokenInfo);
		}

		[TestMethod]
		public void GetTokenInfo_InvalidCredentials()
		{
			TokenInfoDto tokenInfo = AuthHelper.GetTokenInfo().Result;

			Assert.IsNotNull(tokenInfo);
		}

	}
}
