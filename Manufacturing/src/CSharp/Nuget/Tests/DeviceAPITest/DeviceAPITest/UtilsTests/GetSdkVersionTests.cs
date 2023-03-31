/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace TestDeviceRestAPI.UtilsTests
{
    /// <summary>
    /// A test class for utility functions
    /// </summary>
    [TestClass]
    public class GetSdkVersionTests
    {
        /// <summary>
        /// Tests if the sdk version is returned.
        /// </summary>
        [TestMethod]
        public void GetSdkVersion_Call_ReturnsSdkVersion()
        {
            SemanticVersion version = Utils.GetSdkVersion();

            Assert.IsNotNull(version);
        }

        /// <summary>
        /// Asserts that the current sdk version is greater than 22.11.0.
        /// </summary>
        [TestMethod]
        public void GetSdkVersion_Call_ReturnsSdkVersionGreaterThan22110()
        {
            SemanticVersion version = Utils.GetSdkVersion();

            Assert.IsTrue(version >= new SemanticVersion(22, 11, 0));
        }

        /// <summary>
        /// Asserts that the current sdk version is greater than 22.11.0.
        /// </summary>
        [TestMethod]
        public void GetSdkVersion_Call_MultipleCallsIsSame()
        {
            Assert.IsTrue(Utils.GetSdkVersion() == Utils.GetSdkVersion());
        }

        /// <summary>
        /// Asserts that an older SDK does not support multiple devices.
        /// </summary>
        [TestMethod]
        public void OlderSdk_Throws_On_ActiveDeviceIp_Change()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Utils.sdkVersion = new SemanticVersion(22, 11, 0);
                try
                {
                    Devices.SetActiveDeviceIpAddress("192.168.35.5");
                    Assert.Fail("Expected exception");
                }
                catch (AzureSphereException e)
                {
                    Assert.AreEqual("ERROR: Cannot set active device IP address to '192.168.35.5'. This SDK version does not support multiple devices.", e.Message);
                }
            }
            else
            {
                Assert.Inconclusive("This test is only valid on Linux");
            }

        }
    }
}
