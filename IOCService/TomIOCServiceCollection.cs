using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IOCService
{
    public class TomIOCServiceCollection : ITomIOCServiceCollection
    {
        private Dictionary<string, Type> typeDictionary = new Dictionary<string, Type>();

        /// <summary>
        /// 注册服务， 把抽象和具体的映射关系保存起来
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddTransient(Type serviceType, Type implementationType)
        {
            Console.WriteLine($"AddTransient:{serviceType.FullName}");
            // typeDictionary.Add(serviceType.FullName, implementationType);
            typeDictionary[serviceType.FullName] = implementationType;
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public T GetService<T>()
        {
            #region MyRegion
            //// 1.获取构造函数
            //var contructorArr = type.GetConstructors();
            //// 获取参数最多的那一个构造函数,如果要获取规定的构造函数（需要用特性标注）
            //var contructor = contructorArr.OrderByDescending(i=> i.GetParameters().Length).FirstOrDefault();

            //// 2.获取构造函数的参数
            //ParameterInfo[] parameters = contructor.GetParameters();
            //List<object> parametersArr = new List<object>();
            //foreach (var item in parameters)
            //{
            //    Type paraType = item.ParameterType;
            //    Type paramTagerType = typeDictionary[paraType.FullName];
            //    var target = Activator.CreateInstance(paramTagerType);         // 构造函数的参数类型的构造函数，可能还需要参数
            //    parametersArr.Add(target);
            //}

            //var result = (T)Activator.CreateInstance(type, parameters);
            //return result;
            #endregion
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

            var objInstance = Activator.CreateInstance(type, parametersList.ToArray());
            // var result2 = constructor.Invoke(parametersList.ToArray()); //或者通过构造函数创建

            #region 3 属性注入
            // 3.1 获取需要属性注入的类型(属性带有PropertyInject特性的)
            var propertyTypes = type.GetProperties().Where(p => p.IsDefined(typeof(PropertyInjectAttribute), true));

            foreach (var propertyInfo in propertyTypes) 
            {
                Type propertyType = propertyInfo.PropertyType;
                Type propertyImplementType = typeDictionary[propertyType.FullName];
                object propertyInstance = GetService(propertyImplementType);  // 属性的实例
                propertyInfo.SetValue(objInstance, propertyInstance);    // 给对象objInstance的属性赋值
            }

            #endregion

            #region 方法注入  //MethodInject
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

            #endregion

            return objInstance;


        }

    }
}
