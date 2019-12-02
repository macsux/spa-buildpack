using System;
using EasyNetQ;
using SpaLauncher.SpringCloudBus.Messages;
using SpaLauncher.SpringCloudBus.Util;

namespace SpaLauncher.SpringCloudBus
{
    public class SpringMessageSerializationStrategy : IMessageSerializationStrategy
    {
        private readonly ITypeNameSerializer _typeNameSerializer;
        private readonly ISerializer _serializer;
        private readonly ICorrelationIdGenerationStrategy _correlationIdGenerator;

        public SpringMessageSerializationStrategy(ITypeNameSerializer typeNameSerializer,
            ISerializer serializer,
            ICorrelationIdGenerationStrategy correlationIdGenerator)
        {
            _typeNameSerializer = typeNameSerializer;
            _serializer = serializer;
            _correlationIdGenerator = correlationIdGenerator;
        }

        public SerializedMessage SerializeMessage(IMessage message)
        {
            var bytes = _serializer.MessageToBytes(message.MessageType, message.GetBody());
            var properties = message.Properties;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();
            properties.DeliveryMode = 2;
            if (message is ApplicationEvent eventMessage)
            {
                properties.Timestamp = eventMessage.TimeStamp.ToEpochMillisececonds();
            }
            else
            {
                properties.Timestamp = DateTime.UtcNow.ToEpochMillisececonds();
            }
            
            if (string.IsNullOrEmpty(properties.CorrelationId))
                properties.CorrelationId = _correlationIdGenerator.GetCorrelationId();
            return new SerializedMessage(properties, bytes);
        }

        public IMessage DeserializeMessage(MessageProperties properties, byte[] body)
        {
            var message = this._serializer.BytesToMessage(typeof(object), body);
            if (message == null)
                return null;
            return MessageFactory.CreateInstance(message.GetType(), message, properties);
        }
    }
}