using System;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;
using DynamicTranslator.LanguageManagement;
using DynamicTranslator.Model;
using HtmlAgilityPack;
using RestSharp;

namespace DynamicTranslator.Tureng
{
	public class DynamicTranslatorTurengModule
	{
		private const string AcceptLanguage = "en-US,en;q=0.8,tr;q=0.6";
		private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";

		public DynamicTranslatorTurengModule(DynamicTranslatorConfiguration configurations)
		{
			var tureng = new TurengTranslatorConfiguration(configurations.ActiveTranslatorConfiguration,
				configurations.ApplicationConfiguration)
			{
				Url = "http://tureng.com/search/",
				SupportedLanguages = LanguageMapping.Tureng.ToLanguages()
			};

			configurations.TurengTranslatorConfiguration = tureng;

			configurations.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Tureng, Find(configurations, tureng));
		}

		private Find Find(DynamicTranslatorConfiguration configurations, TurengTranslatorConfiguration tureng) =>
			async (translateRequest, token) =>
			{
				var uri = new Uri(tureng.Url + translateRequest.CurrentText);

				IRestResponse response = await configurations.ClientFactory()
					.With(client =>
					{
						client.BaseUrl = uri;
						client.Encoding = Encoding.UTF8;
						client.CachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, TimeSpan.FromHours(1));
					}).ExecuteGetTaskAsync(
						new RestRequest(Method.GET)
							.AddHeader(Headers.UserAgent, UserAgent)
							.AddHeader(Headers.AcceptLanguage, AcceptLanguage));

				var mean = string.Empty;

				if (response.Ok())
				{
					mean = OrganizeMean(response.Content, translateRequest.FromLanguageExtension);
				}

				return new TranslateResult(true, mean);
			};

		public string OrganizeMean(string text, string fromLanguageExtension)
		{
			if (text == null)
			{
				return string.Empty;
			}

			string result = text;
			var output = new StringBuilder();
			var doc = new HtmlDocument();
			string decoded = WebUtility.HtmlDecode(result);
			doc.LoadHtml(decoded);

			if (!result.Contains("table") || doc.DocumentNode.SelectSingleNode("//table") == null)
			{
				return string.Empty;
			}

			(from x in doc.DocumentNode.Descendants()
			 where x.Name == "table"
			 from y in x.Descendants().AsParallel()
			 where y.Name == "tr"
			 from z in y.Descendants().AsParallel()
			 where (z.Name == "th" || z.Name == "td") && z.GetAttributeValue("lang", string.Empty) == (fromLanguageExtension == "tr" ? "en" : "tr")
			 from t in z.Descendants().AsParallel()
			 where t.Name == "a"
			 select t.InnerHtml)
				.AsParallel()
				.ToList()
				.ForEach(mean => output.AppendLine(mean));

			return output.ToString().ToLower().Trim();
		}
	}
}
