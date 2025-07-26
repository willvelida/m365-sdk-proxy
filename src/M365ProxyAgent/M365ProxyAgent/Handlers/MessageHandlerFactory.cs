using M365ProxyAgent.Interfaces;
using Microsoft.Agents.Core.Models;

namespace M365ProxyAgent.Handlers
{
    /// <summary>
    /// Default implementation of the message handler factory.
    /// </summary>
    public class MessageHandlerFactory(IServiceProvider serviceProvider) : IMessageHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public IMessageHandler? CreateHandler(string activityType)
        {
            ArgumentNullException.ThrowIfNull(activityType);

            return activityType switch
            {
                ActivityTypes.ConversationUpdate => _serviceProvider.GetService<WelcomeMessageHandler>(),
                ActivityTypes.Message => _serviceProvider.GetService<RegularMessageHandler>(),
                _ => null
            };
        }
    }
}
