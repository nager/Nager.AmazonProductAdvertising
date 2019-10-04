﻿using Amazon.Runtime;
using Nager.AmazonProductAdvertising.Model;
using Nager.AmazonProductAdvertising.Model.Paapi;
using Nager.AmazonProductAdvertising.Model.Request;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nager.AmazonProductAdvertising
{
    /// <summary>
    /// Amazon Product Advertising Client
    /// </summary>
    public class AmazonProductAdvertisingClient
    {
        private readonly HttpClient _httpClient;
        private readonly ImmutableCredentials _credentials;
        private readonly string _partnerTag;
        private readonly JsonSerializerSettings _jsonSerializerSettingsResponse;
        private readonly JsonSerializerSettings _jsonSerializerSettingsRequest;
        private readonly AmazonEndpointConfig _amazonEndpointConfig;
        private readonly AmazonResourceValidator _amazonResourceValidator;

        /// <summary>
        /// Amazon Product Advertising Client
        /// </summary>
        /// <param name="amazonAuthentication"></param>
        /// <param name="amazonEndpoint"></param>
        /// <param name="partnerTag"></param>
        /// <param name="userAgent"></param>
        /// <param name="strictJsonMapping"></param>
        public AmazonProductAdvertisingClient(AmazonAuthentication amazonAuthentication, AmazonEndpoint amazonEndpoint, string partnerTag, string userAgent = null, bool strictJsonMapping = false)
        {
            this._httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));
            this._httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent ?? "Nager.AmazonProductAdvertising");

            this._credentials = new ImmutableCredentials(amazonAuthentication.AccessKey, amazonAuthentication.SecretKey, null);
            this._partnerTag = partnerTag;

            this._jsonSerializerSettingsRequest = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            this._jsonSerializerSettingsResponse = new JsonSerializerSettings
            {
                MissingMemberHandling = strictJsonMapping ? MissingMemberHandling.Error : MissingMemberHandling.Ignore
            };

            var amazonConfigEndpointConfigRepository = new AmazonEndpointConfigRepository();
            this._amazonEndpointConfig = amazonConfigEndpointConfigRepository.Get(amazonEndpoint);

            this._amazonResourceValidator = new AmazonResourceValidator();
        }

        /// <summary>
        /// Search items with a keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<SearchItemResponse> SearchItemsAsync(string keyword)
        {
            var request = new SearchRequest
            {
                Keywords = keyword,
                Resources = new[]
                {
                    "BrowseNodeInfo.BrowseNodes",
                    "BrowseNodeInfo.BrowseNodes.Ancestor",
                    "BrowseNodeInfo.BrowseNodes.SalesRank",

                    "Images.Primary.Small",
                    "Images.Primary.Medium",
                    "Images.Primary.Large",

                    "Images.Variants.Small",
                    "Images.Variants.Medium",
                    "Images.Variants.Large",

                    "ItemInfo.ByLineInfo",
                    "ItemInfo.Classifications",
                    "ItemInfo.ContentInfo",
                    "ItemInfo.ContentRating",
                    "ItemInfo.ExternalIds",
                    "ItemInfo.Features",
                    "ItemInfo.ProductInfo",
                    "ItemInfo.TechnicalInfo",
                    "ItemInfo.Title",
                    "ItemInfo.TradeInInfo",

                    "Offers.Listings.Availability.MinOrderQuantity",
                    "Offers.Listings.Availability.MaxOrderQuantity",
                    "Offers.Listings.Availability.Message",
                    "Offers.Listings.Availability.Type",
                    "Offers.Listings.Condition",
                    "Offers.Listings.Condition.SubCondition",
                    "Offers.Listings.DeliveryInfo.IsAmazonFulfilled",
                    "Offers.Listings.DeliveryInfo.IsFreeShippingEligible",
                    "Offers.Listings.DeliveryInfo.IsPrimeEligible",
                    "Offers.Listings.LoyaltyPoints.Points",
                    "Offers.Listings.MerchantInfo",
                    "Offers.Listings.Price",
                    "Offers.Listings.ProgramEligibility.IsPrimeExclusive",
                    "Offers.Listings.ProgramEligibility.IsPrimePantry",
                    "Offers.Listings.Promotions",
                    "Offers.Listings.SavingBasis",
                    "Offers.Summaries.HighestPrice",
                    "Offers.Summaries.LowestPrice",
                    "Offers.Summaries.OfferCount",

                    "ParentASIN",
                    "SearchRefinements",
                },
            };

            return await this.SearchItemsAsync(request);
        }

        /// <summary>
        /// Search items with a search request
        /// </summary>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public async Task<SearchItemResponse> SearchItemsAsync(SearchRequest searchRequest)
        {
            if (!this._amazonResourceValidator.IsResourcesValid(searchRequest.Resources, "SearchItems"))
            {
                return new SearchItemResponse { Successful = false, ErrorMessage = "Resources has wrong values" };
            }

            var searchItemRequest = new SearchItemRequest
            {
                Keywords = searchRequest.Keywords,
                Resources = searchRequest.Resources,
                ItemPage = searchRequest.ItemPage,
                SortBy = searchRequest.SortBy,
                BrowseNodeId = searchRequest.BrowseNodeId,
                PartnerTag = this._partnerTag,
                PartnerType = "Associates",
                Marketplace = $"www.{this._amazonEndpointConfig.Host}"
            };

            var json = JsonConvert.SerializeObject(searchItemRequest, this._jsonSerializerSettingsRequest);
            if (string.IsNullOrEmpty(json))
            {
                return new SearchItemResponse { Successful = false, ErrorMessage = "Cannot serialize object" };
            }

            var response = await this.RequestAsync("SearchItems", json);
            if (!response.Successful)
            {
                return new SearchItemResponse { Successful = false, ErrorMessage = $"API request failure {response.StatusCode} {response.Content}" };
            }

            var amazonResponse = JsonConvert.DeserializeObject<SearchItemResponse>(response.Content, this._jsonSerializerSettingsResponse);
            amazonResponse.Successful = (amazonResponse.SearchResult != null && amazonResponse.Errors == null);

            if (amazonResponse.Errors != null)
                amazonResponse.ErrorMessage = String.Join("\n", amazonResponse.Errors.Select(x => x.Message));

            return amazonResponse;
        }

        /// <summary>
        /// Get items via id
        /// </summary>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        public async Task<GetItemsResponse> GetItemsAsync(params string[] itemIds)
        {
            var request = new ItemsRequest
            {
                ItemIds = itemIds,
                Resources = new[]
                {
                    "BrowseNodeInfo.BrowseNodes",
                    "BrowseNodeInfo.BrowseNodes.Ancestor",
                    "BrowseNodeInfo.BrowseNodes.SalesRank",

                    "Images.Primary.Small",
                    "Images.Primary.Medium",
                    "Images.Primary.Large",

                    "Images.Variants.Small",
                    "Images.Variants.Medium",
                    "Images.Variants.Large",

                    "ItemInfo.ByLineInfo",
                    "ItemInfo.Classifications",
                    "ItemInfo.ContentInfo",
                    "ItemInfo.ContentRating",
                    "ItemInfo.ExternalIds",
                    "ItemInfo.Features",
                    "ItemInfo.ProductInfo",
                    "ItemInfo.TechnicalInfo",
                    "ItemInfo.Title",
                    "ItemInfo.TradeInInfo",

                    "Offers.Listings.Availability.MinOrderQuantity",
                    "Offers.Listings.Availability.MaxOrderQuantity",
                    "Offers.Listings.Availability.Message",
                    "Offers.Listings.Availability.Type",
                    "Offers.Listings.Condition",
                    "Offers.Listings.Condition.SubCondition",
                    "Offers.Listings.DeliveryInfo.IsAmazonFulfilled",
                    "Offers.Listings.DeliveryInfo.IsFreeShippingEligible",
                    "Offers.Listings.DeliveryInfo.IsPrimeEligible",
                    "Offers.Listings.LoyaltyPoints.Points",
                    "Offers.Listings.MerchantInfo",
                    "Offers.Listings.Price",
                    "Offers.Listings.ProgramEligibility.IsPrimeExclusive",
                    "Offers.Listings.ProgramEligibility.IsPrimePantry",
                    "Offers.Listings.Promotions",
                    "Offers.Listings.SavingBasis",
                    "Offers.Summaries.HighestPrice",
                    "Offers.Summaries.LowestPrice",
                    "Offers.Summaries.OfferCount",

                    "ParentASIN",
                }
            };

            return await this.GetItemsAsync(request);
        }

        /// <summary>
        /// Get items with a item request
        /// </summary>
        /// <param name="itemsRequest"></param>
        /// <returns></returns>
        public async Task<GetItemsResponse> GetItemsAsync(ItemsRequest itemsRequest)
        {
            if (!this._amazonResourceValidator.IsResourcesValid(itemsRequest.Resources, "GetItems"))
            {
                return new GetItemsResponse { Successful = false, ErrorMessage = "Resources has wrong values" };
            }

            var request = new GetItemsRequest
            {
                ItemIds = itemsRequest.ItemIds,
                Resources = itemsRequest.Resources,
                PartnerTag = this._partnerTag,
                PartnerType = "Associates",
                Marketplace = $"www.{this._amazonEndpointConfig.Host}",
            };

            var json = JsonConvert.SerializeObject(request, this._jsonSerializerSettingsRequest);
            if (string.IsNullOrEmpty(json))
            {
                return new GetItemsResponse { Successful = false, ErrorMessage = "Cannot serialize object" };
            }

            var response = await this.RequestAsync("GetItems", json);
            if (!response.Successful)
            {
                return new GetItemsResponse { Successful = false, ErrorMessage = $"API request failure {response.StatusCode} {response.Content}" };
            }

            var amazonResponse = JsonConvert.DeserializeObject<GetItemsResponse>(response.Content, this._jsonSerializerSettingsResponse);
            amazonResponse.Successful = true;
            return amazonResponse;
        }

        /// <summary>
        /// Get variations via asin
        /// </summary>
        /// <param name="asin"></param>
        /// <returns></returns>
        public async Task<GetVariationsResponse> GetVariationsAsync(string asin)
        {
            var request = new GetVariationsRequest
            {
                ASIN = asin,
                PartnerTag = this._partnerTag,
                PartnerType = "Associates",
                Marketplace = $"www.{this._amazonEndpointConfig.Host}",
                Resources = new[]
                {
                    "VariationSummary.VariationDimension",

                    "Images.Primary.Small",
                    "Images.Primary.Medium",
                    "Images.Primary.Large",

                    "Images.Variants.Small",
                    "Images.Variants.Medium",
                    "Images.Variants.Large",
                }
            };

            if (!this._amazonResourceValidator.IsResourcesValid(request.Resources, "GetVariations"))
            {
                return new GetVariationsResponse { Successful = false, ErrorMessage = "Resources has wrong values" };
            }

            var json = JsonConvert.SerializeObject(request, this._jsonSerializerSettingsRequest);
            if (string.IsNullOrEmpty(json))
            {
                return new GetVariationsResponse { Successful = false, ErrorMessage = "Cannot serialize object" };
            }

            var response = await this.RequestAsync("GetVariations", json);
            if (!response.Successful)
            {
                return new GetVariationsResponse { Successful = false, ErrorMessage = $"API request failure {response.StatusCode} {response.Content}" };
            }

            var amazonResponse = JsonConvert.DeserializeObject<GetVariationsResponse>(response.Content, this._jsonSerializerSettingsResponse);
            amazonResponse.Successful = true;
            return amazonResponse;
        }

        private async Task<HttpResponse> RequestAsync(string type, string json)
        {
            var serviceName = "ProductAdvertisingAPI";

            var amzTarget = $"com.amazon.paapi5.v1.ProductAdvertisingAPIv1.{type}";
            var requestUri = $"https://webservices.{this._amazonEndpointConfig.Host}/paapi5/{type.ToLower()}";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(requestUri),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Content.Headers.ContentEncoding.Add("amz-1.0");
            request.Headers.Add("x-amz-target", amzTarget);

            var result = await this._httpClient.SendAsync(request, this._amazonEndpointConfig.Region, serviceName, this._credentials);
            if (!result.IsSuccessStatusCode)
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                return new HttpResponse { Successful = false, StatusCode = result.StatusCode, Content = errorContent };
            }

            var content = await result.Content.ReadAsStringAsync();
            return new HttpResponse { Successful = true, Content = content };
        }
    }
}
