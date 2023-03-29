﻿using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Options;
using Sample.Sdk.Core.Attributes;
using Sample.Sdk.Core.Azure;
using Sample.Sdk.Core.Security;
using Sample.Sdk.Msg.Data;
using Sample.Sdk.Msg.Data.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Sdk.Msg
{
    public class MessageInTransitService : IMessageInTransitService
    {
        private readonly IOptions<MessageSettingsConfigurationOptions> _msgConfigOptions;
        private readonly IOptions<AzureKeyVaultOptions> _keyVaultOptions;
        private readonly IOptions<CustomProtocolOptions> _protocolOptions;

        public MessageInTransitService(IOptions<MessageSettingsConfigurationOptions> msgConfigOptions,
            IOptions<AzureKeyVaultOptions> keyVaultOptions,
            IOptions<CustomProtocolOptions> protocolOptions)
        {
            _msgConfigOptions = msgConfigOptions;
            _keyVaultOptions = keyVaultOptions;
            _protocolOptions = protocolOptions;
        }

        public ExternalMessage Bind(ExternalMessage message)
        {
            message.SignDataKeyId = _protocolOptions.Value.SignDataKeyId;
            message.CryptoEndpoint = _protocolOptions.Value.CryptoEndpoint;
            message.CertificateVaultUri = _keyVaultOptions.Value.VaultUri;
            message.CertificateKey = _keyVaultOptions.Value.DefaultCertificateName;
            message.SignDataKeyId = _protocolOptions.Value.SignDataKeyId;
            message.CryptoEndpoint = _protocolOptions.Value.CryptoEndpoint;
            var msgAttr = message.GetType().GetCustomAttribute(typeof(MessageMetadaAttribute));
            if (msgAttr is MessageMetadaAttribute msgAttribute && !string.IsNullOrEmpty(msgAttribute.QueueName))
            {
                message.MsgQueueName = msgAttribute.QueueName;
                message.AckQueueName = _msgConfigOptions.Value.Sender
                                    .SelectMany(sender => sender.MessageInTransitOptions)
                                    .FirstOrDefault(option => option.MsgQueueName.ToLower() == msgAttribute.QueueName.ToLower())?.AckQueueName
                                    ?? string.Empty;
                message.MsgDecryptScope = msgAttribute.DecryptScope;
                message.MsgQueueEndpoint = _msgConfigOptions.Value.Sender
                                    .SelectMany(sender=> sender.MessageInTransitOptions)
                                    .FirstOrDefault(option=> option.MsgQueueName.ToLower() == msgAttribute.QueueName.ToLower())?.MsgQueueEndpoint 
                                    ?? string.Empty;
            }
            else
            {
                throw new InvalidOperationException("Message missing attrbiute to specify the queue");
            }
            return message;
        }
    }
}
