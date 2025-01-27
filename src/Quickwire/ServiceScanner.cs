﻿// Copyright 2021 Flavien Charlon
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

namespace Quickwire;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quickwire.Attributes;

public static class ServiceScanner
{
    public static IEnumerable<Func<ServiceDescriptor>> ScanServiceRegistrations(Type type, IServiceProvider serviceProvider)
    {
        IServiceActivator serviceActivator = serviceProvider.GetRequiredService<IServiceActivator>();

        if (!type.IsAbstract && CanScan(type, serviceProvider))
        {
            foreach (RegisterServiceAttribute registerAttribute in type.GetCustomAttributes<RegisterServiceAttribute>())
            {
                if (registerAttribute.AsImplementedInterfaces)
                {
                    IEnumerable<Type> interfaces=type.GetInterfaces().Where(i => i != typeof(IDisposable));
                    foreach (Type implInterface in interfaces)
                    {
                        yield return () => new ServiceDescriptor(
                            implInterface,
                            serviceActivator.GetFactory(type),
                            registerAttribute.Scope);
                    }
                }
                else
                {
                    Type serviceType = registerAttribute.ServiceType ?? type;

                    if (!serviceType.IsAssignableFrom(type))
                    {
                        throw new ArgumentException(
                            $"The concrete type '{type.FullName}' cannot be used to register service type '{serviceType.FullName}'.");
                    }
                    yield return () => new ServiceDescriptor(
                        serviceType,
                        serviceActivator.GetFactory(type),
                        registerAttribute.Scope);
                }
            }
        }
    }

    public static IEnumerable<Func<ServiceDescriptor>> ScanFactoryRegistrations(Type type, IServiceProvider serviceProvider)
    {
        if (CanScan(type, serviceProvider))
        {
            IServiceActivator serviceActivator = serviceProvider.GetRequiredService<IServiceActivator>();

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                if (CanScan(method, serviceProvider))
                {
                    foreach (RegisterFactoryAttribute registerAttribute in method.GetCustomAttributes<RegisterFactoryAttribute>())
                    {
                        Type serviceType = registerAttribute.ServiceType ?? method.ReturnType;

                        if (!serviceType.IsAssignableFrom(method.ReturnType))
                        {
                            throw new ArgumentException(
                                $"The method '{method.Name}' with return type '{method.ReturnType}' cannot be used " +
                                $"to register service type '{serviceType.FullName}'.");
                        }

                        yield return () => new ServiceDescriptor(
                            serviceType,
                            serviceActivator.GetFactory(method),
                            registerAttribute.Scope);
                    }
                }
            }
        }
    }

    private static bool CanScan(ICustomAttributeProvider customAttributeProvider, IServiceProvider serviceProvider)
    {
        return customAttributeProvider
            .GetCustomAttributes(typeof(IServiceScanningFilter), false)
            .OfType<IServiceScanningFilter>()
            .All(filter => filter.CanScan(serviceProvider));
    }
}
