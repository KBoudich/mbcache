using System;
using MbCacheTest.TestData;
using NUnit.Framework;

namespace MbCacheTest.Configuration
{
	public class CacheBuilderTest : TestBothProxyFactories
	{
		public CacheBuilderTest(string proxyTypeString) : base(proxyTypeString)
		{
		}

		[Test]
		public void OnlyDeclareTypeOnce()
		{
			CacheBuilder
				 .For<ObjectReturningNewGuids>()
				 .CacheMethod(c => c.CachedMethod())
				 .As<IObjectReturningNewGuids>();
			Assert.Throws<ArgumentException>(()
														=>
														CacheBuilder
															.For<ObjectReturningNewGuids>()
															.CacheMethod(c => c.CachedMethod2())
															.As<IObjectReturningNewGuids>());

		}

		[Test]
		public void ReturnTypeMustBeDeclared()
		{
			CacheBuilder
				 .For<ObjectReturningNewGuids>()
				 .CacheMethod(c => c.CachedMethod());
			Assert.Throws<InvalidOperationException>(() => CacheBuilder.BuildFactory());
		}

		[Test]
		public void CreatingProxiesOfSameDeclaredTypeShouldReturnIdenticalTypes()
		{
			CacheBuilder
				 .For<ObjectReturningNewGuids>()
				 .CacheMethod(c => c.CachedMethod())
				 .As<IObjectReturningNewGuids>();

			var factory = CacheBuilder.BuildFactory();

			Assert.AreEqual(factory.Create<IObjectReturningNewGuids>().GetType(), factory.Create<IObjectReturningNewGuids>().GetType());
		}

		[Test]
		public void FactoryReturnsNewInterfaceInstances()
		{
			CacheBuilder
				 .For<ObjectWithIdentifier>()
				 .As<IObjectWithIdentifier>();
			var factory = CacheBuilder.BuildFactory();
			var obj1 = factory.Create<IObjectWithIdentifier>();
			var obj2 = factory.Create<IObjectWithIdentifier>();
			Assert.AreNotSame(obj1, obj2);
			Assert.AreNotEqual(obj1.Id, obj2.Id);
		}
	}
}