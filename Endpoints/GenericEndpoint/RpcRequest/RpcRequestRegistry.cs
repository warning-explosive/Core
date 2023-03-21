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
        private readonly GenericEndpointSettings _genericEndpointSettings;

        private readonly MemoryCache _memoryCache;

        public RpcRequestRegistry(ISettingsProvider<GenericEndpointSettings> genericEndpointSettingsProvider)
        {
            _genericEndpointSettings = genericEndpointSettingsProvider.Get();

            _memoryCache = new MemoryCache(nameof(RpcRequestRegistry));
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }

        public async Task<IntegrationMessage> Enroll(Guid requestId, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<IntegrationMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

            var cacheItem = new CacheItem(requestId.ToString(), tcs);

            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(_genericEndpointSettings.RpcRequestTimeout),
                RemovedCallback = RemovedCallback(tcs, requestId, _genericEndpointSettings)
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