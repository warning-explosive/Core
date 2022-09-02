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

        public async Task<IntegrationMessage> Enroll(Guid requestId, CancellationToken token)
        {
            var settings = await _genericEndpointSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var tcs = new TaskCompletionSource<IntegrationMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

            var cacheItem = new CacheItem(requestId.ToString(), tcs);

            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(settings.RpcRequestTimeout),
                RemovedCallback = RemovedCallback(tcs, requestId, settings)
            };

            if (!_memoryCache.Add(cacheItem, cacheItemPolicy))
            {
                tcs.TrySetException(new InvalidOperationException($"Rpc-request registry has already had enrolled request {requestId}"));
            }

            return await tcs.Task.ConfigureAwait(false);

            static CacheEntryRemovedCallback RemovedCallback(
                TaskCompletionSource<IntegrationMessage> tcs,
                Guid requestId,
                GenericEndpointSettings settings)
            {
                return args =>
                {
                    if (args.RemovedReason != CacheEntryRemovedReason.Removed)
                    {
                        tcs.TrySetException(new TimeoutException($"Rpc-request {requestId} was timed out of {settings.RpcRequestTimeout.TotalSeconds} seconds"));
                    }
                };
            }
        }

        public bool TrySetResult(Guid requestId, IntegrationMessage reply)
        {
            var cacheItem = _memoryCache.Remove(requestId.ToString());

            return cacheItem is TaskCompletionSource<IntegrationMessage> tcs
                && tcs.TrySetResult(reply);
        }

        public bool TrySetException(Guid requestId, Exception exception)
        {
            var cacheItem = _memoryCache.Remove(requestId.ToString());

            return cacheItem is TaskCompletionSource<IntegrationMessage> tcs
                && tcs.TrySetException(exception);
        }

        public bool TrySetCancelled(Guid requestId)
        {
            var cacheItem = _memoryCache.Remove(requestId.ToString());

            return cacheItem is TaskCompletionSource<IntegrationMessage> tcs
                && tcs.TrySetCanceled();
        }
    }
}