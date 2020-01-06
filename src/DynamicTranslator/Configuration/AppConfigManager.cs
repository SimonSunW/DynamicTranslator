using Microsoft.Extensions.Configuration;

namespace DynamicTranslator.Configuration
{
	public class AppConfigManager
    {
        private readonly IConfigurationRoot _configurationRoot;
        public AppConfigManager(IConfigurationRoot configurationRoot)
        {
            this._configurationRoot = configurationRoot;
        }
		public string Get(string key)
        {
            return _configurationRoot[key];
        }

		public void SaveOrUpdate(string key, string value)
        {
            _configurationRoot[key] = value;
            //TODO SAVE ini file
		}
	}
}
