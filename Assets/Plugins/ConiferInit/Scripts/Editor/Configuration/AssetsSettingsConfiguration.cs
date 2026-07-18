using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace ConiferInit.Editor.Configuration
{
    internal sealed class AssetsSettingsConfiguration
    {
        private readonly ConfigurationCache _configurationCache;
        private readonly Dictionary<string, AssetInfo> _assets = new Dictionary<string, AssetInfo>();
        
        public bool SuccessfullyRetrievedAssets { get; private set; } = false;
        public List<string> AvailableAssets { get; private set; }


        public AssetsSettingsConfiguration(ConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
        }


        public void Initialize()
        {
            RetrieveCachedAssets();
        }


        public void RetrieveCachedAssets()
        {
            string cachedAssetsPath;
            if (Environment.OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
            {
                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                cachedAssetsPath = Path.Combine(homeDirectory, "Library", "Unity", "Asset Store-5.x");
            }
            else
            {
                string defaultPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity");
                cachedAssetsPath = Path.Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath),
                    "Asset Store-5.x");
            }
                
            if (Directory.Exists(cachedAssetsPath))
            {
                string[] assetPaths = Directory.GetFiles(cachedAssetsPath, "*.unitypackage", SearchOption.AllDirectories);

                _assets.Clear();
                foreach (string assetPath in assetPaths)
                {
                    string id = Path.GetFileNameWithoutExtension(assetPath);
                    _assets.Add(id, new AssetInfo(assetPath, Path.GetFileNameWithoutExtension(assetPath), id));
                }

                SuccessfullyRetrievedAssets = true;
            }
            else
            {
                throw new DirectoryNotFoundException($"Couldn't find {cachedAssetsPath}");
            }
            
            GenerateAvailableAssets();
        }


        public void GenerateAvailableAssets()
        {
            AvailableAssets = SuccessfullyRetrievedAssets && _assets != null
                ? _assets.Keys.Where(id =>
                    !_configurationCache.QueuedAssets.Exists(a => a.ID == id)).ToList()
                : new List<string>();
        }


        public List<AssetImportEntry> GetQueuedAssets()
        {
            return _configurationCache.QueuedAssets;
        }
        
        
        public List<string> FindAssets(string nameFilter)
        {
            return AvailableAssets.FindAll(id =>
                _assets[id].Name.Contains(nameFilter,
                    StringComparison.OrdinalIgnoreCase));
        }


        public AssetInfo FindAssetByID(string id)
        {
            return _assets[id];
        }


        public void QueueAsset(string id)
        {
            _configurationCache.QueuedAssets.Add(new AssetImportEntry(_assets[id], false));
            
            GenerateAvailableAssets();
        }


        public void DequeueAsset(string id)
        {
            _configurationCache.QueuedAssets.Remove(_configurationCache.QueuedAssets.Find(a => a.ID == id));
            
            GenerateAvailableAssets();
        }


        public void SetInteractiveImportForAsset(string id, bool interactive)
        {
            int index = _configurationCache.QueuedAssets.FindIndex(a => a.ID == id);
            AssetImportEntry entry = _configurationCache.QueuedAssets[index];
            _configurationCache.QueuedAssets[index] =
                new AssetImportEntry(entry.Path, entry.Name, entry.ID, interactive);
        }
    }
}