namespace ConiferInit.Editor.Configuration
{
    internal sealed class MiscSettingsConfiguration
    {
        private readonly ConfigurationCache _configurationCache;


        public MiscSettingsConfiguration(ConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
        }


        public MiscSettings GetMiscSettings()
        {
            return _configurationCache.MiscSettings;
        }


        public void SetMiscSettings(MiscSettings miscSettings)
        {
            _configurationCache.MiscSettings = miscSettings;
        }
    }
}