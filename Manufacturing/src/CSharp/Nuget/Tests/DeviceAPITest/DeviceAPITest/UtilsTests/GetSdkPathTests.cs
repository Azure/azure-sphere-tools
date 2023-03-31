/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;

namespace TestDeviceRestAPI.UtilsTests
{
    /// <summary>
    /// A test class for utility functions
    /// </summary>
    [TestClass]
    public class GetSdkPathTests
    {
        /// <summary>
        /// Tests if the sdk version is returned.
        /// </summary>
        [TestMethod]
        public void GetSdkVersion_Call_ReturnsSdkPath()
        {
            string path = Utils.GetSdkPath();

            Assert.IsFalse(string.IsNullOrEmpty(path));
        }
    }
}
