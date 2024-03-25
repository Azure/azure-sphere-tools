/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;

namespace TestDeviceRestAPI.UtilsTests
{
    /// <summary>
    /// A test class for utility functions
    /// </summary>
    [TestClass]
    public class GetSetTimeoutTests
    {
        /// <summary>
        /// Tests if the timeout is set correctly
        /// </summary>
        [TestMethod]
        public void GetTimeoutMatchesSet()
        {
            uint timeout = RestUtils.GetRequestTimeout();
            Assert.AreEqual(1000, timeout);
        }

        /// <summary>
        /// Tests if the timeout is applied to requests
        /// </summary>
        [TestMethod]
        public void GetSetTimeoutIsApplied()
        {
            RestUtils.SetRequestTimeout(0);
            Assert.ThrowsException<AzureSphereException>(() => Device.GetDeviceStatus());
        }
    }
}
