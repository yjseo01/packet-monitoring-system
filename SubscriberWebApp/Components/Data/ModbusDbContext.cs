using Microsoft.EntityFrameworkCore;
using SubscriberWebApp.Components.Models;

public class ModbusDbContext : DbContext
{
    public ModbusDbContext(DbContextOptions<ModbusDbContext> options) : base(options)
    {

    }

    public DbSet<ModbusData> ModbusData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite("Data Source=modbus.db");
    }
}