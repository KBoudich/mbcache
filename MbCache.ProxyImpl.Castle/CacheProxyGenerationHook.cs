﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;

namespace MbCache.ProxyImpl.Castle
{
    public class CacheProxyGenerationHook : IProxyGenerationHook
    {
        private readonly IEnumerable<MethodInfo> _methods;

        public CacheProxyGenerationHook(IEnumerable<MethodInfo> methods)
        {
            _methods = methods;
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            return isMethodMarkedForCaching(methodInfo);
        }

        public void NonVirtualMemberNotification(Type type, MemberInfo memberInfo)
        {
        }

        public void MethodsInspected()
        {
        }

        private bool isMethodMarkedForCaching(MethodInfo key)
        {
            return _methods.Contains(key);
        }

        public override bool Equals(object obj)
        {
            var casted = obj as CacheProxyGenerationHook;
            return casted != null && casted._methods.Equals(_methods);
        }

        public override int GetHashCode()
        {
            return _methods.GetHashCode();
        }
    }
}