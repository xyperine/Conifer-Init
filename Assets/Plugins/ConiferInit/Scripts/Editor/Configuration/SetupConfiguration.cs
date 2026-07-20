using ConiferInit.Editor.Execution;

namespace ConiferInit.Editor.Configuration
{
    /// <summary>
    /// Coordinates setup configuration.
    /// </summary>
    internal sealed class SetupConfiguration
    {
        private readonly ConfigurationCache _configurationCache;
        private readonly ExecutionCache _executionCache;
        
        private readonly SettingsProfileConfiguration _profileConfiguration;
        private readonly PackagesSettingsConfiguration _packagesConfiguration;
        private readonly AssetsSettingsConfiguration _assetsConfiguration;
        

        public SetupConfiguration(ConfigurationCache configurationCache, ExecutionCache executionCache,
            SettingsProfileConfiguration profileConfiguration, PackagesSettingsConfiguration packagesConfiguration,
            AssetsSettingsConfiguration assetsConfiguration)
        {
            _configurationCache = configurationCache;
            _executionCache = executionCache;
            
            _profileConfiguration = profileConfiguration;
            _packagesConfiguration = packagesConfiguration;
            _assetsConfiguration = assetsConfiguration;
            
            _profileConfiguration.AppliedProfile += OnAppliedProfile;
        }
        

        private void OnAppliedProfile()
        {
            _assetsConfiguration.GenerateAvailableAssets();
            _packagesConfiguration.GenerateAvailablePackages();
        }

        
        public void ClearCache()
        {
            _configurationCache.Clear();
        }
        

        public void Update()
        {
            if (!_executionCache.SetupInProgress)
            {
                _configurationCache.Save();
            }
        }
    }
}