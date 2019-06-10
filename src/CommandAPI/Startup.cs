using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommandAPI.Models;


namespace CommandAPI
{
    public class Startup
    {
        public IConfiguration Configuration {get;}
        public Startup(IConfiguration configuration) => Configuration = configuration;


        public void ConfigureServices(IServiceCollection services)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = Configuration.GetConnectionString("CommandAPISQLConection");
            builder.UserID = Configuration["UserID"];
            builder.Password = Configuration["Password"];

            services.AddDbContext<CommandContext>
            (opt => opt.UseSqlServer(builder.ConnectionString));

            services.AddMvc().
            SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        }

        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
