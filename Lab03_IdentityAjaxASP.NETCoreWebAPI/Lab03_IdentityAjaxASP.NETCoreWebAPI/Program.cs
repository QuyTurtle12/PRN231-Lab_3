using DataAccess;
using DataAccess.DAO;
using DataAccess.Interface;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Repositories.Repository;

namespace Lab03_IdentityAjaxASP.NETCoreWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Register DI services
            builder.Services.AddScoped<IUOW, UOW>();
            builder.Services.AddScoped(typeof(IGenericDAO<>), typeof(GenericDAO<>));
            builder.Services.AddScoped<IOrchidRepository, OrchidRepository>();

            // Get connection string from appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("MyCnn");

            // Register DbContext with DI
            builder.Services.AddDbContext<ProductManagementDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
