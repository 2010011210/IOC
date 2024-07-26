using Autofac;
using IOCDemoProject.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCDemoProject.Model
{
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
}
