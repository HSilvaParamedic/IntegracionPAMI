using IntegracionPAMI.APIConsumer.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;

using System.Text;
using System.Threading.Tasks;

namespace IntegracionPAMI.APIConsumer.Helpers
{
	public class AuthHelper
	{
		private static TokenInfoDto tokenInfo;
		private static string username = ConfigurationManager.AppSettings.Get("API_Username");
		private static string password = ConfigurationManager.AppSettings.Get("API_Password");
		private static string APIEndpointGetToken = ConfigurationManager.AppSettings.Get("API_Endpoint_GetToken");

		public static async Task<TokenInfoDto> GetTokenInfo()
		{
			if (tokenInfo == null || tokenInfo.Expires <= DateTime.UtcNow)
			{
				tokenInfo = await GetTokenInfoFromApi();
			}
			return tokenInfo;
		}

		private static async Task<TokenInfoDto> GetTokenInfoFromApi()
		{
			FormUrlEncodedContent bodyContent = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("grant_type", "password"),
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("password", password)
			});

			using (HttpResponseMessage response = await ApiHelper.ApiClient.PostAsync(APIEndpointGetToken, bodyContent))
			{
				return JsonConvert.DeserializeObject<TokenInfoDto>(await response.Content.ReadAsStringAsync());
			}
		}
	}
}
