using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine.Assertions;

namespace ConiferInit.Editor.Configuration
{
    internal sealed class PackagesSettingsConfiguration
    {
        private readonly ConfigurationCache _configurationCache;
        
        private bool _successfullyRetrievedPackages;
        private Dictionary<string, PackageInfo> _allPackages;

        public List<string> AvailablePackages { get; private set; }
        
        public SearchRequest PackagesListRequest { get; private set; }


        public PackagesSettingsConfiguration(ConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
        }


        public void Initialize()
        {
            PackagesListRequest = Client.SearchAll();
        }
        
        
        public List<PackageImportEntry> GetQueuedPackageIDs()
        {
            return _configurationCache.QueuedPackages;
        }
        
        
        public bool SuccessfullyRetrievedPackages()
        {
            if (_successfullyRetrievedPackages)
            {
                return _allPackages != null;
            }
            
            switch (PackagesListRequest.Status)
            {
                case StatusCode.InProgress:
                    _successfullyRetrievedPackages = false;
                    break;
                case StatusCode.Success:
                    _successfullyRetrievedPackages = true;
                    _allPackages = PackagesListRequest.Result.ToDictionary(p => p.name, p => p);
                    GenerateAvailablePackages();
                    break;
                case StatusCode.Failure:
                    _successfullyRetrievedPackages = false;
                    break;
                default:
                    _successfullyRetrievedPackages = false;
                    throw new ArgumentOutOfRangeException();
            }

            return _successfullyRetrievedPackages;
        }


        public void GenerateAvailablePackages()
        {
            AvailablePackages = _successfullyRetrievedPackages && _allPackages != null
                ? _allPackages.Keys.Where(id =>
                    !_configurationCache.QueuedPackages.Exists(p => p.ShortID == id)).ToList()
                : new List<string>();
        }


        public List<string> FindPackages(string nameFilter)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(nameFilter));

            return AvailablePackages.FindAll(id => _allPackages[id].displayName.Contains(nameFilter,
                StringComparison.OrdinalIgnoreCase));
        }


        public PackageInfo GetPackageByID(string id)
        {
            return _allPackages[id];
        }


        public void QueuePackage(string id)
        {
            _configurationCache.QueuedPackages.Add(new PackageImportEntry(_allPackages[id]));

            GenerateAvailablePackages();
        }


        public void DequeuePackage(string id)
        {
            _configurationCache.QueuedPackages.Remove(_configurationCache.QueuedPackages.Find(p => p.ShortID == id));
            
            GenerateAvailablePackages();
        }
    }
}