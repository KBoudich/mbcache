using System;
using System.Linq;
using System.Reflection;
using LinFu.DynamicProxy;
using MbCache.Configuration;
using MbCache.Core;
using MbCache.Core.Events;
using MbCache.Logic;

namespace MbCache.ProxyImpl.LinFu
{
	public class CacheInterceptor : IInterceptor
	{
		[NonSerialized]
		private static readonly MethodInfo exceptionInternalPreserveStackTrace =
			typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

		private readonly CacheAdapter _cache;
		private readonly ICacheKey _cacheKey;
		private readonly ILockObjectGenerator _lockObjectGenerator;
		private readonly ConfigurationForType _configurationForType;
		private readonly object _target;
		private readonly ICachingComponent _cachingComponent;

		public CacheInterceptor(CacheAdapter cache,
										ICacheKey cacheKey,
										ILockObjectGenerator lockObjectGenerator, 
										ConfigurationForType configurationForType,
										object target)
		{
			_cache = cache;
			_cacheKey = cacheKey;
			_lockObjectGenerator = lockObjectGenerator;
			_configurationForType = configurationForType;
			_target = target;
			_cachingComponent = new CachingComponent(cache, cacheKey, configurationForType);
		}

		public object Intercept(InvocationInfo info)
		{
			var realTargetMethod = createGenericMethodInfoIfNeeded(info.TargetMethod, info.TypeArguments);
			return methodMarkedForCaching(realTargetMethod) ?
					  interceptUsingCache(realTargetMethod, info.Arguments) : 
					  callOriginalMethod(realTargetMethod, info.Arguments);
		}

		private static MethodInfo createGenericMethodInfoIfNeeded(MethodInfo orgMethodInfo, Type[] typeArguments)
		{
			return orgMethodInfo.ContainsGenericParameters ? 
				orgMethodInfo.MakeGenericMethod(typeArguments) : 
				orgMethodInfo;
		}

		private bool methodMarkedForCaching(MethodInfo method)
		{
			return _configurationForType.CachedMethods.Contains(method, MethodInfoComparer.Instance) &&
								_configurationForType.EnabledCache;
		}

		private object interceptUsingCache(MethodInfo method, object[] arguments)
		{
			var key = _cacheKey.Key(_configurationForType.ComponentType, _cachingComponent, method, arguments);
			var eventInformation = new EventInformation(key, _configurationForType.ComponentType.ConfiguredType, method, arguments);
			if (key == null)
			{
				return callOriginalMethod(method, arguments);
			}
			var cachedValue = _cache.Get(eventInformation);
			if (cachedValue != null)
			{
				return cachedValue;
			}
			var lockObject = _lockObjectGenerator.GetFor(key);
			if (lockObject == null)
			{
				return executeAndPutInCache(method, arguments, eventInformation);
			}
			lock (lockObject)
			{
				var cachedValue2 = _cache.Get(eventInformation);
				return cachedValue2 ?? executeAndPutInCache(method, arguments, eventInformation);
			}
		}

		private object executeAndPutInCache(MethodInfo method, object[] arguments, EventInformation eventInformation)
		{
			var retVal = callOriginalMethod(method, arguments);
			_cache.Put(new CachedItem(eventInformation, retVal));
			return retVal;
		}

		private object callOriginalMethod(MethodInfo method, object[] arguments)
		{
			return invokeMethod(method.DeclaringType == typeof(ICachingComponent) ? _cachingComponent : _target, method, arguments);
		}

		private static object invokeMethod(object target, MethodInfo method, object[] arguments)
		{
			try
			{
				return method.Invoke(target, arguments);
			}
			catch (TargetInvocationException ex)
			{
				exceptionInternalPreserveStackTrace.Invoke(ex.InnerException, new Object[] { });
				throw ex.InnerException;
			}
		}
	}
}