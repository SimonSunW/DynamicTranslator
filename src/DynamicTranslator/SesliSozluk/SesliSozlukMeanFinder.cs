using System;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;
using RestSharp;

namespace DynamicTranslator.SesliSozluk
{
	public class SesliSozlukMeanFinder
	{
		private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
		private const string AcceptEncoding = "gzip, deflate";
		private const string AcceptLanguage = "en-US,en;q=0.8,tr;q=0.6";
		private const string ContentType = "application/x-www-form-urlencoded";
		private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";

		private readonly ApplicationConfiguration _applicationConfiguration;
		private readonly IRestClient _restClient;
		private readonly SesliSozlukTranslatorConfiguration _configuration;
		private readonly SesliSozlukMeanOrganizer _meanOrganizer;

		public SesliSozlukMeanFinder(ApplicationConfiguration applicationConfiguration, IRestClient restClient, SesliSozlukTranslatorConfiguration configuration, SesliSozlukMeanOrganizer meanOrganizer)
		{
			_applicationConfiguration = applicationConfiguration;
			_restClient = restClient;
			_configuration = configuration;
			_meanOrganizer = meanOrganizer;
		}

		public async Task<TranslateResult> Find(TranslateRequest translateRequest)
		{
			string parameter = $"sl=auto&text={Uri.EscapeUriString(translateRequest.CurrentText)}&tl={_applicationConfiguration.ToLanguage.Extension}";

			IRestResponse response = await _restClient.Manipulate(client =>
			{
				client.BaseUrl = _configuration.Url.ToUri();
				client.Encoding = Encoding.UTF8;
				client.CachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, TimeSpan.FromHours(1));
			}).ExecutePostTaskAsync(
				new RestRequest(Method.POST)
					.AddHeader(Headers.AcceptLanguage, AcceptLanguage)
					.AddHeader(Headers.AcceptEncoding, AcceptEncoding)
					.AddHeader(Headers.ContentType, ContentType)
					.AddHeader(Headers.UserAgent, UserAgent)
					.AddHeader(Headers.Accept, Accept)
					.AddParameter(ContentType, parameter, ParameterType.RequestBody));

			var mean = string.Empty;

			if (response.Ok())
			{
				mean = await _meanOrganizer.OrganizeMean(response.Content);
			}

			return new TranslateResult(true, mean);
		}
	}
}
