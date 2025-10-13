using ASTEL.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

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
            // Configuração da chave composta para DadosFinanceiros
            modelBuilder.Entity<DadosFinanceiros>()
                .HasKey(df => new { df.MatriculaSistel, df.MatriculaAstel, df.Ano });

            // Configuração do relacionamento entre DadosCadastrais e DadosFinanceiros
            modelBuilder.Entity<DadosFinanceiros>()
                .HasOne(df => df.DadosCadastrais)
                .WithMany(dc => dc.DadosFinanceiros)
                .HasForeignKey(df => df.MatriculaSistel);

            base.OnModelCreating(modelBuilder);
        }
    }
}
