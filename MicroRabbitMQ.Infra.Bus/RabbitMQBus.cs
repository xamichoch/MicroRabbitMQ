﻿using MediatR;
using MicroRabbitMQ.Domain.Core.Bus;
using MicroRabbitMQ.Domain.Core.Commands;
using MicroRabbitMQ.Domain.Core.Events;
using MicroRabbitMQ.Transfer.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroRabbitMQ.Infra.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQBus(IMediator mediator, IServiceScopeFactory serviceScopeFactory)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }
        
        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }
        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = "192.168.99.100",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                ContinuationTimeout = new TimeSpan(20, 0, 0, 0)
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var eventName = @event.GetType().Name;
            channel.QueueDeclare(eventName, false, false, false, null);
            var message = System.Text.Json.JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("", eventName, null, body);
        }


        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(s => GetType() == handlerType))
            {
                throw new ArgumentException(
                    $"Handler type {handlerType.Name} " +
                    $"is already registered for '{eventName}'");
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();

        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = "192.168.99.100",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                ContinuationTimeout = new TimeSpan(20, 0, 0, 0),
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body);
            
            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var scope = _serviceScopeFactory.CreateScope();
                var subscriptions = _handlers[eventName];
                foreach (var subscription in subscriptions)
                {
                    var handler = scope.ServiceProvider.GetService(subscription);
                    if (handler == null) continue;
                    var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                    //var @event = JsonSerializer.Deserialize(message, eventType);
                    var @event = JsonConvert.DeserializeObject(message, eventType);
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }
            }
        }
    }
}
