using Newtonsoft.Json;

namespace IntegracionPAMI.APIConsumer.Dto
{
	public class ErrorDto
	{
		[JsonProperty("error")]
		public string Code { get; set; }

		[JsonProperty("error_description")]
		public string Description { get; set; }
	}
}
