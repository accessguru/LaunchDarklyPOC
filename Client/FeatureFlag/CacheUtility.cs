using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;

namespace BlazorApp2.Client.FeatureFlag
{
    public class CacheUtility
    {
        #region Private Fields

        private static readonly ConcurrentDictionary<string, AsyncLock> KeyLocks = new();
        private static readonly MemoryCache MemoryCache = new(new MemoryCacheOptions());

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Gets the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns></returns>
        public static object Get(string cacheKey)
        {
            return MemoryCache.TryGetValue(cacheKey, out var result) ? result : null;
        }

        /// <summary>
        /// Gets the specified cache key. This method call will add all cache items with 'Never Remove' priority. Once retrieved, cache items will not expire.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheMethod">The cache method.</param>
        /// <returns>The cached object.</returns>
        public static TResult Get<TResult>(string cacheKey, Func<TResult> cacheMethod)
        {
            return Get(cacheKey, cacheMethod, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
        }

        /// <summary>
        /// Gets the specified cache key.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheMethod">The cache method.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">cacheMethod</exception>
        /// <exception cref="System.ArgumentNullException">cacheMethod</exception>
        public static TResult Get<TResult>(string cacheKey, Func<TResult> cacheMethod, MemoryCacheEntryOptions options)
        {
            if (cacheMethod == null)
            {
                throw new ArgumentNullException(nameof(cacheMethod));
            }

            if (!(Get(cacheKey) is TResult value))
            {
                lock (string.Intern(cacheKey))
                {
                    var getResult = Get(cacheKey);

                    // Object exists in cache
                    if (getResult != null)
                    {
                        value = getResult is TResult result ? result : default;
                    }
                    // Fetch object and add to cache
                    else
                    {
                        var cacheResult = cacheMethod();

                        // Object exists in user-defined function
                        if (cacheResult != null)
                        {
                            value = cacheResult;
                            MemoryCache.Set(cacheKey, value, options);
                        }
                        else
                        {
                            value = default;
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheMethod">The cache method.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns>The cached object.</returns>
        public static TResult Get<TResult>(string cacheKey, Func<TResult> cacheMethod, DateTimeOffset expiration)
        {
            return Get(cacheKey, cacheMethod, new MemoryCacheEntryOptions { AbsoluteExpiration = expiration });
        }

        /// <summary>
        /// Gets the specified cache key. This method call will add all cache items with 'Never Remove' priority. Once retrieved, cache items will not expire.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheMethod">The cache method.</param>
        /// <returns></returns>
        public static async Task<TResult> GetAsync<TResult>(string cacheKey, Func<Task<TResult>> cacheMethod)
        {
            return await GetAsync(cacheKey, cacheMethod, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
        }

        /// <summary>
        /// Gets the specified cache key.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheMethod">The cache method.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">cacheMethod</exception>
        /// <exception cref="System.ArgumentNullException">cacheMethod</exception>
        public static async Task<TResult> GetAsync<TResult>(string cacheKey, Func<Task<TResult>> cacheMethod, MemoryCacheEntryOptions options)
        {
            if (cacheMethod == null)
            {
                throw new ArgumentNullException(nameof(cacheMethod));
            }

            if (Get(cacheKey) is not TResult value)
            {
                var keyLock = KeyLocks.GetOrAdd(cacheKey, new AsyncLock());

                using (await keyLock.LockAsync())
                {
                    var getResult = Get(cacheKey);

                    // Object exists in cache
                    if (getResult != null)
                    {
                        value = getResult is TResult result ? result : default;
                    }
                    // Fetch object and add to cache
                    else
                    {
                        var cacheResult = await cacheMethod();

                        // Object exists in user-defined function
                        if (cacheResult != null)
                        {
                            value = cacheResult;
                            MemoryCache.Set(cacheKey, value, options);
                        }
                        else
                        {
                            value = default;
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheMethod">The cache method.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        public static async Task<TResult> GetAsync<TResult>(string cacheKey, Func<Task<TResult>> cacheMethod, DateTimeOffset expiration)
        {
            return await GetAsync(cacheKey, cacheMethod, new MemoryCacheEntryOptions { AbsoluteExpiration = expiration });
        }

        /// <summary>
        /// Removes the specified cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        public static void Remove(string cacheKey) => MemoryCache.Remove(cacheKey);

        /// <summary>
        /// Updates the specified cache key. No Expiration.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="value">The value.</param>
        public static void Update(string cacheKey, object value) => MemoryCache.Set(cacheKey, value);

        /// <summary>
        /// Updates the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiration">The expiration.</param>
        public static void Update(string cacheKey, object value, DateTimeOffset expiration) => MemoryCache.Set(cacheKey, value, expiration);

        /// <summary>
        /// Updates the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The options.</param>
        public static void Update(string cacheKey, object value, MemoryCacheEntryOptions options) => MemoryCache.Set(cacheKey, value, options);

        #endregion Public Methods
    }
}