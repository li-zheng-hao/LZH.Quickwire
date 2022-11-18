# LZH.Quickwire

本项目为[Quickwire项目](https://github.com/Flavien/quickwire)的Fork版本，加上了我的一些定制改动，下面是源项目的README个人中文翻译版本，加上变动的说明，可能存在一些语义上的偏差，有不理解的可以看源项目

一个基于特性标记的.NET 依赖注入库


## 改动列表

1. 添加自动注册为所有实现的接口功能(与Autofac一致)，使用方法如下:

```c#
[RegisterService(AsImplementedInterfaces = true)]
public class A : IInterface
{
    public int CompareTo(object obj)
    {
        return 1;
    }
}
```

> A类会自动注册为IInterface

2. 配置项注册时如果为null，则会使用代码中的默认值，如下：

```c#
[RegisterService]
public class Config
{
    [InjectConfiguration("test:not_exist")]
    public string Test { get; set; } = "default";
}
```

>  配置项不存在，则`Test`值为`default`


## 特性

- 使用特性进行依赖注入从而注册服务，避免重复代码
- 使用了.NET内置的依赖注入容器，不需要使用第三方的容器库或者改变现有的代码
- 全面支持属性注入
- 无缝注入配置项到服务中
- 使用配置选择器，根据当前环境注册不同的实现方式

## 快速开始

1. Nuget上下载包:

```
dotnet add package Quickwire.Plus
```

2. 在`Program.cs`(.NET 6)中添加注册代码:

```csharp
builder.Services.ScanCurrentAssembly();
```

1. 使用特性标记需要注入的类:

```csharp
[RegisterService]
public class MyService
{
    // ...
}
```

## 注册服务

有两种方式来注册服务：

- 在类上面添加 `[RegisterService]`特性
- 在静态工厂方法上添加 `[RegisterFactory]`特性来注册该方法返回的类型

## 使用类来注册服务

### 注册

当在类上添加 `[RegisterService]` 特性时, 该类将会被注册为相同类型的服务

同样也可以通过`ServiceType`属性来自定义需要注册的类型或接口

该特性上还有一个属性`ServiceLifetime`表示该服务的生命周期，默认为`Transient` 

### 实例化

Quickwire默认使用public的构造函数来实例化具体的类型，因此必须要有一个public的构造函数，否则会抛出异常

如果存在多个public构造函数，或者需要使用一个非public的构造函数，应该使用 `[ServiceConstructor]`特性标记需要使用的构造函数，否则将会抛出异常

默认所有构造函数的参数都使用依赖注入解析，然而你也可以通过使用`[InjectConfiguration]`特性来标记注入一个配置项，或者使用`[InjectService(Optional = true)]`特性标记表示这个依赖项是可选的


### 属性注入

属性注入可以在带有setter的属性上使用`[InjectService]`特性

```csharp
[RegisterService(ServiceLifetime.Singleton)]
public class MyService
{
    [InjectService]
    public IHttpClientBuilder HttpClientBuilder { get; private set; }

    // ...
}
```

### 自动注入init-only属性

在类上标记`[InjectAllInitOnlyProperties]`特性，所有的init-only属性都会自动注入

```csharp
[RegisterService(ServiceLifetime.Singleton)]
[InjectAllInitOnlyProperties]
public class MyService
{
    public IHttpClientBuilder HttpClientBuilder { get; init; }

    // ...
}
```

## 使用工程注册服务

在静态方法上添加`[RegisterFactory]`特性标记时，这个静态方法会被注册为返回类型的工厂方法

方法上所有的参数默认都会使用依赖注入解析，如果使用`[InjectConfiguration]` 特性标记的参数会使用配置项注入，也可以使用 `[InjectService(Optional = true)]` 来使依赖可选

```csharp
public static class LoggingConfiguration
{
    [RegisterFactory]
    public static ILogger CreateLogger()
    {
        // ...
    }
}
```

## 配置设置注入

构造函数参数，属性和工厂方法都可以使用 `[InjectConfiguration]`特性标记从而注入配置项

配置项的类型转换支持绝大多数基本类型，包括迭代类型

```csharp
[RegisterService]
public class MyService
{
    [InjectConfiguration("external_api:url")]
    public string ExternalApiUrl { get; init; }

    [InjectConfiguration("external_api:retries")]
    public int Retries { get; init; }

    [InjectConfiguration("external_api:timeout")]
    public TimeSpan Timeout { get; init; }

    // ...
}
```

## 环境选择


支持使用`[EnvironmentSelector]`特性来根据不同的环境来选择是否需要注入服务

```csharp
[EnvironmentSelector(Enabled = new[] { "Development" })]
public class DebugFactories
{
    // This is only registered in the Development environment
    [RegisterFactory]
    public static ILogger CreateDebugLogger()
    {
        // ...
    }

    // ...
}
```

## 根据配置项选择

通过使用`[ConfigurationBasedSelector]`特性来根据配置项的值选择是否需要注册服务

```csharp
[ConfigurationBasedSelector("logging:mode", "debug")]
public class DebugFactories
{
    // This is only registered if the logging:mode configuration setting is set to "debug"
    [RegisterFactory]
    public static ILogger CreateDebugLogger()
    {
        // ...
    }

    // ...
}
```

## 在ASP.NET Core controllers中使用Quickwire

默认asp.net core中的controller不会使用依赖注入容器来实例化，不过官方也提供了一个简单的方式来更改这个行为，只需要在`Program.cs`(.NET 6)中添加下面这段代码：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Activate controllers using the dependency injection container
    builder.Services.AddControllers().AddControllersAsServices();

    services.ScanCurrentAssembly();
}
```

然后使用特性标记就可以注入其它服务了：

```csharp
[ApiController]
[Route("[controller]")]
[RegisterService]
public class ShoppingCartController : ControllerBase
{
    [InjectService]
    public IShoppingCartRepository { get; init; }

    // ...
}
```

## 和Java Spring框架一致

Quickwire提供的方法和Java中的Spring框架非常相似，下面是比较：

|       | Quickwire | Spring |
| ----- | --------- | ------ |
| Register a class for dependency injection | `[RegisterService]` | `@Service`, `@Component`, `@Repository` |
| Register a factory method | `[RegisterFactory]` | `@Bean` |
| Property injection | `[InjectService]` | `@Autowired` |
| Configuration setting injection | `[InjectConfiguration]` | `@Value` |
| Selective activation based on environment | `[EnvironmentSelector]` | `@Profile("profile")`
| Bootstrap | `services.ScanCurrentAssembly()` | `@EnableAutoConfiguration`, `@ComponentScan` |

## License

Copyright 2021 Flavien Charlon

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and limitations under the License.
