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
1. 一个服务接口注册多给类，如何实现？注册的时候， 加一个多业务标识进行区分  
2. 通过属性注入
3. 通过方法注入
