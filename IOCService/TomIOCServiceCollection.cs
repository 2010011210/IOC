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

            var result = Activator.CreateInstance(type, parametersList.ToArray());
            // var result2 = constructor.Invoke(parametersList.ToArray()); //或者通过构造函数创建
            return result;


        }

    }
}
