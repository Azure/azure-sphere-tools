/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.DeviceTests
{
    /// <summary>
    /// A test class for the get attached devices api endpoint.
    /// </summary>
    [TestClass]
    public class GetAttachedDevicesTests
    {
        /// <summary>
        /// Getting the attached devices returns a list of items in the correct format.
        /// </summary>
        [TestMethod]
        public void AttachedDevices_Get_ReturnsCorrectFormatDevices()
        {
            JsonSchema responseSchema =
                JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"DeviceConnectionPath\":{\"type\":\"string\"}, \"IpAddress\":{\"type\":\"string\"}}}");

            string response = Devices.GetAttachedDevices();

            JsonArray parsedResponse = JsonSerializer.Deserialize<JsonArray>(response);

            foreach (JsonNode device in parsedResponse)
            {
                Assert.IsTrue(responseSchema.Evaluate(device).IsValid);
            }
        }
    }
}
