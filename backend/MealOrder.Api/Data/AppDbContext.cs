using Microsoft.EntityFrameworkCore;
using MealOrder.Api.Models;

namespace MealOrder.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
}
