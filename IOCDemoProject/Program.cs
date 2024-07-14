using IOCDemoProject.Interface;
using IOCDemoProject.Model;
using IOCService;

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
                service.AddTransient(typeof(IMobilePhone), typeof(NOKIAPhnoe));
                service.AddTransient(typeof(IAndriod), typeof(XiaoMi));
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
            }
            catch(Exception e) 
            {
                Console.WriteLine($"exception:{e.Message}");
            }
        }
    }
}
