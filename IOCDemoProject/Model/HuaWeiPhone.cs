using IOCDemoProject.Interface;
using IOCService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCDemoProject.Model
{
    public class HuaWeiPhone: IHarmonry
    {
        public string Name = "华为手机";

        //[SelectConstructor]  //IOC用这个构造函数
        public HuaWeiPhone() 
        {
            Console.WriteLine($"构造函数：HuaWeiPhone()");
        }

        [SelectConstructor]
        public HuaWeiPhone(IMobilePhone mobilePhone)
        {
            Console.WriteLine($"构造函数：HuaWeiPhone(IMobilePhone mobilePhone)");
        }


    }
}
