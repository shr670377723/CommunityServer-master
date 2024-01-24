#if NET6_0_OR_GREATER
using System;
using System.Diagnostics;

using Microsoft.Extensions.Caching.Memory;

namespace AppLimit.CloudComputing.SharpBox.Common.Cache
{
    public sealed class CachedDictionary<T> : CachedDictionaryBase<T>
    {
        private readonly TimeSpan _absoluteExpirationPeriod;

        private DateTime AbsoluteExpiration
        {
            get
            {
                if (_absoluteExpirationPeriod == TimeSpan.Zero)
                    return DateTime.MaxValue;
                return DateTime.Now + _absoluteExpirationPeriod;
            }
        }

        private TimeSpan SlidingExpiration { get; set; }
        private MemoryCache MemoryCache { get; set; }

        public CachedDictionary(string baseKey, TimeSpan absoluteExpirationPeriod, TimeSpan slidingExpiration, Func<T, bool> cacheCodition)
        {
            MemoryCache = new MemoryCache(new MemoryCacheOptions());
            _baseKey = baseKey;
            _absoluteExpirationPeriod = absoluteExpirationPeriod;
            SlidingExpiration = slidingExpiration;
            _cacheCodition = cacheCodition ?? throw new ArgumentNullException("cacheCodition");
            InsertRootKey(_baseKey);
        }

        public CachedDictionary(string baseKey)
            : this(baseKey, x => true)
        {
        }

        public CachedDictionary(string baseKey, Func<T, bool> cacheCodition)
            : this(baseKey, TimeSpan.Zero, TimeSpan.Zero, cacheCodition)
        {
        }

        protected override void InsertRootKey(string rootKey)
        {
#if (DEBUG)
            Debug.Print("inserted root key {0}", rootKey);
#endif
            MemoryCache.Remove(rootKey);
            MemoryCache.Set(rootKey, DateTime.UtcNow.Ticks, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });
        }

        public override void Reset(string rootKey, string key)
        {
            MemoryCache.Remove(BuildKey(key, rootKey));
        }

        protected override object GetObjectFromCache(string fullKey)
        {
            return MemoryCache.Get(fullKey);
        }

        public override void Add(string rootkey, string key, T newValue)
        {
            var builtrootkey = BuildKey(string.Empty, string.IsNullOrEmpty(rootkey) ? "root" : rootkey);
            var options =  new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.Normal,
                    AbsoluteExpiration = AbsoluteExpiration
                };
            
            if (SlidingExpiration != TimeSpan.Zero)
            {
                options.SlidingExpiration = SlidingExpiration;
            }

            if (!MemoryCache.TryGetValue(builtrootkey, out _))
            {
#if (DEBUG)
                Debug.Print("added root key {0}", builtrootkey);
#endif
                //Insert root if no present
                MemoryCache.Remove(builtrootkey);
                MemoryCache.Set(builtrootkey, DateTime.UtcNow.Ticks, options);

                MemoryCache.Remove(BuildKey(key, rootkey));
            }

            if (newValue != null)
            {
                var buildKey = BuildKey(key, rootkey);
                MemoryCache.Remove(buildKey);
                
                MemoryCache.Set(BuildKey(key, rootkey), newValue, options);
                //TODO
                //options.AddExpirationToken(Microsoft.Extensions.Primitives.CancellationChangeToken);
                //new CacheDependency(null, new[] { _baseKey, builtrootkey }),
            }
            else
            {
                MemoryCache.Remove(BuildKey(key, rootkey)); //Remove if null
            }
        }
    }
}
#endif

#if NET48
using System;
using System.Diagnostics;
using System.Web;
using System.Web.Caching;

namespace AppLimit.CloudComputing.SharpBox.Common.Cache
{
    public sealed class CachedDictionary<T> : CachedDictionaryBase<T>
    {
        private readonly TimeSpan _absoluteExpirationPeriod;

        private DateTime AbsoluteExpiration
        {
            get
            {
                if (_absoluteExpirationPeriod == TimeSpan.Zero)
                    return DateTime.MaxValue;
                return DateTime.Now + _absoluteExpirationPeriod;
            }
        }

        private TimeSpan SlidingExpiration { get; set; }

        public CachedDictionary(string baseKey, TimeSpan absoluteExpirationPeriod, TimeSpan slidingExpiration, Func<T, bool> cacheCodition)
        {
            if (cacheCodition == null) throw new ArgumentNullException("cacheCodition");
            _baseKey = baseKey;
            _absoluteExpirationPeriod = absoluteExpirationPeriod;
            SlidingExpiration = slidingExpiration;
            _cacheCodition = cacheCodition;
            InsertRootKey(_baseKey);
        }

        public CachedDictionary(string baseKey)
            : this(baseKey, x => true)
        {
        }

        public CachedDictionary(string baseKey, Func<T, bool> cacheCodition)
            : this(baseKey, TimeSpan.Zero, TimeSpan.Zero, cacheCodition)
        {
        }

        protected override void InsertRootKey(string rootKey)
        {
#if (DEBUG)
            Debug.Print("inserted root key {0}", rootKey);
#endif
            HttpRuntime.Cache.Remove(rootKey);
            HttpRuntime.Cache.Insert(rootKey, DateTime.UtcNow.Ticks, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration,
                                     CacheItemPriority.NotRemovable, (key, value, reason) => Debug.Print("gloabl root key: {0} removed. reason: {1}", key, reason));
        }

        public override void Reset(string rootKey, string key)
        {
            HttpRuntime.Cache.Remove(BuildKey(key, rootKey));
        }

        protected override object GetObjectFromCache(string fullKey)
        {
            return HttpRuntime.Cache.Get(fullKey);
        }

        public override void Add(string rootkey, string key, T newValue)
        {
            var builtrootkey = BuildKey(string.Empty, string.IsNullOrEmpty(rootkey) ? "root" : rootkey);
            if (HttpRuntime.Cache[builtrootkey] == null)
            {
#if (DEBUG)
                Debug.Print("added root key {0}", builtrootkey);
#endif
                //Insert root if no present
                HttpRuntime.Cache.Remove(builtrootkey);
                HttpRuntime.Cache.Insert(builtrootkey, DateTime.UtcNow.Ticks, null, AbsoluteExpiration, SlidingExpiration,
                                         CacheItemPriority.NotRemovable, (removedkey, value, reason) => Debug.Print("root key: {0} removed. reason: {1}", removedkey, reason));
            }

            CacheItemRemovedCallback removeCallBack = ItemRemoved;

            if (newValue != null)
            {
                var buildKey = BuildKey(key, rootkey);
                HttpRuntime.Cache.Remove(buildKey);
                HttpRuntime.Cache.Insert(BuildKey(key, rootkey), newValue,
                                         new CacheDependency(null, new[] { _baseKey, builtrootkey }),
                                         AbsoluteExpiration, SlidingExpiration,
                                         CacheItemPriority.Normal, removeCallBack);
            }
            else
            {
                HttpRuntime.Cache.Remove(BuildKey(key, rootkey)); //Remove if null
            }
        }

        private static void ItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            Debug.Print("key: {0} removed. reason: {1}", key, reason);
        }
    }
}
#endif