using Microsoft.EntityFrameworkCore;

namespace WoWAHDataProject.Database;

public class DataContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        try
        {
            Console.WriteLine("Connecting to database");
            try
            {
                Console.WriteLine("try");
                //optionsBuilder.UseSqlite($"Data Source={DatabaseMain.dbFilePath};");
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Error connecting to database");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e + "Error connecting to database");
        }
    }
    //public DbSet<Entities.Sales> Sales { get; set; }
    //public DbSet<Entities.Player> Player { get; set; }

    public DbSet<Entities.OtherPlayer> otherPlayer { get; set; }


    /*
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Entities.Sales>().HasKey(s => s.orderId);
        //modelBuilder.Entity<Entities.Player>().HasKey(p => p.playerId);
        //modelBuilder.Entity<Entities.OtherPlayer>().HasKey(op => op.otherPlayerId);
    }*/
}
