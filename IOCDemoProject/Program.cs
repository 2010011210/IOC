using Autofac;
using IOCDemoProject.Interface;
using IOCDemoProject.Model;
using IOCService;
//using System.ComponentModel;

namespace IOCDemoProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            try
            {
                TomIOCServiceCollection service = new TomIOCServiceCollection();
                service.AddTransient(typeof(IAndriod), typeof(OppoPhone), "Oppo");
                service.AddTransient(typeof(IAndriod), typeof(XiaoMi));
                service.AddTransient(typeof(IMobilePhone), typeof(NOKIAPhnoe));
                service.AddTransient(typeof(IPhone), typeof(ApplePhone));
                service.AddTransient(typeof(IHarmonry), typeof(HuaWeiPhone));
                service.AddTransient(typeof(IPower), typeof(Power));

                //var xiaomi = new XiaoMi();
                //var nokia = new NOKIAPhnoe();
                //var parameters = new List<object>() { nokia, xiaomi };
                //var createObj = Activator.CreateInstance(typeof(Power), parameters.ToArray());

                var huawei = service.GetService<IHarmonry>();
                var apple = service.GetService<IPhone>();
                var power = service.GetService<IPower>();
                var oppo = service.GetService<IAndriod>("Oppo");      
                var xiaomi  = service.GetService<IAndriod>();

                #region Autofac
                ContainerBuilder containerBuilder = new ContainerBuilder();
                //containerBuilder.RegisterModule<AutofacModel>();  //把注册部分写到AutofacModel类中。

                containerBuilder.RegisterType<XiaoMi>().As<IAndriod>();
                containerBuilder.RegisterType<OppoPhone>().As<IAndriod>().Named<IAndriod>("OppoPhone"); // 1. 一个接口多个实例
                containerBuilder.RegisterType<NOKIAPhnoe>().As<IMobilePhone>();
                containerBuilder.RegisterType<ApplePhone>().As<IPhone>();
                containerBuilder.RegisterType<HuaWeiPhone>().As<IHarmonry>();
                containerBuilder.RegisterType<Power>().As<IPower>().PropertiesAutowired(); // 2.属性注入

                // build一个contain
                IContainer container = containerBuilder.Build();
                IAndriod xiaomiPhone = container.Resolve<IAndriod>();
                IAndriod oppoPhone = container.ResolveKeyed<IAndriod>("OppoPhone");

                IPower powerPhone = container.Resolve<IPower>();
                IHarmonry huaweiPhone = container.Resolve<IHarmonry>();

                #endregion

            }
            catch (Exception e) 
            {
                Console.WriteLine($"exception:{e.Message}");
            }
        }
    }
}
