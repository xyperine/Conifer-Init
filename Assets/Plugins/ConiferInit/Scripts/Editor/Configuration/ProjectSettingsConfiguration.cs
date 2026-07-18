namespace ConiferInit.Editor.Configuration
{
    internal sealed class ProjectSettingsConfiguration
    {
        private readonly ConfigurationCache _configurationCache;


        public ProjectSettingsConfiguration(ConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
        }


        public ProjectSettings GetProjectSettings()
        {
            return _configurationCache.ProjectSettings;
        }


        public void SetProjectSettings(ProjectSettings projectSettings)
        {
            _configurationCache.ProjectSettings = projectSettings;
        }
    }
}