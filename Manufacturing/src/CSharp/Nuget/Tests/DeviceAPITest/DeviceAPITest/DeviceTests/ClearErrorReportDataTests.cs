/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using System.Text;


namespace TestDeviceRestAPI.DeviceTests
{
    /// <summary>
    /// A test class for the clear error report data api endpoint.
    /// </summary>
    [TestClass]
    public class ClearErrorReportDataTests
    {
        /// <summary>
        /// Tests if clearing the error report data returns an empty json response.
        /// </summary>
        [TestMethod]
        public void ClearErrorReportData_Call_ReturnsEmptyJsonResponse()
        {
            string response = Device.ClearErrorReportData();

            Assert.AreEqual("{}", response);
        }

        /// <summary>
        /// Tests if clearing the error report data clears the error report data.
        /// </summary>
        [TestMethod]
        public void ClearErrorReportData_Call_ClearsErrorReportData()
        {
            int maxRetries = 3;
            int maxMilliseconds = 5000;
            int dataLength = -1;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                string response = Device.ClearErrorReportData();
                Assert.AreEqual("{}", response);

                int elapsedMilliseconds = 0;
                dataLength = GetDataLength(Device.GetErrorReportData());

                while (dataLength != 0 && elapsedMilliseconds < maxMilliseconds)
                {
                    Thread.Sleep(100);
                    elapsedMilliseconds += 100;
                    dataLength = GetDataLength(Device.GetErrorReportData());
                }

                if (dataLength == 0)
                {
                    break;
                }
            }

            Assert.AreEqual(0, dataLength);
        }

        /// <summary>
        /// Helper class that gets the data length bytes of the error reponse data.
        /// </summary>
        /// <param name="response">The error reponse data.</param>
        /// <returns>The data length bytes from the error response data.</returns>
        private static short GetDataLength(string response)
        {
            byte[] parsedResponse = Encoding.ASCII.GetBytes(response);

            return (short)(parsedResponse[3] << 8 | parsedResponse[4]);
        }
    }
}
