using System;
using System.Threading.Tasks;

namespace DynamicTranslator.Wpf
{
    public interface ITranslatorBootstrapper : IDisposable
    {
        bool IsInitialized { get; }

        Task InitializeAsync();

        void SubscribeShutdownEvents();
    }
}