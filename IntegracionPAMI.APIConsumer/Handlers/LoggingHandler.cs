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

				string responseMessage = await response.Content.ReadAsStringAsync();

				if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
				{
					if(responseMessage.Contains("error_description"))
					{
						ErrorDto erroDto = JsonConvert.DeserializeObject<ErrorDto>(responseMessage);
						error = $"Authentication Error - ( Code: {erroDto.Code}, Description: {erroDto.Description})";
					}
					else
					{
						error = responseMessage;
					}
					throw new Exception(error);
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					if (!responseMessage.Contains("access_token"))
					{
						SuccessState successState = JsonConvert.DeserializeObject<SuccessState>(responseMessage);

						if (!successState.IsSuccess)
						{
							error = successState.Message;
							throw new Exception(error);
						}
					}
				}

				return response;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Failed to get response");
				throw;
			}
		}
	}
}
