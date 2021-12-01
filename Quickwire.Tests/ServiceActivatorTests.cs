// Copyright 2021 Flavien Charlon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quickwire.Tests.Subjects;
using Xunit;

namespace Quickwire.Tests
{
    public class ServiceActivatorTests
    {
        private readonly ServiceActivator _activator = new ServiceActivator();
        private readonly ServiceCollection _services;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dependency _dependency = new Dependency();

        public ServiceActivatorTests()
        {
            _services = new ServiceCollection();
            _services.AddSingleton<Dependency>(_dependency);
            _services.AddSingleton<string>("Test");
            _serviceProvider = _services.BuildServiceProvider();
        }

        #region GetFactory(Type)

        [Fact]
        public void GetFactoryType_ConstructorInjection()
        {
            object resultObject = _activator.GetFactory(typeof(TestObjects.ConstructorInjection))(_serviceProvider);
            TestObjects.ConstructorInjection result = resultObject as TestObjects.ConstructorInjection;

            Assert.NotNull(result);
            Assert.Equal(_dependency, result.Dependency1);
            Assert.Equal("Test", result.Dependency2);
        }

        [Fact]
        public void GetFactoryType_MultipleConstructors()
        {
            object resultObject = _activator.GetFactory(typeof(TestObjects.MultipleConstructors))(_serviceProvider);
            TestObjects.MultipleConstructors result = resultObject as TestObjects.MultipleConstructors;

            Assert.NotNull(result);
            Assert.Equal(_dependency, result.Dependency1);
            Assert.Equal("Second Constructor", result.Dependency2);
        }

        [Fact]
        public void GetFactoryType_PrivateConstructor()
        {
            Assert.Throws<ArgumentException>(() =>
                _activator.GetFactory(typeof(TestObjects.PrivateConstructor)));
        }

        [Fact]
        public void GetFactoryType_NoConstructorSelector()
        {
            Assert.Throws<ArgumentException>(() =>
                _activator.GetFactory(typeof(TestObjects.NoConstructorSelector)));
        }

        [Fact]
        public void GetFactoryType_MoreThanOneConstructorSelector()
        {
            Assert.Throws<ArgumentException>(() =>
                _activator.GetFactory(typeof(TestObjects.NoConstructorSelector)));
        }

        [Fact]
        public void GetFactoryType_UnresolvableConstructorInjection()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _activator.GetFactory(typeof(TestObjects.UnresolvableConstructorInjection))(_serviceProvider));
        }

        [Fact]
        public void GetFactoryType_NoSetterInjection()
        {
            object resultObject = _activator.GetFactory(typeof(TestObjects.NoSetterInjection))(_serviceProvider);
            TestObjects.NoSetterInjection result = resultObject as TestObjects.NoSetterInjection;

            Assert.NotNull(result);
            Assert.Null(result.DependencyGet);
            Assert.Null(result.DependencyGetSet);
            Assert.Null(result.DependencyGetInit);
        }

        [Fact]
        public void GetFactoryType_SetterInjection()
        {
            object resultObject = _activator.GetFactory(typeof(TestObjects.SetterInjection))(_serviceProvider);
            TestObjects.SetterInjection result = resultObject as TestObjects.SetterInjection;

            Assert.NotNull(result);
            Assert.Equal(_dependency, result.DependencyGetSet);
        }

        [Fact]
        public void GetFactoryType_InitOnlySetterInjection()
        {
            object resultObject = _activator.GetFactory(typeof(TestObjects.InitOnlySetterInjection))(_serviceProvider);
            TestObjects.InitOnlySetterInjection result = resultObject as TestObjects.InitOnlySetterInjection;

            Assert.NotNull(result);
            Assert.Null(result.DependencyGet);
            Assert.Null(result.DependencyGetSet);
            Assert.Equal(_dependency, result.DependencyGetInit);
        }

        [Fact]
        public void GetFactoryType_NonPublicSetter()
        {
            object resultObject = _activator.GetFactory(typeof(TestObjects.NonPublicSetter))(_serviceProvider);
            TestObjects.NonPublicSetter result = resultObject as TestObjects.NonPublicSetter;

            Assert.NotNull(result);
            Assert.Equal(_dependency, result.GetDependencyGetSet1());
            Assert.Equal(_dependency, result.DependencyGetSet2);
            Assert.Equal(_dependency, result.DependencyGetSet3);
            Assert.Equal(_dependency, result.DependencyGetSet4);
        }

        [Fact]
        public void GetFactoryType_UnresolvableSetterInjection()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _activator.GetFactory(typeof(TestObjects.UnresolvableSetterInjection))(_serviceProvider));
        }

        [Fact]
        public void GetFactoryType_UnresolvableInitOnlySetterInjection()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _activator.GetFactory(typeof(TestObjects.UnresolvableInitOnlySetterInjection))(_serviceProvider));
        }

        #endregion

        #region GetFactory(MethodInfo)

        [Fact]
        public void GetFactoryMethodInfo_ParameterInjection()
        {
            object resultObject = _activator.GetFactory(GetMethod(nameof(TestMethods.ParameterInjection)))(_serviceProvider);
            string result = resultObject as string;

            Assert.Equal("Value", result);
        }

        [Fact]
        public void GetFactoryMethodInfo_InstanceMethod()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _activator.GetFactory(GetMethod(nameof(TestMethods.InstanceMethod))));
        }

        [Fact]
        public void GetFactoryMethodInfo_InternalMethod()
        {
            object resultObject = _activator.GetFactory(GetMethod(nameof(TestMethods.InternalMethod)))(_serviceProvider);
            string result = resultObject as string;

            Assert.Equal("Value", result);
        }

        [Fact]
        public void GetFactoryMethodInfo_PrivateMethod()
        {
            object resultObject = _activator.GetFactory(GetMethod("PrivateMethod"))(_serviceProvider);
            string result = resultObject as string;

            Assert.Equal("Value", result);
        }

        [Fact]
        public void GetFactoryMethodInfo_UnresolvableParameterInjection()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _activator.GetFactory(GetMethod(nameof(TestMethods.UnresolvableParameterInjection)))(_serviceProvider));
        }

        private MethodInfo GetMethod(string name) =>
            typeof(TestMethods).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        #endregion
    }
}