/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;
using TestDeviceRestAPI.Helpers;

namespace TestDeviceRestAPI.CertificateTests
{
    /// <summary>
    /// A test class for the get certificate space api endpoint.
    /// </summary>
    [TestClass]
    public class GetCertificateSpaceTests
    {
        /// <summary>
        /// Removes added certificates before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            Utilities.CleanCertificates();
        }

        /// <summary>
        /// Tests if getting the certificate space returns a response in the expected format.
        /// </summary>
        [TestMethod]
        public void GetCertificateSpace_Get_ReturnsResponseInExpectedFormat()
        {
            string response =
                Certificate.GetCertificateSpace();

            JsonSchema responseSchema =
                JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"AvailableSpace\" : {\"type\":\"string\"}}}");

            JsonNode parsedObject = JsonNode.Parse(response);
            Assert.IsTrue(responseSchema.Evaluate(parsedObject).IsValid);
        }

        /// <summary>
        /// Tests if getting the certificate space after adding a certificate, decreases the certificate space.
        /// </summary>
        [TestMethod]
        public void GetCertificateSpace_AddingCertificate_ReducesCertSpace()
        {
            string startResponse =
                Certificate.GetCertificateSpace();
            int startSpace = JsonSerializer.Deserialize<Dictionary<string, int>>(
                startResponse, new JsonSerializerOptions
                {
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                })["AvailableSpace"];

            Certificate.AddCertificate("RootID", Utilities.pathToTestRootCert,
                                       "rootca");

            string endResponse =
                Certificate.GetCertificateSpace();
            int endSpace = JsonSerializer.Deserialize<Dictionary<string, int>>(
                endResponse, new JsonSerializerOptions
                {
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                })["AvailableSpace"];

            Assert.IsTrue(startSpace > endSpace);
        }

        /// <summary>
        /// Tests if getting the certificate space after removing a certificate increases the certificate space.
        /// </summary>
        [TestMethod]
        public void GetCertificateSpace_RemoveCertificate_IncreasesCertSpace()
        {
            Certificate.AddCertificate("TestCert", Utilities.pathToTestRootCert,
                                       "rootca");

            string startResponse =
                Certificate.GetCertificateSpace();
            int startSpace = JsonSerializer.Deserialize<Dictionary<string, int>>(
                startResponse, new JsonSerializerOptions
                {
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                })["AvailableSpace"];

            Certificate.RemoveCertificate("TestCert");

            string endResponse =
                Certificate.GetCertificateSpace();
            int endSpace = JsonSerializer.Deserialize<Dictionary<string, int>>(
                endResponse, new JsonSerializerOptions
                {
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                })["AvailableSpace"];

            Assert.IsTrue(startSpace < endSpace);
        }
    }
}
