using System;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;
using HtmlAgilityPack;

namespace DynamicTranslator.Tureng
{
    public class TurengTranslator
    {
        private const string AcceptLanguage = "en-US,en;q=0.8,tr;q=0.6";
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";

        public TurengTranslator(DynamicTranslatorServices services)
        {
            var tureng = new TurengTranslatorConfiguration(services.ActiveTranslatorConfiguration,
                services.ApplicationConfiguration)
            {
                Url = "http://tureng.com/search/",
                SupportedLanguages = LanguageMapping.Tureng.ToLanguages()
            };

            services.TurengTranslatorConfiguration = tureng;

            services.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Tureng, Find(services, tureng));
        }

        private Find Find(DynamicTranslatorServices services, TurengTranslatorConfiguration tureng) =>
            async (translateRequest, token) =>
            {
                var uri = new Uri(tureng.Url + translateRequest.CurrentText);

                var httpClient = services.ClientFactory().HttpClient
                    .With(client => { client.BaseAddress = uri; });

                var req = new HttpRequestMessage { Method = HttpMethod.Get };
                req.Headers.Add(Headers.UserAgent, UserAgent);
                req.Headers.Add(Headers.AcceptLanguage, AcceptLanguage);

                HttpResponseMessage response = await httpClient.SendAsync(req);
                
                string mean = string.Empty;
                if (response.IsSuccessStatusCode)
                {
                    mean = OrganizeMean(await response.Content.ReadAsStringAsync(), translateRequest.FromLanguageExtension);
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
