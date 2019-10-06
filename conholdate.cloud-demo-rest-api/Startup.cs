using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace conholdate.cloud_demo_rest_api
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
            services.AddCors();

            services.AddHttpContextAccessor();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for 
                // production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors(builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
            );

            app.UseDefaultFiles();

            // uncomment if you want to support static files
            app.UseStaticFiles();

            var configJson = JsonConvert.SerializeObject(new
            {
                AppSid = Configuration.GetValue<string>("ApiCred:AppSid"),
                AppKey = Configuration.GetValue<string>("ApiCred:AppKey"),
                TicketNo = Configuration.GetValue<string>("Defaults:TicketNo"),
                FlightNo = Configuration.GetValue<string>("Defaults:FlightNo"),
                FlightDate = Configuration.GetValue<string>("Defaults:FlightDate"),
                From = Configuration.GetValue<string>("Defaults:From"),
                To = Configuration.GetValue<string>("Defaults:To"),
                Class = Configuration.GetValue<string>("Defaults:Class"),
                Seat = Configuration.GetValue<string>("Defaults:Seat"),
                Name = Configuration.GetValue<string>("Defaults:Name"),
                Age = Configuration.GetValue<string>("Defaults:Age"),
                Phone = Configuration.GetValue<string>("Defaults:Phone"),
                Gender = Configuration.GetValue<string>("Defaults:Gender"),
            });

            app.UseRouter(builder =>
            {
                builder.MapGet("config.json", async context =>
                {
                    await context.Response.WriteAsync(configJson);
                });
            });


            app.UseMvc();

        }
    }
}
