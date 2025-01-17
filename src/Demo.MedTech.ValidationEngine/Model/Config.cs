﻿using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.Utility.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Demo.MedTech.DataModel.Exceptions;

namespace Demo.MedTech.ValidationEngine.Model
{
    public class Config
    {
        private const int ConfigContextErrorCode = 116;

        #region Properties

        private static List<PlatformConfig> PlatformRules { get; }
        private static List<RuleType> DefaultLotRuleGroup { get; }
        private static List<RuleType> DefaultAuctionRuleGroup { get; }
        public List<RuleType> AuctionRuleGroup { get; }
        public List<RuleType> LotRuleGroup { get; }

        public static Dictionary<string, bool> DynamicAuditLogRules { get; }
        public static Dictionary<string, string> ErrorDescriptions { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// This constructor do file read and serialization operation and throw exception if any
        /// </summary>
        static Config()
        {
            var validationResult = new RuleValidationMessage() { IsValid = true };
            try
            {
                //Reading file and serialize object of ErrorDescriptions
                ErrorDescriptions = new Dictionary<string, string>();
                var jsonErrorDescriptions =
                    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ErrorDescriptions.json"));
                ErrorDescriptions = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonErrorDescriptions,
                    JsonSerializerOption.CaseInsensitive);

                //Reading file and serialize object of DynamicLogConfig
                DynamicAuditLogRules = new Dictionary<string, bool>();
                var jsonDynamicLogConfig =
                    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "AuctioneerDynamicLogConfiguration.json"));
                DynamicAuditLogRules = JsonSerializer.Deserialize<Dictionary<string, bool>>(jsonDynamicLogConfig,
                    JsonSerializerOption.CaseInsensitive);

                //Reading file and serialize object of PlatformConfig
                PlatformRules = new List<PlatformConfig>();
                var jsonPlatformConfig =
                    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "AuctioneerPlatformConfiguration.json"));
                PlatformRules = JsonSerializer.Deserialize<List<PlatformConfig>>(jsonPlatformConfig,
                    JsonSerializerOption.CaseInsensitive);

                if (!DynamicAuditLogRules.Any() || !PlatformRules.Any())
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationResults.AddRange(
                        Response.ValidationResults.Where(x => x.Code == ConfigContextErrorCode));
                    throw new RuleEngineException(validationResult);
                }

                var platformConfig = PlatformRules.FirstOrDefault(x => x.PlatformCode == "0");
                if (platformConfig != null)
                {
                    DefaultLotRuleGroup = platformConfig.LotRuleGroup;
                    DefaultAuctionRuleGroup = platformConfig.AuctionRuleGroup;
                }
                else
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationResults.AddRange(
                        Response.ValidationResults.Where(x => x.Code == ConfigContextErrorCode));
                    throw new RuleEngineException(validationResult);
                }
            }
            catch (Exception)
            {
                validationResult.IsValid = false;
                validationResult.ValidationResults.AddRange(Response.ValidationResults.Where(x => x.Code == ConfigContextErrorCode));
                throw new RuleEngineException(validationResult);
            }
        }

        /// <summary>
        /// Assign rule group when context is initiated
        /// </summary>
        /// <param name="platformCode">Platform is passed by user</param>
        public Config(string platformCode)
        {
            if (PlatformRules.Any(x => x.PlatformCode == platformCode))
            {
                var platformGroup = PlatformRules.FirstOrDefault(x => x.PlatformCode == platformCode);
                LotRuleGroup = platformGroup?.LotRuleGroup;
                AuctionRuleGroup = platformGroup?.AuctionRuleGroup;
            }
            else
            {
                LotRuleGroup = DefaultLotRuleGroup;
                AuctionRuleGroup = DefaultAuctionRuleGroup;
            }
        }

        #endregion
    }
}