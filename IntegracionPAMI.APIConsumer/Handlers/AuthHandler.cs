using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IntegracionPAMI.APIConsumer.Dto;
using IntegracionPAMI.APIConsumer.Helpers;

namespace IntegracionPAMI.APIConsumer.Handlers
{
	public class AuthHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request.RequestUri.ToString().ToUpper().Contains("/API/"))
			{
				TokenInfoDto tokenInfo = AuthHelper.GetTokenInfo().Result;

				request.Headers.Add("Authorization", $"{tokenInfo.TokenType} {tokenInfo.AccessToken}");
			}

			return await base.SendAsync(request, cancellationToken);
		}
	}
}
