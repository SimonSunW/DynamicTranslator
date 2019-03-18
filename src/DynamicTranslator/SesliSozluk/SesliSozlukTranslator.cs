using System;
using System.Linq;
using System.Net.Cache;
using System.Text;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;
using HtmlAgilityPack;
using RestSharp;

namespace DynamicTranslator.SesliSozluk
{
	public class SesliSozlukTranslator
	{
		private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
		private const string AcceptEncoding = "gzip, deflate";
		private const string AcceptLanguage = "en-US,en;q=0.8,tr;q=0.6";
		private const string ContentType = "application/x-www-form-urlencoded";
		private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";

		public SesliSozlukTranslator(DynamicTranslatorConfiguration configurations)
		{
			var cfg = new SesliSozlukTranslatorConfiguration(configurations.ActiveTranslatorConfiguration,
				configurations.ApplicationConfiguration)
			{
				Url = "http://www.seslisozluk.net/c%C3%BCmle-%C3%A7eviri/",
				SupportedLanguages = LanguageMapping.SesliSozluk.ToLanguages()
			};

			configurations.SesliSozlukTranslatorConfiguration = cfg;

			configurations.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.SesliSozluk, async (request, token) =>
			{
				string parameter = $"sl=auto&text={Uri.EscapeUriString(request.CurrentText)}&tl={configurations.ApplicationConfiguration.ToLanguage.Extension}";

				IRestResponse response = await configurations.ClientFactory().With(client =>
				{
					client.BaseUrl = cfg.Url.ToUri();
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
					mean = OrganizeMean(response.Content);
				}

				return new TranslateResult(true, mean);
			});
		}

		public string OrganizeMean(string text)
		{
			var output = new StringBuilder();

			var document = new HtmlDocument();
			document.LoadHtml(text);

			(from x in document.DocumentNode.Descendants()
			 where x.Name == "pre"
			 from y in x.Descendants()
			 where y.Name == "ol"
			 from z in y.Descendants()
			 where z.Name == "li"
			 select z.InnerHtml)
				.AsParallel()
				.ToList()
				.ForEach(mean => output.AppendLine(mean));

			if (string.IsNullOrEmpty(output.ToString()))
			{
				(from x in document.DocumentNode.Descendants()
				 where x.Name == "pre"
				 from y in x.Descendants()
				 where y.Name == "span"
				 select y.InnerHtml)
					.AsParallel()
					.ToList()
					.ForEach(mean => output.AppendLine(mean.StripTagsCharArray()));
			}

			return output.ToString();
		}
	}
}
