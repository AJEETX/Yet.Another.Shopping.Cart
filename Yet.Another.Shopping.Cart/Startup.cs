using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yet.Another.Shopping.Cart.Infrastructure;
using Yet.Another.Shopping.Cart.Infrastructure.EFModels;
using Yet.Another.Shopping.Cart.Infrastructure.EFRepository;
using Yet.Another.Shopping.Cart.Infrastructure.Services.Catalog;
using Yet.Another.Shopping.Cart.Infrastructure.Services.Messages;
using Yet.Another.Shopping.Cart.Infrastructure.Services.Sale;
using Yet.Another.Shopping.Cart.Infrastructure.Services.Statistics;
using Yet.Another.Shopping.Cart.Infrastructure.Services.User;
using Yet.Another.Shopping.Cart.Web.Areas.Admin.Helpers;
using Yet.Another.Shopping.Cart.Web.Helpers;
using Yet.Another.Shopping.Cart.Web.Middleware;
using Yet.Another.Shopping.Cart.Web.Models;

namespace Yet.Another.Shopping.Cart
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets("aspnet-aspCart.Web-b7b6c0c8-2794-41a1-ad6c-528772b97f8a");
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            WebHostEnvironment = env;

            MapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfileConfiguration());
            });
        }

        public IConfigurationRoot Configuration { get; }
        public MapperConfiguration MapperConfiguration { get; set; }
        private readonly IWebHostEnvironment WebHostEnvironment;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            // idenity password requirement
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            });

            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            // configure admin account injectable
            services.Configure<AdminAccount>(Configuration.GetSection("AdminAccount"));

            services.Configure<UserAccount>(Configuration.GetSection("UserAccount"));
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });


            // Add application services.
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IBillingAddressService, BillingAddressService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IImageManagerService, ImageManagerService>();
            services.AddTransient<IManufacturerService, ManufacturerService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IReviewService, ReviewService>();
            services.AddTransient<ISpecificationService, SpecificationService>();

            services.AddTransient<IOrderCountService, OrderCountService>();
            services.AddScoped<IVisitorCountService, VisitorCountService>();

            services.AddTransient<IContactUsService, ContactUsService>();

            // singleton
            services.AddSingleton(sp => MapperConfiguration.CreateMapper());
            services.AddScoped<ViewHelper>();
            services.AddScoped<DataHelper>();
            services.AddSingleton<IFileProvider>(WebHostEnvironment.ContentRootFileProvider);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // redirect http request to https with 301 status code
                var options = new RewriteOptions().AddRedirectToHttpsPermanent();
                app.UseRewriter(options);
            }
            app.UseImageResize();
            app.UseStaticFiles();
            app.UseStatusCodePages();
 
            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();
            app.UseVisitorCounter();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areaRoute",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "productInfo",
                    pattern: "Product/{seo}",
                    defaults: new { controller = "Home", action = "ProductInfo" });
                endpoints.MapControllerRoute(
                    name: "category",
                    pattern: "Category/{category}",
                    defaults: new { controller = "Home", action = "ProductCategory" });
                endpoints.MapControllerRoute(
                    name: "manufacturer",
                    pattern: "manufacturer/{manufacturer}",
                    defaults: new { controller = "Home", action = "ProductManufacturer" });
                endpoints.MapControllerRoute(
                    name: "productSearch",
                    pattern: "search/{name?}",
                    defaults: new { controller = "Home", action = "ProductSearch" });
                endpoints.MapControllerRoute(
                    name: "create review",
                    pattern: "CreateReview/{id}",
                    defaults: new { controller = "Home", action = "CreateReview" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            if (Configuration.GetValue<bool>("Migrate"))
            {
                // apply migration
                SampleDataProvider.ApplyMigration(app.ApplicationServices);
            }
            if (Configuration.GetValue<bool>("Seed"))
            {
                // seed default data
                SampleDataProvider.Seed(app.ApplicationServices, Configuration, WebHostEnvironment);
            }
        }
    }
}
