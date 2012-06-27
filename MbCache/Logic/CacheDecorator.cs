using System.Threading;
using log4net;
using MbCache.Configuration;
using MbCache.Core;

namespace MbCache.Logic
{
	/// <summary>
	/// Adds logging, statistics and null replacement for <see cref="ICache"/>.
	/// </summary>
	public class CacheDecorator : ICache, IStatistics
	{
		private readonly ILog _log;
		private readonly ICache _cache;
		private long _cacheHits;
		private long _physicalCacheMisses;

		public CacheDecorator(ICache cache)
		{
			_cache = cache;
			_log = LogManager.GetLogger(cache.GetType());
		}

		public long CacheHits
		{
			get { return _cacheHits; }
		}

		public long CacheMisses
		{
			get
			{
				return _physicalCacheMisses;
			}
		}

		public object Get(string key)
		{
			if (_log.IsDebugEnabled)
			{
				_log.DebugFormat("Trying to find cache entry <{0}>", key);				
			}
			var cacheValue = _cache.Get(key);
			if(cacheValue == null)
			{
				cacheMiss(key);
			}
			else
			{
				cacheHit(key);
			}
			return cacheValue is nullValue ? null : cacheValue;
		}

		public void Put(string key, object value)
		{
			if (_log.IsDebugEnabled)
			{
				_log.DebugFormat("Adding cache entry for <{0}>", key);				
			}
			//creating new nullValue instance here - not really necessary with current aspnetcache impl
			//but gives a possibility for ICache implementations to use call backs
			_cache.Put(key, value ?? new nullValue());
		}

		public void Delete(string keyStartingWith)
		{
			if (keyStartingWith != null)
			{
				if (_log.IsDebugEnabled)
				{
					_log.DebugFormat("Removing cache entries starting with {0}", keyStartingWith);					
				}
				_cache.Delete(keyStartingWith);				
			}
		}

		public void Clear()
		{
			_physicalCacheMisses = 0;                
			_cacheHits = 0;
		}

		private void cacheHit(string key)
		{
			if (_log.IsDebugEnabled)
			{
				_log.DebugFormat("Cache hit for <{0}>", key);				
			}
			Interlocked.Increment(ref _cacheHits);
		}

		private void cacheMiss(string key)
		{
			if (_log.IsDebugEnabled)
			{
				_log.DebugFormat("Cache miss for <{0}>", key);				
			}
			Interlocked.Increment(ref _physicalCacheMisses);
		}

		private class nullValue { }
	}
}