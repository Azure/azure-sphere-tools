/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using System.IO;
using System.Management.Automation;
using System;

namespace Microsoft.Azure.Sphere.DeviceAPI
{

    /// <summary>
    /// Utility functions for DeviceAPI functionality
    /// </summary>
    public static class Utils
    {
        internal static SemanticVersion sdkVersion = null;

        /// <summary>Retrieves the path to the currently installed Azure Sphere SDK</summary>
        /// <returns>A string containing the path to the Azure Sphere SDK installation. An exception will be thrown on error.</returns>
        public static string GetSdkPath()
        {
            var sdkPath = System.Environment.GetEnvironmentVariable("AzureSphereDefaultSDKDir");

            if (string.IsNullOrEmpty(sdkPath))
            {
                throw new AzureSphereException("Cannot retrieve the SDK path, 'AzureSphereDefaultSDKDir' environment variable is not set! Is the SDK installed and available?");
            }

            return sdkPath;
        }

        /// <summary>Retrieves the version of the currently installed Azure Sphere SDK</summary>
        /// <returns>A SemanticVersion object set to the current SDK version. An exception will be thrown on error.</returns>
        public static SemanticVersion GetSdkVersion()
        {
            if (sdkVersion != null)
            {
                return sdkVersion;
            }

            string sdkVersionPath = Path.Join(GetSdkPath(), "VERSION");

            if (File.Exists(sdkVersionPath))
            {
                string versionString = File.ReadAllText(sdkVersionPath).Replace("\r", "").Replace("\n", "").Trim();
                string previewVersion = "";

                int periodCount = versionString.Split('.').Length;

                if (periodCount > 3)
                {
                    if (periodCount > 4)
                    {
                        throw new AzureSphereException("Cannot retrieve the SDK version, version is not a parseable format!");
                    }

                    // if the build is a pre-release version, the run id will be appended to the end
                    // e.g. 22.11.5.456717 - this is not semver compliant
                    // reformat the version string to be semver compliant
                    previewVersion = versionString.Substring(versionString.LastIndexOf('.') + 1);
                    versionString = versionString.Substring(0, versionString.LastIndexOf('.'));
                    versionString += "-" + previewVersion;
                }

                // cache the version for subsequent calls.
                sdkVersion = SemanticVersion.Parse(versionString);
                return sdkVersion;
            }

            throw new AzureSphereException("Cannot retrieve the SDK version, version file does not exist! Is the SDK installed and available?");
        }
    }
}
