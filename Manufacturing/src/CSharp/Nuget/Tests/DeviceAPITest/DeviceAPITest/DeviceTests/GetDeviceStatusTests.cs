/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.DeviceTests
{
    /// <summary>
    /// A test class for the get device status api endpoint.
    /// </summary>
    [TestClass]
    public class GetDeviceStatusTests
    {
        /// <summary>
        /// Tests if getting the device status returns a response in the correct format.
        /// </summary>
        [TestMethod]
        public void GetDeviceStatus_Get_ReturnsCorrectFormat()
        {
            JsonSchema responseSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"uptime\": {\"type\":\"integer\"}}}");

            string response = Device.GetDeviceStatus();

            JsonNode parsedObject = JsonNode.Parse(response);
            Assert.IsTrue(responseSchema.Evaluate(parsedObject).IsValid);
        }

        /// <summary>
        /// Tests if getting the device status at later points increases the \"uptime\".
        /// </summary>
        [TestMethod]
        public void GetDeviceStatus_GetThenGet_UptimeIncreasesWithLaterCalls()
        {
            string response = Device.GetDeviceStatus();
            int startTime = JsonSerializer.Deserialize<Dictionary<string, int>>(response)["uptime"];

            Thread.Sleep(1000);

            string endResponse = Device.GetDeviceStatus();

            int endTime = JsonSerializer.Deserialize<Dictionary<string, int>>(endResponse)["uptime"];

            Assert.IsTrue(startTime < endTime);
        }
    }
}
