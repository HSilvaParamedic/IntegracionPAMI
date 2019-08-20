using System;
using System.Net.Http;
using System.Configuration;
using System.Net.Http.Headers;
using IntegracionPAMI.APIConsumer.Handlers;

namespace IntegracionPAMI.APIConsumer.Helpers
{
	public class ApiHelper
	{
		public static HttpClient ApiClient { get; set; }

		public static void InitializeClient()
		{
			ApiClient = HttpClientFactory.Create(new DelegatingHandler[] { new LoggingHandler(), new AuthHandler()  });
			ApiClient.BaseAddress = new Uri(ConfigurationManager.AppSettings.Get("API_Host"));
			ApiClient.DefaultRequestHeaders.Accept.Clear();
			ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}
