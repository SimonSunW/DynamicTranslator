using System.Threading.Tasks;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;
using RestSharp;

namespace DynamicTranslator.Prompt
{
	public class PromptMeanFinder
	{
		private const string ContentType = "application/json;Charset=UTF-8";
		private const string ContentTypeName = "Content-Type";

		private readonly ApplicationConfiguration _applicationConfiguration;
		private readonly IRestClient _restClient;
		private readonly PromptTranslatorConfiguration _configuration;
		private readonly PromptMeanOrganizer _meanOrganizer;

		public PromptMeanFinder(ApplicationConfiguration applicationConfiguration, IRestClient restClient, PromptTranslatorConfiguration configuration, PromptMeanOrganizer meanOrganizer)
		{
			_applicationConfiguration = applicationConfiguration;
			_restClient = restClient;
			_configuration = configuration;
			_meanOrganizer = meanOrganizer;
		}

		public async Task<TranslateResult> Find(TranslateRequest translateRequest)
		{
			var requestObject = new
			{
				dirCode = $"{translateRequest.FromLanguageExtension}-{_applicationConfiguration.ToLanguage.Extension}",
				template = _configuration.Template,
				text = translateRequest.CurrentText,
				lang = translateRequest.FromLanguageExtension,
				limit = _configuration.Limit,
				useAutoDetect = true,
				key = string.Empty,
				ts = _configuration.Ts,
				tid = string.Empty,
				IsMobile = false
			};

			IRestResponse response = await _restClient
				.Manipulate(client => { client.BaseUrl = _configuration.Url.ToUri(); }).ExecutePostTaskAsync(new RestRequest(Method.POST)
					.AddHeader(ContentTypeName, ContentType)
					.AddParameter(ContentType, requestObject.ToJsonString(false), ParameterType.RequestBody));

			string mean = string.Empty;

			if (response.Ok())
			{
				mean = await _meanOrganizer.OrganizeMean(response.Content);
			}

			return new TranslateResult(true, mean);
		}
	}
}
