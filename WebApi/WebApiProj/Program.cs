
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
            // �Դ���DI����ע��
            //builder.Services.AddSingleton<ICompany, CompanyService>(); // ʾ��������ģʽ  
            //builder.Services.AddTransient<ICompany, CompanyService>(); // ʾ����˲ʱģʽ  
            //builder.Services.AddScoped<ICompany, CompanyService>();    // ʾ����������ģʽ  

            /// Autofac��ʹ��
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterType<CompanyService>().As<ICompany>();  // Autofac����ע��
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
