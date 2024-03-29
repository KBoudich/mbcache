﻿using System;

namespace MbCache.Logic
{
	[Serializable]
	public class ComponentType
	{
		public ComponentType(Type concreteType, string typeAsCacheKeyString)
		{
			ConcreteType = concreteType;
			TypeAsCacheKeyString = typeAsCacheKeyString ?? concreteType.ToString();
		}

		public Type ConcreteType { get; private set; }
		public Type ConfiguredType { get; internal set; }
		public string TypeAsCacheKeyString { get; private set; }

		public override string ToString()
		{
			return TypeAsCacheKeyString;
		}
	}
}