
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using WebApiProj.Service;

namespace WebApiProj
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // 自带的DI依赖注入
            //builder.Services.AddSingleton<ICompany, CompanyService>(); // 示例：单例模式  
            //builder.Services.AddTransient<ICompany, CompanyService>(); // 示例：瞬时模式  
            //builder.Services.AddScoped<ICompany, CompanyService>();    // 示例：作用域模式  

            /// Autofac的使用
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterType<CompanyService>().As<ICompany>();  // Autofac依赖注入
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
