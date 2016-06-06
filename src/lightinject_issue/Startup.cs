﻿using System;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace lightinject_issue
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options)
            : base(options)
        { }
    }

    public interface IInt { };

    public class IntImpl : IInt
    {
    }

    public class BaseController : Controller
    {
        public IInt BaseImpl { get; set; }
    }

    [Route("/1")]
    public class TestController : BaseController
    {
        public IInt MyInt { get; set; }
        protected IInt MyProtectedInt { get; set; }
//        private MyContext _ctx;
//
//        public TestController(MyContext ctx)
//        {
//            _ctx = ctx;
//        }

        public async Task<JsonResult> Index()
        {
            if (MyInt != null && MyProtectedInt != null && BaseImpl != null)
            {
                return Json("success");
            }
            throw new NullReferenceException();
        }
    }

    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDbContext<MyContext>(options => options.UseInMemoryDatabase());
            ServiceContainer ctr = new ServiceContainer();
            ctr.RegisterAssembly(typeof(Startup).GetTypeInfo().Assembly);
            //ctr.ScopeManagerProvider = new PerLogicalCallContextScopeManagerProvider();
            //ctr.Register<MyContext>(new PerScopeLifetime());
            
            
            return ctr.CreateServiceProvider(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseMvc();
        }
    }
}
