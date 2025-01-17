﻿using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.Utility.Helper;
using Demo.MedTech.ValidationEngine.Model;
using System.Linq;

namespace Demo.MedTech.ValidationEngine.Rules.Auctioneer.Atomic
{
    public class HasValidQuantity : IRule
    {
        private const int HasValidQuantityErrorCode = 158;

        public RuleValidationMessage Execute(AuctioneerContext auctioneerContext)
        {
            RuleValidationMessage ruleValidationMessage = new RuleValidationMessage() { IsValid = true };

            if (auctioneerContext.LotDetail.Quantity < 0)
            {
                ruleValidationMessage.IsValid = false;
                ruleValidationMessage.ValidationResults.AddRange(
                    Response.ValidationResults.Where(x => x.Code == HasValidQuantityErrorCode));
            }

            return ruleValidationMessage;
        }
    }
}