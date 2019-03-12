using System.Configuration;
using System.Linq;

namespace DynamicTranslator.Configuration
{
	public class AppConfigManager
	{
		public string Get(string key)
		{
			if (ConfigurationManager.AppSettings.AllKeys.ToList().Contains(key))
			{
				return ConfigurationManager.AppSettings[key];
			}

			return string.Empty;
		}

		public void SaveOrUpdate(string key, string value)
		{
			System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.AppSettings.Settings[key].Value = value;
			configuration.Save();
			ConfigurationManager.RefreshSection("appSettings");
		}
	}
}
