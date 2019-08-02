using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Handlers;
using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace IntegracionPAMI.APIConsumer.Helpers
{
	public class ApiHelper
	{
		public static HttpClient ApiClient { get; set; }

		public static void InitializeClient()
		{
			ApiClient = HttpClientFactory.Create(new DelegatingHandler[] { new LoggingHandler(), new AuthHandler()  });
			ApiClient.BaseAddress = new Uri("http://209.13.97.251/");
			ApiClient.DefaultRequestHeaders.Accept.Clear();
			//ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}
