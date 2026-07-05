using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.Build;
using UnityEngine.Assertions;

namespace ConiferInit.Editor
{
    public static class NamedBuildTargetHelpers
    {
        public static IReadOnlyCollection<NamedBuildTarget> AllTargets { get; } = new []
        {
            NamedBuildTarget.Android,
            NamedBuildTarget.EmbeddedLinux,
            NamedBuildTarget.iOS,
            NamedBuildTarget.LinuxHeadlessSimulation,
            NamedBuildTarget.NintendoSwitch,
            NamedBuildTarget.NintendoSwitch2,
            NamedBuildTarget.PS4,
            NamedBuildTarget.PS5,
            NamedBuildTarget.QNX,
            NamedBuildTarget.Server,
            NamedBuildTarget.Standalone,
            NamedBuildTarget.tvOS,
            NamedBuildTarget.VisionOS,
            NamedBuildTarget.WebGL,
            NamedBuildTarget.WindowsStoreApps,
            NamedBuildTarget.XboxOne,
        };
        

        public static NamedBuildTarget FindByName(string name)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(name));

            NamedBuildTarget result = AllTargets.SingleOrDefault(t => t.TargetName == name);

            if (result.TargetName == string.Empty)
            {
                throw new ArgumentException("Invalid build target name!");
            }

            return result;
        }
    }
}