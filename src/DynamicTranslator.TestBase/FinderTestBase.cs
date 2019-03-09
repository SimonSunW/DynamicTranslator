using System.Threading.Tasks;

using Abp.Dependency;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Google.Configuration;
using DynamicTranslator.LanguageManagement;
using DynamicTranslator.Orchestrators.Organizers;
using DynamicTranslator.Prompt.Configuration;
using DynamicTranslator.SesliSozluk.Configuration;
using DynamicTranslator.Tureng.Configuration;
using DynamicTranslator.Yandex.Configuration;
using NSubstitute;

using RestSharp;

namespace DynamicTranslator.TestBase
{
    public class FinderTestBase<TSut, TConfig, TConfigImplementation, TMeanOrganizer> : TestBaseWithLocalIocManager
        where TConfigImplementation : class, TConfig
        where TConfig : class, IMustHaveUrl
        where TMeanOrganizer : AbstractMeanOrganizer, IMeanOrganizer
        where TSut : class
    {
        protected FinderTestBase()
        {
            ApplicationConfiguration = Substitute.For<IApplicationConfiguration, ApplicationConfiguration>();
            ApplicationConfiguration.FromLanguage.Returns(new Language("English", "en"));
            ApplicationConfiguration.ToLanguage.Returns(new Language("Turkish", "tr"));
            Register(ApplicationConfiguration);

            RestClient = Substitute.For<IRestClient>();
            Register(RestClient);

            TranslatorConfiguration = Substitute.For<TConfig, TConfigImplementation>();
            TranslatorConfiguration.Url.Returns("http://www.dummycorrecturl.com/");
            Register(TranslatorConfiguration);

            MeanOrganizer = Substitute.For<IMeanOrganizer, TMeanOrganizer>();
            MeanOrganizer.TranslatorType.Returns(FindTranslatorType());
            MeanOrganizer.OrganizeMean(Arg.Any<string>()).Returns(Task.FromResult(new Maybe<string>("selam")));
            MeanOrganizer.OrganizeMean(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(new Maybe<string>("selam")));
            Register((TMeanOrganizer)MeanOrganizer, DependencyLifeStyle.Transient);

            Register<TSut>();
        }

        protected IApplicationConfiguration ApplicationConfiguration { get; set; }

        protected IMeanOrganizer MeanOrganizer { get; set; }

        protected TConfig TranslatorConfiguration { get; set; }

        protected IRestClient RestClient { get; set; }

        protected TSut ResolveSut()
        {
            return Resolve<TSut>();
        }

        private TranslatorType FindTranslatorType()
        {
            if (typeof(TConfig) == typeof(IGoogleTranslatorConfiguration))
            {
                return TranslatorType.Google;
            }

            if (typeof(TConfig) == typeof(IYandexTranslatorConfiguration))
            {
                return TranslatorType.Yandex;
            }

            if (typeof(TConfig) == typeof(IPromptTranslatorConfiguration))
            {
                return TranslatorType.Prompt;
            }

            if (typeof(TConfig) == typeof(ISesliSozlukTranslatorConfiguration))
            {
                return TranslatorType.SesliSozluk;
            }

            if (typeof(TConfig) == typeof(ITurengTranslatorConfiguration))
            {
                return TranslatorType.Tureng;
            }

            return default(TranslatorType);
        }
    }
}
