/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;
using TestDeviceRestAPI.Helpers;

namespace TestDeviceRestAPI.ImageTests
{
    /// <summary>
    /// A test class for the get images api endpoint.
    /// </summary>
    [TestClass]
    public class GetImagesTests
    {
        /// <summary>Initialize Get Images tests - removes any installed applications (except gdbserver).</summary>
        [TestInitialize]
        public void TestInitialize()
        {
            Utilities.CleanImages();
        }

        /// <summary>Tests if getting images returns all images in correct format.</summary>
        [TestMethod]
        public void GetImages_Get_ReturnsCorrectFormat()
        {
            JsonSchema topLevelSchema =
                JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"is_ota_update_in_progress\" : {\"type\":\"boolean\"}, \"has_staged_updates\" : {\"type\":\"boolean\"}, \"restart_required\" : {\"type\":\"boolean\"}, \"components\" : {\"type\":\"array\"}}}");
            JsonSchema componentsSchema =
                JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"uid\": {\"type\":\"string\"}, \"image_type\": {\"type\":\"integer\"}, \"is_update_staged\": {\"type\":\"boolean\"}, \"does_image_type_require_restart\": {\"type\":\"boolean\"}, \"images\": {\"type\":\"array\"}, \"name\": {\"type\":\"string\"}}}");
            JsonSchema imagesSchema =
                JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"uid\": {\"type\":\"string\"}, \"length_in_bytes\": {\"type\":\"integer\"}, \"uncompressed_length_in_bytes\": {\"type\":\"integer\"}, \"replica_type\": {\"type\":\"integer\"}}}");

            string response = Image.GetImages();

            JsonNode topLevelObject = JsonNode.Parse(response);
            Assert.IsTrue(topLevelSchema.Evaluate(topLevelObject).IsValid);

            JsonArray components = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(response)["components"].AsArray();

            foreach (JsonObject component in components)
            {
                Assert.IsTrue(componentsSchema.Evaluate(component).IsValid);

                JsonArray images = component["images"].AsArray();

                foreach (JsonObject image in images)
                {
                    Assert.IsTrue(imagesSchema.Evaluate(image).IsValid);
                }
            }
        }

        /// <summary>Tests if adding an image is reflected in a get images call.</summary>
        [TestMethod]
        public void GetImages_AddImage_AddsImageToComponentsList()
        {
            List<Dictionary<string, object>> startImages =
                Utilities.GetImageComponents();
            Dictionary<string, object>
                 blinkStart = startImages.Find(
                      elem =>
                          ((string)elem["uid"]).Equals(Utilities.blinkComponentId));

            Assert.IsTrue(blinkStart == null);

            Sideload.StageImage(Utilities.pathToBlinkImage);
            Sideload.InstallImages();

            List<Dictionary<string, object>> endImages =
                Utilities.GetImageComponents();
            Dictionary<string, object>
                blinkEnd = endImages.Find(
                      elem =>
                          ((string)elem["uid"]).Equals(Utilities.blinkComponentId));

            Assert.IsTrue(blinkEnd != null);
        }

        /// <summary>Tests if adding and then deleting an image shows no change in get images.</summary>
        [TestMethod]
        public void GetImages_DeleteImage_DeletesImageFromComponentsList()
        {
            Sideload.StageImage(Utilities.pathToBlinkImage);
            Sideload.InstallImages();

            List<Dictionary<string, object>> startImages =
                Utilities.GetImageComponents();
            Dictionary<string, object>
                blinkStart = startImages.Find(
                      elem =>
                          ((string)elem["uid"]).Equals(Utilities.blinkComponentId));

            Assert.IsTrue(blinkStart != null);

            Sideload.DeleteImage(Utilities.blinkComponentId);

            List<Dictionary<string, object>> endImages =
                Utilities.GetImageComponents();
            Dictionary<string, object>
                blinkEnd = endImages.Find(
                      elem =>
                          ((string)elem["uid"]).Equals(Utilities.blinkComponentId));

            Assert.IsTrue(blinkEnd == null);
        }
    }
}
