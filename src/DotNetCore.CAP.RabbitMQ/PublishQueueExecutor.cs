﻿using System;
using System.Text;
using System.Threading.Tasks;
using DotNetCore.CAP.Processor.States;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DotNetCore.CAP.RabbitMQ
{
    internal sealed class PublishQueueExecutor : BasePublishQueueExecutor
    {
        private readonly ConnectionPool _connectionPool;
        private readonly ILogger _logger;
        private readonly RabbitMQOptions _rabbitMQOptions;

        public PublishQueueExecutor(
            CapOptions options,
            IStateChanger stateChanger,
            ConnectionPool connectionPool,
            RabbitMQOptions rabbitMQOptions,
            ILogger<PublishQueueExecutor> logger)
            : base(options, stateChanger, logger)
        {
            _logger = logger;
            _connectionPool = connectionPool;
            _rabbitMQOptions = rabbitMQOptions;
        }

        public override Task<OperateResult> PublishAsync(string keyName, string content)
        {
            var connection = _connectionPool.Rent();

            try
            {
                using (var channel = connection.CreateModel())
                {
                    var body = Encoding.UTF8.GetBytes(content);

                    channel.ExchangeDeclare(_rabbitMQOptions.TopicExchangeName, RabbitMQOptions.ExchangeType, true);
                    channel.BasicPublish(_rabbitMQOptions.TopicExchangeName,
                        keyName,
                        null,
                        body);

                    _logger.LogDebug($"RabbitMQ topic message [{keyName}] has been published.");
                }
                return Task.FromResult(OperateResult.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"RabbitMQ topic message [{keyName}] has been raised an exception of sending. the exception is: {ex.Message}");

                return Task.FromResult(OperateResult.Failed(ex,
                    new OperateError
                    {
                        Code = ex.HResult.ToString(),
                        Description = ex.Message
                    }));
            }
            finally
            {
                _connectionPool.Return(connection);
            }
        }
    }
}