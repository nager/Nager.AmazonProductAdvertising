﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nager.AmazonProductAdvertising.Model;
using System.IO;

namespace Nager.AmazonProductAdvertising.UnitTest
{
    [TestClass]
    public class XmlParseItemSearchTest
    {
        [TestMethod]
        [DeploymentItem("ItemSearchResponse.xml")]
        public void ParseItemSearchResponse()
        {
            var xml = Resources.LoadResource("ItemSearchResponse.xml");
            var result = XmlHelper.ParseXml<ItemSearchResponse>(xml);
            Assert.AreNotEqual(null, result);
            Assert.AreEqual(10, result.Items.Item.Length);
        }

        [TestMethod]
        [DeploymentItem("ItemSearchResponseWithError.xml")]
        public void ParseItemSearchResponseWithError()
        {
            var xml = Resources.LoadResource("ItemSearchResponseWithError.xml");
            var result = XmlHelper.ParseXml<ItemSearchResponse>(xml);
            Assert.AreNotEqual(null, result);
            Assert.AreEqual("AWS.RestrictedParameterValueCombination", result.Items.Request.Errors[0].Code);
        }

        [TestMethod]
        [DeploymentItem("ItemSearchErrorResponse.xml")]
        public void ParseItemSearchErrorResponse()
        {
            var xml = Resources.LoadResource("ItemSearchErrorResponse.xml");
            var result = XmlHelper.ParseXml<ItemSearchErrorResponse>(xml);
            Assert.AreNotEqual(null, result);
            Assert.AreNotEqual(null, result.RequestId);
            Assert.AreNotEqual(null, result.Error.Code);
            Assert.AreNotEqual(null, result.Error.Message);
        }
    }
}
