namespace SpaceEngineers.Core.IntegrationTransport.Internals
{
    using System;
    using System.Runtime.Caching;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class RpcRequestRegistry : IRpcRequestRegistry
    {
        private readonly MemoryCache _memoryCache;
        private readonly ISettingsProvider<IntegrationTransportSettings> _integrationTransportSettings;

        public RpcRequestRegistry(ISettingsProvider<IntegrationTransportSettings> integrationTransportSettings)
        {
            _memoryCache = new MemoryCache(nameof(RpcRequestRegistry));
            _integrationTransportSettings = integrationTransportSettings;
        }

        public async Task<bool> TryEnroll<TReply>(Guid requestId, TaskCompletionSource<TReply> tcs, CancellationToken token)
            where TReply : IIntegrationMessage
        {
            var settings = await _integrationTransportSettings
                .Get(token)
                .ConfigureAwait(false);

            var cacheItem = new CacheItem(requestId.ToString(), tcs);

            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(settings.RpcRequestTimeout),
                RemovedCallback = RemovedCallback(tcs)
            };

            return _memoryCache.Add(cacheItem, cacheItemPolicy);
        }

        public bool TrySetResult<TReply>(Guid requestId, TReply reply)
            where TReply : IIntegrationMessage
        {
            var cacheItem = _memoryCache.GetCacheItem(requestId.ToString());

            return cacheItem?.Value is TaskCompletionSource<TReply> tcs
                   && tcs.TrySetResult(reply);
        }

        private static CacheEntryRemovedCallback RemovedCallback<TReply>(TaskCompletionSource<TReply> tcs)
            where TReply : IIntegrationMessage
        {
            return _ => tcs.TrySetCanceled();
        }
    }
}