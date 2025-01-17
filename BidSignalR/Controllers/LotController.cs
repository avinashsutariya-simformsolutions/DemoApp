﻿using Demo.MedTech.DataModel.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Playground.Models;
using Playground.Policies;
using Playground.Services.IServices;
using RestSharp;
using System;
using System.Text;
using System.Text.Json;

namespace Playground.Controllers
{
    public class EgressLotDetail : LotDetail
    {
        public bool EgressIsPiecemeal
        {
            get => false;
            set {; }
        }
    }

    public class LotController : Controller
    {
        private readonly IRestClientApiCall _restClientApiCall;
        private static IConfiguration _configuration;
        private readonly CosmosPollySettings _cosmosPollySettings;

        public LotController(IRestClientApiCall restClientApiCall, IConfiguration configuration, IOptions<CosmosPollySettings> cosmosOptions)
        {
            _restClientApiCall = restClientApiCall;
            _configuration = configuration;
            _cosmosPollySettings = cosmosOptions.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyLot(EgressLotDetail lotDetails)
        {
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", JsonSerializer.Serialize(lotDetails), ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["INGRESS_API"] + _configuration["VERIFY_LOT_ENDPOINT"]);
            return Ok(response.Content);
        }

        [HttpPost]
        public ActionResult CreateLot(EgressLotDetail lotDetails)
        {
            var request = new RestRequest(Method.POST);
            StringBuilder correlation = new StringBuilder();

            string correlationId = correlation.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
            .Append("SBS")
            .Append("PLAY")
            .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
            .ToString();

            request.AddHeader("x-correlation-id", correlationId);
            request.AddParameter("application/json", JsonSerializer.Serialize(CreateLotObj(lotDetails)),
                ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["INGRESS_API"] + _configuration["LOT_LATEST_DETAILS_ENDPOINT"]);

            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    var policy = RetryPolicy.GetCosmosAsyncPolicy(_cosmosPollySettings.RetryTimeInSeconds, _cosmosPollySettings.RetryCount, _cosmosPollySettings.TimeoutPeriodInSeconds);
            //    _ = Task.Run(async () =>
            //    {
            //        await policy.ExecuteAsync(async (ct) =>
            //        {
            //            await _cosmosRepo.ResetLotAsync(lotDetails.AuctionId, lotDetails.LotId);
            //        }, new CancellationToken());
            //    }).ConfigureAwait(false);
            //}

            return Ok(response.Content == "" ? response?.ErrorException?.Message : response.Content);
        }

        [HttpPut]
        public ActionResult UpdateLot(EgressLotDetail lotDetails)
        {
            var request = new RestRequest(Method.POST);
            StringBuilder correlation = new StringBuilder();

            string correlationId = correlation.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
                .Append("SBS")
                .Append("PLAY")
                .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
                .ToString();

            request.AddHeader("x-correlation-id", correlationId);
            request.AddParameter("application/json", JsonSerializer.Serialize(CreateLotObj(lotDetails)),
                ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["INGRESS_API"] + _configuration["LOT_UPDATE_DETAILS_ENDPOINT"]);

            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    var policy = RetryPolicy.GetCosmosAsyncPolicy(_cosmosPollySettings.RetryTimeInSeconds, _cosmosPollySettings.RetryCount, _cosmosPollySettings.TimeoutPeriodInSeconds);
            //    _ = Task.Run(async () =>
            //    {
            //        await policy.ExecuteAsync(async (ct) =>
            //        {
            //            await _cosmosRepo.ResetLotAsync(lotDetails.AuctionId, lotDetails.LotId);
            //        }, new CancellationToken());
            //    }).ConfigureAwait(false);
            //}

            return Ok(response.Content == "" ? response?.ErrorException?.Message : response.Content);
        }

        [HttpDelete]
        public ActionResult DeleteLot(long auctionId, long lotId)
        {
            string url = _configuration["INGRESS_API"] + _configuration["DELETE_LOT_ENDPOINT"] + $"?auctionId={auctionId}&lotId={lotId}";
            var request = new RestRequest(Method.DELETE);
            StringBuilder correlation = new StringBuilder();

            string correlationId = correlation.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
                .Append("SBS")
                .Append("PLAY")
            .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
            .ToString();

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("x-correlation-id", correlationId);

            IRestResponse response = _restClientApiCall.Execute(request, url);
            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    var policy = RetryPolicy.GetCosmosAsyncPolicy(_cosmosPollySettings.RetryTimeInSeconds, _cosmosPollySettings.RetryCount, _cosmosPollySettings.TimeoutPeriodInSeconds);
            //    _ = Task.Run(async () =>
            //    {
            //        await policy.ExecuteAsync(async (ct) =>
            //        {
            //            await _cosmosRepo.ResetLotAsync(auctionId, lotId);
            //        }, new CancellationToken());
            //    }).ConfigureAwait(false);
            //}

            if (response.Content == "")
            {
                response.Content = "{\"isValid\":true,\"validationResults\":[]}";
            }

            return Ok(response.Content);
        }

        private EgressLotModel CreateLotObj(LotDetail lotDetails)
        {
            return new EgressLotModel()
            {
                Domain = "SBS",
                SubDomain = "Auctioneer",
                LotId = lotDetails.LotId,
                AuctionId = lotDetails.AuctionId,
                LotDetail = lotDetails,
            };
        }
    }
}
