using DynamicTranslator.Configuration.Startup;

namespace DynamicTranslator.Configuration
{
    public class ClientConfiguration
    {
        public ClientConfiguration(DynamicTranslatorConfiguration configurations)
        {
            Configurations = configurations;
        }

        public string AppVersion { get; set; }

        public DynamicTranslatorConfiguration Configurations { get; }

        public string Id { get; set; }

        public string MachineName { get; set; }
    }
}
