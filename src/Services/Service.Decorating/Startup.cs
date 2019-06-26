﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.BackgroundServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SignalRDemo.Hubs;
using Confluent.Kafka;
namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddJsonFile("globalkafkasettings.json")
            .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSignalR();

            services.AddHostedService<DecorateProcessService>();

            services.AddCors(c =>
                {
                    c.AddPolicy("AllowOrigin", options => options.WithOrigins("http://localhost:3000","http://35.232.19.14","https://35.232.19.14").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
                });
            
            var consumerConfig = new ConsumerConfig();
            Configuration.Bind("consumer",consumerConfig);


            //Reading the environment variable.
            var envBootStrapServers = Configuration.GetValue<string>("ENV_KAFKA_CLUSTER");
            if(!String.IsNullOrEmpty(envBootStrapServers)){
                consumerConfig.BootstrapServers =  envBootStrapServers;
            }

            var envSaslUserName = Configuration.GetValue<string>("ENV_KAFKA_USER_NAME");
            if(!String.IsNullOrEmpty(envSaslUserName)){
                consumerConfig.SaslUsername =  envSaslUserName;
            }

            var envSaslPassword = Configuration.GetValue<string>("ENV_KAFKA_USER_PASSWORD");
            if(!String.IsNullOrEmpty(envSaslPassword)){
                consumerConfig.SaslPassword =  envSaslPassword;
            }
            services.AddSingleton<ConsumerConfig>(consumerConfig);
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
                app.UseHsts();
            }
            app.UseCors("AllowOrigin");
            app.UseMvc();
            app.UseSignalR(routes =>
            {
                routes.MapHub<OrderMonitorHub>("/ordermonitorhub");
            });
        }
    }
}
