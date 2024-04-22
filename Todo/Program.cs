using Microsoft.EntityFrameworkCore;
using Todo.DataAccess;
using Microsoft.AspNetCore.Identity;

namespace Todo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            string connectionStr = builder.Configuration.GetConnectionString("SqlConnection");
            builder.Services.AddDbContext<ToDoDBContext>(options => options.UseSqlServer(connectionStr));

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ToDoDBContext>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=UserAuth}/{action=Login}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}
