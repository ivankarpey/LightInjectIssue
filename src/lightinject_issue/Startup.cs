using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        public User()
        {
            Id = new Guid();
        }
    }

    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }

    public interface IIntService
    {
        void Register(User u);
    };

    public class IntServiceImpl : IIntService
    {
        private MyContext _ctx;

        public IntServiceImpl(MyContext ctx)
        {
            this._ctx = ctx;
        }

        public void Register(User u)
        {
            this._ctx.Users.Add(u);
            this._ctx.SaveChangesAsync();
        }
    }

    public class BaseController : Controller
    {
        public IIntService BaseImpl { get; set; }
    }

    [Route("1")]
    public class TestController : BaseController
    {
        public IIntService MyIntService { get; set; }
        protected IIntService MyProtectedIntService { get; set; }

        public async Task<JsonResult> Index()
        {
            if (MyIntService != null && MyProtectedIntService != null && BaseImpl != null)
            {
                return Json("success");
            }
            throw new NullReferenceException();
        }

        [HttpGet("scope_error")]
        public async Task<JsonResult> ScopeError()
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() =>
            {
                ((dynamic)Startup.GetProvider().GetService(typeof(IIntService))).Register(new User());
            }));
            tasks.Add(Task.Run(() =>
            {
                ((dynamic)Startup.GetProvider().GetService(typeof(IIntService))).Register(new User());
            }));
            tasks.Add(Task.Run(() =>
            {
                ((dynamic)Startup.GetProvider().GetService(typeof(IIntService))).Register(new User());
            }));
            tasks.Add(Task.Run(() =>
            {
                ((dynamic)Startup.GetProvider().GetService(typeof(IIntService))).Register(new User());
            }));
            Task.WaitAll(tasks.ToArray());
            return Json("success");
        }

    }

    public class Startup
    {
        private static IServiceProvider provider;
        public static string TEST_ENV = "test";
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
            
            //**********************************************************************************
            ctr.SetDefaultLifetime<PerRequestLifeTime>(); //this line have no effect since this hard override in ctr.CreateServiceProvider
            //**********************************************************************************
            ctr.RegisterAssembly(typeof(Startup).GetTypeInfo().Assembly);
            
            IServiceProvider provider = ctr.CreateServiceProvider(services);
            SetProvider(provider);
            return provider;
        }

        public static void SetProvider(IServiceProvider provider)
        {
            Startup.provider = provider;
        }

        public static IServiceProvider GetProvider()
        {
            return provider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseMvc();

            //just to force ef infrastrcture configuration
            var ctx = app.ApplicationServices.GetService<MyContext>();
            using (ctx)
            {
                ctx.Users.Add(new User());
                ctx.SaveChanges();
            }

        }
    }
}