/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.DeviceTests
{
    /// <summary>
    /// A test class for the get device security state api endpoint.
    /// </summary>
    [TestClass]
    public class GetDeviceSecurityStateTests
    {
        /// <summary>
        /// Tests if getting the the device security state returns a response in the expected format.
        /// </summary>
        [TestMethod]
        public void GetDeviceSecurityState_Get_ReturnsCorrectFormat()
        {
            string response = Device.GetDeviceSecurityState();

            JsonSchema responseSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"securityState\":{\"type\": \"string\"},\"deviceIdentifier\":{\"type\": \"string\"},\"deviceIdentityPublicKey\":{\"type\": \"string\"}}}");

            JsonNode parsedObject = JsonNode.Parse(response);
            Assert.IsTrue(responseSchema.Evaluate(parsedObject).IsValid);
        }
    }
}
