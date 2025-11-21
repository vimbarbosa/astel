using ASTEL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ASTEL.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<DadosFinanceirosGridDTO> DadosFinanceirosGridDTO { get; set; }
        public DbSet<DadosCadastrais> DadosCadastrais { get; set; }
        public DbSet<DadosFinanceiros> DadosFinanceiros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DadosCadastrais>()
                .HasKey(dc => dc.Id);

            modelBuilder.Entity<DadosFinanceiros>()
                .HasKey(df => df.Id);

            modelBuilder.Entity<DadosFinanceiros>()
                .HasOne(df => df.DadosCadastrais)
                .WithMany(dc => dc.DadosFinanceiros)
                .HasForeignKey(df => df.IdDadosCadastrais)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<DadosFinanceirosGridDTO>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }
    }
}