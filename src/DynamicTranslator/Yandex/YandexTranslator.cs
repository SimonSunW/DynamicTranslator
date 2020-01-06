using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;
using DynamicTranslator.Google;
using DynamicTranslator.Model;

namespace DynamicTranslator.Yandex
{
    public class YandexTranslator: ITranslator
    {
        private const string AnonymousUrl = "https://translate.yandex.net/api/v1/tr.json/translate?";
        private const string BaseUrl = "https://translate.yandex.com/";
        private const string InternalSId = "id=93bdaee7.57bb46e3.e787b736-0-0";
        private const string Url = "https://translate.yandex.net/api/v1.5/tr/translate?";

        public YandexTranslator ()
        {
            //var yandex = new YandexTranslatorConfiguration(services.ActiveTranslatorConfiguration, services.ApplicationConfiguration)
            //{
            //    BaseUrl = BaseUrl,
            //    SId = InternalSId,
            //    Url = Url,
            //    ApiKey = services.AppConfigManager.Get("YandexApiKey"),
            //    SupportedLanguages = LanguageMapping.Yandex.ToLanguages()
            //};

            //string MakeMeaningful(string text)
            //{
            //    string output;
            //    if (text == null)
            //    {
            //        return string.Empty;

            //    }
            //    if (text.IsXml())
            //    {
            //        var doc = new XmlDocument();
            //        doc.LoadXml(text);
            //        XmlNode node = doc.SelectSingleNode("//Translation/text");
            //        output = node?.InnerText ?? "!!! An error occurred";
            //    }
            //    else
            //    {
            //        output = text.DeserializeAs<YandexDetectResponse>()?.Text.JoinAsString(",");
            //    }

            //    return output?.ToLower().Trim();
            //}

            //services.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Yandex, async (request, token) =>
            //{

            //    var address = new Uri(string.Format(yandex.Url +
            //                                        new StringBuilder()
            //                                            .Append($"key={yandex.ApiKey}")
            //                                            .Append(Headers.Ampersand)
            //                                            .Append($"lang={request.FromLanguageExtension}-{services.ApplicationConfiguration.ToLanguage.Extension}")
            //                                            .Append(Headers.Ampersand)
            //                                            .Append($"text={Uri.EscapeUriString(request.CurrentText)}")));

            //    HttpResponseMessage response = await services.ClientFactory().HttpClient.With(client => { client.BaseAddress = new Uri(BaseUrl); }).PostAsync(address, null);
            //    string mean = await response.Content.ReadAsStringAsync();
            //    if (response.IsSuccessStatusCode)
            //    {
            //        mean = MakeMeaningful(mean);
            //    }

            //    return new TranslateResult(true, mean);

            //});

            //services.AddTranslatorConfiguration(yandex);
        }

        public Task<TranslateResult> Translate(TranslateRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
