using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Newtonsoft.Json;
using IntegracionPAMI.APIConsumer.Dto;

namespace IntegracionPAMI.APIConsumer.Handlers
{
	public class LoggingHandler : DelegatingHandler
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string error = "";
			try
			{
				var response = await base.SendAsync(request, cancellationToken);

				if(response.StatusCode != System.Net.HttpStatusCode.OK)
				{
					error = $"API error - HttpCode: {(int)response.StatusCode}, HttpDescription: {System.Net.HttpStatusCode.OK.ToString()}";
				}

				if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
				{
					ErrorDto erroDto = JsonConvert.DeserializeObject<ErrorDto>(await response.Content.ReadAsStringAsync());
					_logger.Error($"{error} - ( Code: {erroDto.Code}, Description: {erroDto.Description})");
				}
				return response;
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed to get response: {ex}");
				throw;
			}
		}
	}
}
