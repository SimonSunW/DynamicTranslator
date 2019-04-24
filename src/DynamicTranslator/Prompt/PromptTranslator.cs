using System.Collections.Generic;
using System.Net.Http;
using DynamicTranslator.Configuration;
using DynamicTranslator.Extensions;
using DynamicTranslator.Model;

namespace DynamicTranslator.Prompt
{
    public class PromptTranslator
    {
        private const string ContentType = "application/json;Charset=UTF-8";
        private const int CharacterLimit = 3000;
        private const string Template = "auto";
        private const string Ts = "MainSite";
        private const string Url = "http://www.online-translator.com/services/TranslationService.asmx/GetTranslateNew";

        public PromptTranslator(DynamicTranslatorServices services)
        {
            var cfg = new PromptTranslatorConfiguration(services.ActiveTranslatorConfiguration,
                services.ApplicationConfiguration)
            {
                Url = Url,
                Limit = CharacterLimit,
                Template = Template,
                Ts = Ts,
                SupportedLanguages = LanguageMapping.Prompt.ToLanguages()
            };

            services.PromptTranslatorConfiguration = cfg;

            services.ActiveTranslatorConfiguration.AddTranslator(TranslatorType.Prompt,
                async (translateRequest, token) =>
                {
                    var requestObject = new
                    {
                        dirCode =
                            $"{translateRequest.FromLanguageExtension}-{services.ApplicationConfiguration.ToLanguage.Extension}",
                        template = cfg.Template,
                        text = translateRequest.CurrentText,
                        lang = translateRequest.FromLanguageExtension,
                        limit = cfg.Limit,
                        useAutoDetect = true,
                        key = string.Empty,
                        ts = cfg.Ts,
                        tid = string.Empty,
                        IsMobile = false
                    };

                    HttpClient httpClient = services.ClientFactory()
                        .HttpClient
                        .With(client => { client.BaseAddress = cfg.Url.ToUri(); });


                    var request = new HttpRequestMessage {Method = HttpMethod.Post};
                    request.Headers.Add(Headers.ContentType, ContentType);
                    request.Content = new FormUrlEncodedContent(new[]
                        {new KeyValuePair<string, string>(Headers.ContentType, requestObject.ToJsonString(false))});
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    string mean = string.Empty;
                    if (response.IsSuccessStatusCode) mean = OrganizeMean(await response.Content.ReadAsStringAsync());

                    return new TranslateResult(true, mean);
                });
        }

        private string OrganizeMean(string text)
        {
            var promptResult = text.DeserializeAs<PromptResult>();
            return promptResult.d.result;
        }
    }
}