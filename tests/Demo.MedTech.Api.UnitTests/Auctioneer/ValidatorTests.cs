﻿using System;
using System.Collections.Generic;
using System.Linq;
using Demo.MedTech.DataModel.Exceptions;
using Demo.MedTech.Utility.Helper;
using Demo.MedTech.ValidationEngine.Model;
using Demo.MedTech.ValidationEngine.Rules;
using Xunit;

namespace Demo.MedTech.Api.UnitTests.Auctioneer
{
    public class ValidatorTests
    {
        private static readonly int IsValidDataTypeStatusCode = 101;
        private readonly IList<IRule> _rules;
        private readonly IList<ITransform> _transformRules;
        private static IRequestPipe _requestPipe;

        public ValidatorTests()
        {
            _requestPipe = new RequestPipe();
            _rules = typeof(IRule).Assembly.GetTypes()
                .Where(t => typeof(IRule).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as IRule).ToList();
            _transformRules = typeof(ITransform).Assembly.GetTypes()
                .Where(t => typeof(ITransform).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as ITransform).ToList();
        }

        [Theory]
        [InlineData("{\"AuctionId\":1,\"LotId\":30,\"openingPrice\":20,\"reservePrice\":null,\"increment\":[{\"Low\":0,\"High\":50,\"IncrementValue\":5},{\"Low\":50,\"IncrementValue\":100}],\"quantity\":5}")]
        public void Given_request_has_valid_data_type_When_valid_mandatory_data_is_executed_Then_should_not_return_validation_error(string request)
        {
            //Arrange
            var auctioneerContext = new AuctioneerContext(request, _requestPipe, _rules, _transformRules);

            //Assert
            Assert.NotNull(auctioneerContext.LotDetail);
        }

        [Fact]
        public void Given_user_not_provided_input_When_create_object_for_market_place_bidding_context_Then_should_return_null_reference_exception()
        {
            var caughtException = Assert.Throws<ArgumentNullException>(() => new AuctioneerContext(null, _requestPipe, _rules, _transformRules));

            Assert.Equal("Value cannot be null. (Parameter 'lotDetailJson')", caughtException.Message);
        }

        [Theory]
        [InlineData("{\"AuctionId\":1,\"LotId\":null,\"openingPrice\":20,\"reservePrice\":null,\"increment\":[{\"Low\":0,\"High\":50,\"IncrementValue\":5},{\"Low\":50,\"IncrementValue\":100}],\"quantity\":5}")]
        public void Given_request_has_invalid_data_type_When_valid_mandatory_data_is_executed_Then_should_return_validation_error(string request)
        {
            //Arrange
            var caughtException = Assert.Throws<RuleEngineException>(() =>
                new AuctioneerContext(request, _requestPipe, _rules, _transformRules));

            //Assert
            Assert.False(caughtException.RuleValidationMessage.IsValid);
            Assert.Equal(IsValidDataTypeStatusCode, caughtException.RuleValidationMessage.ValidationResults.FirstOrDefault()?.Code);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == IsValidDataTypeStatusCode)?.Value, caughtException.RuleValidationMessage.ValidationResults.FirstOrDefault()?.Value);
        }
    }
}