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
			string username = ConfigurationManager.AppSettings.Get("Username");
			string password = ConfigurationManager.AppSettings.Get("Password");

			FormUrlEncodedContent bodyContent = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("grant_type", "password"),
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("password", password)
			});

			using (HttpResponseMessage response = await ApiHelper.ApiClient.PostAsync("GeoAPI/GetToken", bodyContent))
			{
				if (response.StatusCode != System.Net.HttpStatusCode.OK)
				{
					//TODO: Loguer statud Code
					return null;
				}

				return JsonConvert.DeserializeObject<TokenInfoDto>(await response.Content.ReadAsStringAsync());
			}
		}
	}
}
