using System;

using Microsoft.Extensions.Caching.Memory;

namespace Waffler.Common.Util
{
    public class Cache
    {
        private const int ExpirationTime = 60 * 10;
        private MemoryCache MemoryCache { get; set; }

        public Cache()
        {
            MemoryCache = new MemoryCache(new MemoryCacheOptions
            {
            });
        }

        public void Clear()
        {
            MemoryCache = new MemoryCache(new MemoryCacheOptions
            {
            });
        }

        public T Get<T>(string key)
        {
            var cacheEntity = MemoryCache.Get<CacheEntity>(key);
            return (T)cacheEntity.Value;
        }

        public T Get<T, U>(string key, out U metaData)
        {
            var cacheEntity = MemoryCache.Get<CacheEntity>(key);
            if(cacheEntity != null)
            {
                metaData = cacheEntity.Metadata != null ? (U)cacheEntity.Metadata : default;
                return (T)cacheEntity.Value;
            }

            metaData = default;
            return default;
        }

        public void Set(string key, object value, object cacheMetadata = null)
        {
            var cacheEntity = new CacheEntity()
            {
                Value = value,
                Metadata = cacheMetadata
            };
            MemoryCache.Set(key, cacheEntity, DateTime.UtcNow.AddSeconds(ExpirationTime));
        }

        public int GetEntriesCount()
        {
            return MemoryCache.Count;
        }

        private class CacheEntity
        {
            public object Value { get; set; }
            public object Metadata { get; set; }
        }
    }
}