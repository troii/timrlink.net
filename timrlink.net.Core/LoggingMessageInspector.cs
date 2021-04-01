using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace timrlink.net.Core
{
    internal class LoggingMessageInspector : IClientMessageInspector
    {
        private readonly ILogger<LoggingMessageInspector> logger;

        public LoggingMessageInspector(ILogger<LoggingMessageInspector> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var correlationState = Guid.NewGuid();
            logger.LogTrace("{0}: {1}", correlationState, channel.Via);
            using (var buffer = request.CreateBufferedCopy(int.MaxValue))
            {
                var document = GetDocument(buffer.CreateMessage());
                logger.LogTrace("{0}: {1}", correlationState, document.OuterXml);

                request = buffer.CreateMessage();
                return correlationState;
            }
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            using (var buffer = reply.CreateBufferedCopy(int.MaxValue))
            {
                var document = GetDocument(buffer.CreateMessage());
                logger.LogTrace("{0}: {1}", correlationState, document.OuterXml);

                reply = buffer.CreateMessage();
            }
        }

        private static XmlDocument GetDocument(Message request)
        {
            var document = new XmlDocument();
            using (var memoryStream = new MemoryStream())
            {
                // write request to memory stream
                var writer = XmlWriter.Create(memoryStream);
                request.WriteMessage(writer);
                writer.Flush();
                memoryStream.Position = 0;

                // load memory stream into a document
                document.Load(memoryStream);
            }

            return document;
        }
    }
}
