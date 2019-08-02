using System;
using Newtonsoft.Json;

namespace IntegracionPAMI.APIConsumer.Dto
{
	public class TokenInfoDto
	{
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }
		[JsonProperty("token_type")]
		public string TokenType { get; set; }
		[JsonProperty("expires_in")]
		public int ExpiresIn { get; set; }
		public string UserName { get; set; }
		[JsonProperty(".issued")]
		public DateTime Issued { get; set; }
		[JsonProperty(".expires")]
		public DateTime Expires { get; set; }
	}
}
