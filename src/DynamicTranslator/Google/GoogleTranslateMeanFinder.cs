using System.Text;
using System.Threading.Tasks;
using System.Web;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;
using RestSharp;

namespace DynamicTranslator.Google
{
	public class GoogleTranslateMeanFinder
	{
		private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
		private const string AcceptEncoding = "gzip, deflate, sdch";
		private const string AcceptLanguage = "en-US,en;q=0.8,tr;q=0.6";

		private const string UserAgent =
			"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";

		private readonly ApplicationConfiguration _applicationConfiguration;
		private readonly GoogleTranslatorConfiguration _configuration;
		private readonly GoogleTranslateMeanOrganizer _meanOrganizer;
		private readonly IRestClient _restClient;

		public GoogleTranslateMeanFinder(ApplicationConfiguration applicationConfiguration, IRestClient restClient, GoogleTranslatorConfiguration configuration, GoogleTranslateMeanOrganizer meanOrganizer)
		{
			_applicationConfiguration = applicationConfiguration;
			_restClient = restClient;
			_configuration = configuration;
			_meanOrganizer = meanOrganizer;
		}

		protected async Task<TranslateResult> Find(TranslateRequest translateRequest)
		{
			if (!_configuration.CanSupport() || !_configuration.IsActive())
			{
				return new TranslateResult();
			}

			var uri = string.Format(
				_configuration.Url,
				_applicationConfiguration.ToLanguage.Extension,
				_applicationConfiguration.ToLanguage.Extension,
				HttpUtility.UrlEncode(translateRequest.CurrentText, Encoding.UTF8));

			var response = await _restClient.Manipulate(client =>
				{
					client.BaseUrl = uri.ToUri();
					client.Encoding = Encoding.UTF8;
				})
				.ExecuteGetTaskAsync(
					new RestRequest(Method.GET)
						.AddHeader(Headers.AcceptLanguage, AcceptLanguage)
						.AddHeader(Headers.AcceptEncoding, AcceptEncoding)
						.AddHeader(Headers.UserAgent, UserAgent)
						.AddHeader(Headers.Accept, Accept));

			string mean = string.Empty;

			if (response.Ok()) mean = await _meanOrganizer.OrganizeMean(response.Content, _applicationConfiguration.FromLanguage.Extension);

			return new TranslateResult(true, mean);
		}
	}
}