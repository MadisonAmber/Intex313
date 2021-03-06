using Intex313.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Intex313
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
            services.AddControllersWithViews();


            services.AddDbContext<AccidentDbContext>(options =>
            {
                options.UseNpgsql(Configuration["CONNECTION_STRING"]);
            });

            services.AddControllersWithViews();

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddEntityFrameworkStores<AccidentDbContext>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });

            //Add for ONNX file
            services.AddSingleton<InferenceSession>(
              new InferenceSession("wwwroot/Utah_Crash_Data.onnx")
            );

            //Repository method
            //services.AddScoped<IAccidentRepository, EFAccidentRepository>();
            services.AddRazorPages();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
            });
            services.AddHsts(options =>
            {
                options.ExcludedHosts.Clear();
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });
            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{            }
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseForwardedHeaders();

            app.UseHsts();

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseCsp(opts => opts
                .BlockAllMixedContent()
                    .ScriptSources(s => s.Self())
                    .ScriptSources(s => s.UnsafeInline())
                    .ScriptSources(s => s.CustomSources("https://use.fontawesome.com"))
                    .ScriptSources(s => s.CustomSources("https://kit.fontawesome.com"))
                    .ConnectSources(s => s.CustomSources("*.fontawesome.com"))
                );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });
        }
    }
}
