using Google.Cloud.PubSub.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSBusiness.Models;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PubSubPublisher
    {
        private readonly string _projectId;
        private readonly string _topicId;

        public PubSubPublisher(string projectId, string topicId)
        {
            _projectId = projectId;
            _topicId = topicId;
        }

        public async Task PublishMessageAsync(List<MyPubSubMessage> messages)
        {
            try
            {
                TopicName topicName = TopicName.FromProjectTopic(_projectId, _topicId);
                PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

                // Convert messages to JSON string
                string messageJson = JsonSerializer.Serialize(messages);
                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(messageJson);

                PubsubMessage pubsubMessage = new PubsubMessage
                {
                    Data = Google.Protobuf.ByteString.CopyFrom(messageBytes)
                };

                string messageId = await publisher.PublishAsync(pubsubMessage);
                Console.WriteLine($"Published message with ID: {messageId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
            }
        }
    }
}