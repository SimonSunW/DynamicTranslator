using System;
using System.Reflection;

using Abp.Dependency;
using Abp.Modules;

using Castle.MicroKernel.Registration;
using DynamicTranslator.Extensions;
using DynamicTranslator.Google;
using DynamicTranslator.Prompt;
using DynamicTranslator.SesliSozluk;
using DynamicTranslator.Tureng;
using DynamicTranslator.Yandex;
using Octokit;

public delegate bool IsNewVersion(string incomingVersion);

namespace DynamicTranslator.Wpf
{
    [DependsOn(
        typeof(DynamicTranslatorGoogleModule),
        typeof(DynamicTranslatorYandexModule),
        typeof(DynamicTranslatorTurengModule),
        typeof(DynamicTranslatorSesliSozlukModule),
        typeof(DynamicTranslatorPromptModule)
    )]
    public class DynamicTranslatorWpfModule : DynamicTranslatorModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
            IocManager.IocContainer.Register(
                Component.For<IsNewVersion>().UsingFactoryMethod<IsNewVersion>(kernel =>
                {
                    return version =>
                    {
                        var currentVersion = new Version(ApplicationVersion.GetCurrentVersion());
                        var newVersion = new Version(version);

                        return newVersion > currentVersion;
                    };
                })
            );
            IocManager.Register<GitHubClient>(new GitHubClient(new ProductHeaderValue(Configurations.ApplicationConfiguration.GitHubRepositoryName)), DependencyLifeStyle.Transient);
        }
    }
}
