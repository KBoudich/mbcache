﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace MbCache.Logic
{
	[Serializable]
	public class ConfigurationForType
	{
		public ConfigurationForType(Type clrType, string typeAsCacheKeyString)
		{
			ComponentType = new ComponentType(clrType, typeAsCacheKeyString);
			CachedMethods = new HashSet<MethodInfo>();
			EnabledCache = true;
		}

		public bool EnabledCache { get; set; }
		public ComponentType ComponentType { get; private set; }
		public ICollection<MethodInfo> CachedMethods { get; private set; }
		public bool CachePerInstance { get; set; }
	}
}