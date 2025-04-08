using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Text.Json;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Models;
using System.Linq;

namespace UNOPS.PAO.UNOPSBusiness.Services
{
    public class PubSubPullService : BackgroundService
    {
        private readonly ILogger<PubSubPullService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string ProjectId;
        private readonly string SubscriptionId;
        private readonly IDbContextFactory<UNOPSAppDbContext> _dbContextFactory;

        public PubSubPullService(ILogger<PubSubPullService> logger, IConfiguration configuration, IDbContextFactory<UNOPSAppDbContext> dbContextFactory)
        {
            _logger = logger;
            _configuration = configuration;
            ProjectId = configuration.GetSection("PubSub")["ProjectId"];
            SubscriptionId = configuration.GetSection("PubSub")["SubscriptionId"];
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, SubscriptionId);
            var subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            _logger.LogInformation("Pub/Sub Pull Service started. Listening for messages...");

            // Start receiving messages
            await subscriber.StartAsync((PubsubMessage message, CancellationToken ct) =>
            {
                return Task.Run(async () =>
                {
                    if (ct.IsCancellationRequested)
                    {
                        return SubscriberClient.Reply.Nack;
                    }

                    try
                    {
                        // Convert the message data from byte array to string
                        string messageText = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());

                        List<MyPubSubMessage>? messages = JsonSerializer.Deserialize<List<MyPubSubMessage>>(messageText);

                        if (messages != null)
                        {
                            foreach (var msg in messages)
                            {
                                // Use the factory to create a new DbContext instance
                                using (var dbContext = _dbContextFactory.CreateDbContext())
                                {
                                    var contextService = new AiContextualService(_configuration, dbContext, null);
                                    await contextService.GenerateEmbeddingAsync(msg.EntityName, msg.EntityId, msg.Content);
                                }
                            }
                        }

                        return SubscriberClient.Reply.Ack;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message: {ex.Message}");
                        return SubscriberClient.Reply.Ack;
                    }
                }, ct);
            });

            // Keep the service running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
