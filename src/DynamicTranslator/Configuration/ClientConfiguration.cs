namespace DynamicTranslator.Configuration
{
    public class ClientConfiguration
    {
        public ClientConfiguration(DynamicTranslatorServices serviceses)
        {
            Serviceses = serviceses;
        }

        public string AppVersion { get; set; }

        public DynamicTranslatorServices Serviceses { get; }

        public string Id { get; set; }

        public string MachineName { get; set; }
    }
}
