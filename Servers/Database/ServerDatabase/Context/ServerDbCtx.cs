using Database.ServerDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.ServerDatabase.Context;

public partial class ServerDbCtx : DbContext, IServerDbCtx
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string cs = "data source=localhost; Database=IotServerDb; Initial Catalog=IotServerDb; Integrated Security=True; Trust Server Certificate=true";
        optionsBuilder.UseSqlServer(cs);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Topic>()
            .HasIndex(top => top.Name)
            .IsUnique(true);
        
        modelBuilder.Entity<Device>()
            .HasIndex(dev => dev.Mac)
            .IsUnique(true);
        
        modelBuilder.Entity<DeviceModel>()
            .HasIndex(devMod => devMod.ModelName)
            .IsUnique(true);
        
        modelBuilder.Entity<Token>()
            .HasIndex(tok => tok.TokenHash)
            .IsUnique(true);
        
        modelBuilder.Entity<User>()
            .HasIndex(us => us.Email)
            .IsUnique(true);
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<TopicData> TopicDatas { get; set; }
    public DbSet<DeviceModel> DeviceModels { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<DeviceModelToTopics> DeviceModelToTopics { get; set; }
}