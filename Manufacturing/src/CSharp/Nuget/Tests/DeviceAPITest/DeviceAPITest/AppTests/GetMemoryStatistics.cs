/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.AppTests
{
    /// <summary>
    /// A test class for the get memory statistics api endpoint.
    /// </summary>
    [TestClass]
    public class GetMemoryStatistics
    {
        /// <summary>
        /// Tests if getting the memory statistics returns in the expected format.
        /// </summary>
        [TestMethod]
        public void GetMemoryStatistics_ReturnsValidResponseFormat()
        {
            string response = App.GetMemoryStatistics();
            JsonObject parsedObject = JsonNode.Parse(response).AsObject();
            Assert.IsTrue(parsedObject.ContainsKey("memoryStats"));
            JsonSchema expectedResponseSchema =
                JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"currentMemoryUsageInBytes\":{\"type\": \"integer\"},\"userModeMemoryUsageInByte\":{\"type\": \"integer\"},\"peakUserModeMemoryUsageInBytes\":{\"type\": \"integer\"}}}");
            Assert.IsTrue(expectedResponseSchema.Evaluate(parsedObject["memoryStats"]).IsValid);
        }
    }
}
