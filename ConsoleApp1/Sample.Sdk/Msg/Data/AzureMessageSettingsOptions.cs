﻿using System.Diagnostics.CodeAnalysis;

namespace Sample.Sdk.Msg.Data
{
    public class AzureMessageSettingsOptions
    {
        public const string SENDER_SECTION_ID = "Service:AzureMessageSettings:Configuration:Sender";
        public const string RECEIVER_SECTION_ID = "Service:AzureMessageSettings:Configuration:Receiver";
        public string ConnStr { get; set; } = string.Empty; 
        public string QueueNames { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public List<MessageInTransitOptions> MessageInTransitOptions { get; set; } = new List<MessageInTransitOptions>();
    }

    public class AzureMessageSettingsOptionsComparer : IEqualityComparer<AzureMessageSettingsOptions>
    {
        public bool Equals(AzureMessageSettingsOptions? x, AzureMessageSettingsOptions? y)
        {
            return x?.ConnStr != y?.ConnStr;
        }

        public int GetHashCode([DisallowNull] AzureMessageSettingsOptions obj)
        {
            return obj.GetHashCode();
        }
    }
}
