namespace SpaceEngineers.Core.IntegrationTransport.RpcRequest
{
    using System;
    using System.Runtime.Caching;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;
    using GenericEndpoint.Messaging;
    using Settings;

    [Component(EnLifestyle.Singleton)]
    internal class RpcRequestRegistry : IRpcRequestRegistry,
                                        IDisposable,
                                        IResolvable<IRpcRequestRegistry>
    {
        private readonly MemoryCache _memoryCache;
        private readonly ISettingsProvider<IntegrationTransportSettings> _integrationTransportSettings;

        public RpcRequestRegistry(ISettingsProvider<IntegrationTransportSettings> integrationTransportSettings)
        {
            _memoryCache = new MemoryCache(nameof(RpcRequestRegistry));
            _integrationTransportSettings = integrationTransportSettings;
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }

        public async Task<bool> TryEnroll(Guid requestId, TaskCompletionSource<IntegrationMessage> tcs, CancellationToken token)
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

            static CacheEntryRemovedCallback RemovedCallback(TaskCompletionSource<IntegrationMessage> tcs)
            {
                return _ => tcs.TrySetCanceled();
            }
        }

        public bool TrySetResult(Guid requestId, IntegrationMessage reply)
        {
            var cacheItem = _memoryCache.GetCacheItem(requestId.ToString());

            return cacheItem?.Value is TaskCompletionSource<IntegrationMessage> tcs
                   && tcs.TrySetResult(reply);
        }
    }
}