using System;
using System.Diagnostics;

using Microsoft.Extensions.Caching.Memory;

namespace Waffler.Common.Util
{
    public class Cache
    {
        private const int ExpirationTime = 60 * 10;
        private MemoryCache _cache { get; set; }

        public Cache()
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
            });
        }

        public void Clear()
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
            });
        }

        public T Get<T>()
        {
            return Get<T>(GetCallingMethodName());
        }

        public T Get<T>(string key)
        {
            var cacheEntity =  _cache.Get<CacheEntity>(key);
            return (T)cacheEntity.Value;
        }

        public T Get<T, U>(out U metaData)
        {
            return Get<T, U>(GetCallingMethodName(), out metaData);
        }

        public T Get<T, U>(string key, out U metaData)
        {
            var cacheEntity = _cache.Get<CacheEntity>(key);
            if(cacheEntity != null)
            {
                metaData = cacheEntity.Metadata != null ? (U)cacheEntity.Metadata : default;
                return (T)cacheEntity.Value;
            }

            metaData = default;
            return default;
        }

        public void Set(object value, object cacheMetadata = null)
        {
            Set(GetCallingMethodName(), value, cacheMetadata);
        }

        public void Set(string key, object value, object cacheMetadata = null)
        {
            var cacheEntity = new CacheEntity()
            {
                Value = value,
                Metadata = cacheMetadata
            };
            _cache.Set(key, cacheEntity, DateTime.UtcNow.AddSeconds(ExpirationTime));
        }

        private string GetCallingMethodName()
        {
            var stack = new StackTrace();
            var frame = stack.GetFrame(2);
            return frame.GetMethod().Name;
        }

        private class CacheEntity
        {
            public object Value { get; set; }
            public object Metadata { get; set; }
        }
    }
}