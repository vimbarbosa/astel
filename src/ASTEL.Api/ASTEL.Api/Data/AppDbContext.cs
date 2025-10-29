using ASTEL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ASTEL.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<DadosCadastrais> DadosCadastrais { get; set; }
        public DbSet<DadosFinanceiros> DadosFinanceiros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DadosFinanceiros>()
                .HasKey(df => new { df.MatriculaSistel, df.MatriculaAstel, df.Ano, df.Mes });

            modelBuilder.Entity<DadosFinanceiros>()
                .HasOne(df => df.DadosCadastrais)
                .WithMany(dc => dc.DadosFinanceiros)
                .HasForeignKey(df => df.MatriculaSistel);

            base.OnModelCreating(modelBuilder);
        }
    }
}
