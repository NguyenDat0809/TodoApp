using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Todo.Models;

namespace Todo.DataAccess
{
    public class ToDoDBContext : IdentityDbContext
    {
        public ToDoDBContext(DbContextOptions<ToDoDBContext> options) : base(options)
        {

        }
        public DbSet<ToDo> ToDos { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Category> Categories { get; set; }

        //seed data
        //the base function
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //}
        //rewrite it
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
                new Category() { CategoryId = "work", Name = "Work" },
                new Category() { CategoryId = "home", Name = "Home" },
                new Category() { CategoryId = "ex", Name = "Exercise" },
                new Category() { CategoryId = "shop", Name = "Shop" },
                new Category() { CategoryId = "call", Name = "Call" }
                );
            modelBuilder.Entity<Status>().HasData(
                new Status() { StatusId = "open", Name = "Open" },
                new Status() { StatusId = "close", Name = "Close" }
                );
        }
    }
}
