using CheckoutApi.IoC;
using CheckoutOrderService.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ninject;
using Ninject.Activation;
using Ninject.Infrastructure.Disposal;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CheckoutApi
{
    public class Startup
    {
        // Dependency Injection
        private readonly AsyncLocal<Scope> scopeProvider = new AsyncLocal<Scope>();
        private IKernel Kernel { get; set; }
        private object Resolve(Type type) => Kernel.Get(type);
        private object RequestScope(IContext context) => scopeProvider.Value;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddRequestScopingMiddleware(() => scopeProvider.Value = new Scope());
            services.AddCustomControllerActivation(Resolve);
            services.AddCustomViewComponentActivation(Resolve);
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("prototype", new Info
                {
                    Title = "Checkout Order API",
                    Description = "The prototype Checkout Order API"                    
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Cors Setup
            app.UseCors(options => options.WithOrigins("http://localhost:45000").AllowAnyMethod().AllowAnyHeader());

            // DI setup
            Kernel = RegisterApplicationComponents(app, loggerFactory);

            // Swagger docs setup
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/prototype/swagger.json", "Checkout Order API"); });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private IKernel RegisterApplicationComponents(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            // TODO Add duplicate registration checks when the next ServiceModule is added
            Kernel = new StandardKernel(
                new ApiModule(),
                new OrderServiceModule());

            // Register application services
            foreach (var ctrlType in app.GetControllerTypes())
            {
                Kernel.Bind(ctrlType).ToSelf().InScope(RequestScope);
            }

            // Cross-wire required framework services
            Kernel.BindToMethod(app.GetRequestService<IViewBufferScope>);
            Kernel.Bind<ILoggerFactory>().ToConstant(loggerFactory);

            return Kernel;
        }

        private sealed class Scope : DisposableObject { }
    }
}
