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
            UInt32 timeout = RestUtils.SetRequestTimeout(1000);
            UInt32 gettimeout = RestUtils.GetRequestTimeout();

            Assert.AreEqual(timeout, gettimeout);
        }

        /// <summary>
        /// Tests if the timeout is applied to requests
        /// </summary>
        [TestMethod]
        public void GetSetTimeoutIsApplied()
        {
            UInt32 timeout = RestUtils.SetRequestTimeout(0);
            Assert.ThrowsException<AzureSphereException>(() => Device.GetDeviceStatus());
        }
    }
}
