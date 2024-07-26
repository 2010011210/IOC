# IOC  
~~~ 
// 把类注册到字
TomIOCServiceCollection service = new TomIOCServiceCollection();
service.AddTransient(typeof(IMobilePhone), typeof(NOKIAPhnoe));
service.AddTransient(typeof(IAndriod), typeof(XiaoMi));
service.AddTransient(typeof(IPhone), typeof(ApplePhone));
service.AddTransient(typeof(IHarmonry), typeof(HuaWeiPhone));
service.AddTransient(typeof(IPower), typeof(Power));

// 从容器中去除服务类
var huawei = service.GetService<IHarmonry>();
var apple = service.GetService<IPhone>();
var power = service.GetService<IPower>();
~~~
## 1.把类的类型放到字典中存放起来    
~~~  
TomIOCServiceCollection service = new TomIOCServiceCollection();
service.AddTransient(typeof(IMobilePhone), typeof(NOKIAPhnoe));

// TomIOCServiceCollection类中的方法
public void AddTransient(Type serviceType, Type implementationType)
{
    Console.WriteLine($"AddTransient:{serviceType.FullName}");
    typeDictionary[serviceType.FullName] = implementationType;
} 
~~~

## 2.通过key找到类，通过反射创建对象    
根据注册的接口T，通过typeof(T).FullName拿到key值，从字典中拿到需要构造的类的类型。  
~~~  
Type type = typeDictionary[typeof(T).FullName]; // 根据注册的接口T，通过typeof(T).FullName拿到key值，从字典中拿到需要构造的类的类型。
~~~
### 2.1 构造函数的选择   
 通过type.GetConstructors()获取构造函数，可能有多个
~~~ 
var contructorArr = type.GetConstructors(); // 获取构造函数，可能有多个。具体使用哪一个构造函数，后面可按数量选择，或者给构造函数加特性，用来筛选。
 ~~~

#### 2.1.1 根据参数数量选择构造函数  
~~~  
// 1.获取构造函数
var contructorArr = type.GetConstructors();  
constructor = contructorArr.OrderByDescending(i => i.GetParameters().Length).FirstOrDefault();  //选择参数最多的构造函数
~~~   

#### 2.1.2 构造函数加特性，用来选择 
~~~
/// 添加一个特性，只能用来修饰构造函数
[AttributeUsage(AttributeTargets.Constructor)]
public class SelectConstructorAttribute : Attribute
{

}

// 假如HuaWeiPhone有两个构造函数，会选择有SelectConstructor特性标记的构造函数

public class HuaWeiPhone: IHarmonry
{
    public string Name = "华为手机";

    [SelectConstructor]  //IOC用这个构造函数
    public HuaWeiPhone() 
    {
        Console.WriteLine($"构造函数：HuaWeiPhone()");
    }

    public HuaWeiPhone(IMobilePhone mobilePhone)
    {
        Console.WriteLine($"构造函数：HuaWeiPhone(IMobilePhone mobilePhone)");
    }
}
~~~  
有特性标记的选特性标记的构造函数，没有特性标记的，取参数最多的。  
~~~ 
// 1.获取构造函数
var contructorArr = type.GetConstructors();

// 获取参数最多的那一个构造函数,如果要获取规定的构造函数（需要用特性标注）
var constructor = contructorArr.Where(i => i.IsDefined(typeof(SelectConstructorAttribute), true)).FirstOrDefault();

// 假如没有特性SelectConstructor标记构造函数，根据参数数量取构造函数
if (constructor == null)  
{
    constructor = contructorArr.OrderByDescending(i => i.GetParameters().Length).FirstOrDefault();
}
~~~  

### 2.2 构造函数如果有参数，需要通过反射把参数构造出来 
构造函数的参数类型的构造函数没有参数，可以按照下面这种，构造出所有参数对象放到数组parametersList，通过Activator.CreateInstance(type, parametersList.ToArray())直接创建出类
~~~ 
// 1.获取构造函数
var contructorArr = type.GetConstructors();
// 获取参数最多的那一个构造函数,如果要获取规定的构造函数（需要用特性标注）
var contructor = contructorArr.OrderByDescending(i => i.GetParameters().Length).FirstOrDefault();

// 2.获取构造函数的参数
ParameterInfo[] parameters = contructor.GetParameters();
List<object> parametersArr = new List<object>();
foreach (var item in parameters)
{
    Type paraType = item.ParameterType;
    Type paramTagerType = typeDictionary[paraType.FullName];
    var target = Activator.CreateInstance(paramTagerType);         // 构造函数的参数类型的构造函数，可能还需要参数
    parametersArr.Add(target);
}
~~~  

如果构造函数的参数的构造函数还有其他的参数，需要通过递归调用来创建。  
~~~ 
/// <summary>
/// 获取服务实例
/// </summary>
/// <typeparam name="T"></typeparam>
/// <returns></returns>
/// <exception cref="NotImplementedException"></exception>
public T GetService<T>()
{
    Console.WriteLine($"GetService:{typeof(T).FullName}");
    Type type = typeDictionary[typeof(T).FullName];
    return (T)this.GetService(type);
}

private object GetService(Type type) 
{
    // 1.获取构造函数
    var contructorArr = type.GetConstructors();
    // 获取参数最多的那一个构造函数,如果要获取规定的构造函数（需要用特性标注）
    var constructor = contructorArr.Where(i => i.IsDefined(typeof(SelectConstructorAttribute), true)).FirstOrDefault();
    if (constructor == null) 
    {
        constructor = contructorArr.OrderByDescending(i => i.GetParameters().Length).FirstOrDefault();
    }

    // 2.获取构造函数的参数
    ParameterInfo[] parameters = constructor.GetParameters();
    List<object> parametersList = new List<object>();
    foreach (var item in parameters)
    {
        Type paraType = item.ParameterType;
        Type paramTagerType = typeDictionary[paraType.FullName];
        var target = GetService(paramTagerType);         // 构造函数的参数类型的构造函数，可能还需要参数
        parametersList.Add(target);
    }

    var result = Activator.CreateInstance(type, parametersList.ToArray());
    // var result2 = constructor.Invoke(parametersList.ToArray()); //或者通过构造函数创建
    return result;
}
~~~ 
## 3.高阶的应用
1. 通过属性注入
2. 通过方法注入
3. 一个服务接口注册多给类，如何实现？注册的时候， 加一个多业务标识进行区分  

### 3.1 属性注入  
如果对象的构造函数，没有给某个属性赋值，可以通过属性注入的方式实现。 献给需要注入的属性打一个标签，然后给有标签的属性注入对象。   

1. 增加属性注入的特性 ，给需要注入的属性加上特性 
~~~
属性注入的类
[AttributeUsage(AttributeTargets.Property)]
public class PropertyInjectAttribute : Attribute
{

}

public class Power : IPower
{
    public  IMobilePhone _mobilePhone { get; set; }
    public IAndriod _andriod { get; set; }

    [PropertyInject]  //给huaweiPhone加上特性的标签，属性注入
    public IHarmonry huaweiPhone { get; set; }
}

~~~
2. 构造的时候把有属性注入特性的属性过滤出来进行注入操作。 

~~~
// 3.1 获取需要属性注入的类型(属性带有PropertyInject特性的)
var propertyTypes = type.GetProperties().Where(p => p.IsDefined(typeof(PropertyInjectAttribute), true));

foreach (var propertyInfo in propertyTypes) 
{
    Type propertyType = propertyInfo.PropertyType;
    Type propertyImplementType = typeDictionary[propertyType.FullName];
    object propertyInstance = GetService(propertyImplementType);  // 属性的实例
    propertyInfo.SetValue(objInstance, propertyInstance);    // objInstance是之前创建的对象，给对象objInstance的属性赋值
}
~~~  

### 3.2 方法注入 
有的属性是通过方法赋值的，比如下面的 。可以通过调用方法给phone赋值。大概的步骤是分两步：  
1. 给需要触发的方法标识特性，过滤出需要触发的方法SetApplePhone，其他方法不要。
2. 触发方法SetApplePhone
~~~
public class Power : IPower
{
    public IPhone phone { get; set; }

    public void SetApplePhone(IPhone phone) 
    {
        this.phone = phone;
    }

    public string GetPhoneName(IPhone phone) 
    {
        return "苹果手机";
    }
}
~~~  

添加一个特性，标识到需要触发的方法上面 
~~~
[AttributeUsage(AttributeTargets.Method | AttributeTargets.All)]
public class MethodInjectAttribute : Attribute
{
}

public class Power : IPower
{
    public IPhone phone { get; set; }

    [MethodInject]
    public void SetApplePhone(IPhone phone) 
    {
        this.phone = phone;
    }

    public string GetPhoneName(IPhone phone) 
    {
        return "苹果手机";
    }
}
~~~

通过type.GetMethods()拿到所有的方法，然后判断方法是否带有MethodInject的特性m.IsDefined(typeof(MethodInjectAttribute), true)。   
    method.Invoke(objInstance, methodParamList.ToArray());
找到方法之后，获取方法的参数，构造所有参数放到列表中。然后触发方法method.Invoke(objInstance, methodParamList.ToArray());

~~~
MethodInfo[] allMethods = type.GetMethods();
List<MethodInfo> injectMethods = allMethods.Where(m => m.IsDefined(typeof(MethodInjectAttribute), true)).ToList();
foreach (var method in injectMethods) 
{
    List<object> methodParamList = new List<object>();
    // 获取方法参数
    ParameterInfo[] methodParameters = method.GetParameters();
    foreach (var param in methodParameters) 
    {
        Type parameterType = param.ParameterType;
        Type paramterImplementType = typeDictionary[parameterType.FullName]; 
        object mParamInstance = GetService(paramterImplementType);  // 类型的构造函数可能还需要其他类型的参数，需要递归调用
        methodParamList.Add(mParamInstance);
    }
    // 触发方法 ,把对象注入
    method.Invoke(objInstance, methodParamList.ToArray());
}

~~~

### 3.3 多个服务注册到同一个接口  


假如一个接口注册了多个服务类，注册的时候可以拼接一个别名 
~~~
TomIOCServiceCollection service = new TomIOCServiceCollection();
service.AddTransient(typeof(IAndriod), typeof(OppoPhone), "Oppo");  // 
service.AddTransient(typeof(IAndriod), typeof(XiaoMi));

var oppo = service.GetService<IAndriod>("Oppo");    // 取出服务类的时候，
var xiaomi  = service.GetService<IAndriod>();
~~~

注册的时候key值后面拼接一个别名，取的时候也把别名拼接上
~~~
写一个方法固定返回注册的key值
private string GetKey(Type type, string shortName) 
{
    if (string.IsNullOrEmpty(shortName))
    {
        return type.FullName;
    }
    else 
    {
        return $"{type.FullName}_{shortName}";
    }
}

/// <summary>
/// 注册服务， 把抽象和具体的映射关系保存起来
/// </summary>
/// <param name="serviceType"></param>
/// <param name="implementationType"></param>
/// <exception cref="NotImplementedException"></exception>
public void AddTransient(Type serviceType, Type implementationType, string shortName = "")
{
    string key  = GetKey(serviceType, shortName);  //key是拼接别名shortName的
    typeDictionary[key] = implementationType;
}

 /// <summary>
 /// 获取服务实例
 /// </summary>
 /// <typeparam name="T"></typeparam>
 /// <returns></returns>
 /// <exception cref="NotImplementedException"></exception>
 public T GetService<T>(string shortName)
 {
     Console.WriteLine($"GetService:{typeof(T).FullName}");
     Type type = typeDictionary[GetKey(typeof(T), shortName)]; // key值也是拼接别名的
     return (T)this.GetService(type);
 }

~~~

## 4. Autofac 

1. 普通构造函数注入  
~~~
 ContainerBuilder containerBuilder = new ContainerBuilder();
 //containerBuilder.RegisterModule<AutofacModel>();  //把注册部分写到AutofacModel类中。
 containerBuilder.RegisterType<NOKIAPhnoe>().As<IMobilePhone>();
 containerBuilder.RegisterType<ApplePhone>().As<IPhone>();

 IContainer container = containerBuilder.Build();

 IHarmonry huaweiPhone = container.Resolve<IHarmonry>();
~~~
2. 属性注入
~~~
containerBuilder.RegisterType<Power>().As<IPower>().PropertiesAutowired(); // 2.属性注入
IPower powerPhone = container.Resolve<IPower>();
// builder.RegisterType<B>()
       .PropertiesAutowired(
         (propInfo, instance) => propInfo.PropertyType.Name.StartsWith("I"));
// 官方文档
// https://autofac.readthedocs.io/en/latest/register/prop-method-injection.html
~~~

3. 一个接口多个实现
~~~
containerBuilder.RegisterType<XiaoMi>().As<IAndriod>();
containerBuilder.RegisterType<OppoPhone>().As<IAndriod>().Named<IAndriod>("OppoPhone"); // 

IAndriod xiaomiPhone = container.Resolve<IAndriod>();
IAndriod oppoPhone = container.ResolveKeyed<IAndriod>("OppoPhone");
~~~

4. 方法注入
~~~
builder
  .RegisterType<MyObjectType>()
  .OnActivating(e => {
    var dep = e.Context.Resolve<TheDependency>();
    e.Instance.SetTheDependency(dep);
  });
~~~

4. 如果注册的类比较多，可以放到解除类Autofac.Module中的方法中，重写Load方法
~~~
public class AutofacModel:Autofac.Module
{
    protected override void Load(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<XiaoMi>().As<IAndriod>();
        containerBuilder.RegisterType<OppoPhone>().As<IAndriod>().Named<IAndriod>("OppoPhone");
        containerBuilder.RegisterType<NOKIAPhnoe>().As<IMobilePhone>();
        containerBuilder.RegisterType<ApplePhone>().As<IPhone>();
        containerBuilder.RegisterType<HuaWeiPhone>().As<IHarmonry>();
        containerBuilder.RegisterType<Power>().As<IPower>().PropertiesAutowired(); //属性注入
    }
}

注入的时候直接RegisterModule<AutofacModel>()；
ContainerBuilder containerBuilder = new ContainerBuilder();
containerBuilder.RegisterModule<AutofacModel>();  //把注册部分写到AutofacModel类中。
IContainer container = containerBuilder.Build();

~~~

## 5.其他问题 
1. 生命周期，瞬时transient，单例Single，作用域Scope
2. 构造函数时值类型，如何赋值


