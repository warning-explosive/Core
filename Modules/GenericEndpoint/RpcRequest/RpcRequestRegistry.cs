namespace SpaceEngineers.Core.GenericEndpoint.RpcRequest
{
    using System;
    using System.Runtime.Caching;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using Settings;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    [ManuallyRegisteredComponent("Should be unique per single generic host")]
    internal class RpcRequestRegistry : IRpcRequestRegistry,
                                        IDisposable,
                                        IResolvable<IRpcRequestRegistry>
    {
        private readonly MemoryCache _memoryCache;
        private readonly ISettingsProvider<GenericEndpointSettings> _genericEndpointSettingsProvider;

        public RpcRequestRegistry(ISettingsProvider<GenericEndpointSettings> genericEndpointSettingsProvider)
        {
            _memoryCache = new MemoryCache(nameof(RpcRequestRegistry));
            _genericEndpointSettingsProvider = genericEndpointSettingsProvider;
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }

        public async Task<bool> TryEnroll(Guid requestId, TaskCompletionSource<IntegrationMessage> tcs, CancellationToken token)
        {
            var settings = await _genericEndpointSettingsProvider
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

        public bool TrySetCancelled(Guid requestId)
        {
            var cacheItem = _memoryCache.GetCacheItem(requestId.ToString());

            return cacheItem?.Value is TaskCompletionSource<IntegrationMessage> tcs
                && tcs.TrySetCanceled();
        }
    }
}