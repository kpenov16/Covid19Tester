using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Covid19Tester.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Covid19Tester.Models.Services;

namespace Covid19Tester
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddSingleton<IPersonService>(
                InitializeCosmosClientInstanceAsync(Configuration.GetSection("CosmosDb"))
                .GetAwaiter().GetResult());
            //InitializeCosmosClientInstanceAsync
            services.AddRazorPages();
        }

        public static async Task<PersonService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string dbName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string accountName = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;

            Microsoft.Azure.Cosmos.CosmosClient cosmosClient = new Microsoft.Azure.Cosmos.CosmosClient(accountName, key);
            PersonService personService = new PersonService(cosmosClient, dbName, containerName);
            Microsoft.Azure.Cosmos.DatabaseResponse databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
            await databaseResponse.Database.CreateContainerIfNotExistsAsync(containerName, "/cprnumber");

            return personService;
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
