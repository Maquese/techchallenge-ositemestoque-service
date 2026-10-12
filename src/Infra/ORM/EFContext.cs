using Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Domain.VOs;
using Domain.Entidades;
namespace Infra;

public class EFContext : DbContext
{

    public DbSet<ItemEstoque> ItensEstoque { get; set; }
     public DbSet<OrdemServico> OrdemServicos { get; set; }
    public DbSet<Servico> Servicos { get; set; }
    public DbSet<OrdemServicoItemEstoque> OrdemServicoItensEstoque { get; set; }
    public EFContext(DbContextOptions<EFContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        

         modelBuilder.Entity<ItemEstoque>(entity =>
        {
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.ToTable("ItemEstoque");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(1000);
            //entity.Property(e => e.Valor).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.DataCadastro).IsRequired();
            entity.Property(e => e.DataAtualizacao).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.QuantidadeEmEstoque).IsRequired();
            entity.Property(e => e.Datavalidade).IsRequired(false);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UnidadeMedida).IsRequired().HasMaxLength(20);
        });
        modelBuilder.Entity<Servico>(entity =>
        {
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.ToTable("Servico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Valor).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.TempoEstimado).IsRequired();
        });

        modelBuilder.Entity<OrdemServico>(entity =>
        {
            entity.ToTable("OrdemServico");
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MecanicoAtribuido).HasMaxLength(200);
            entity.Property(o => o.VeiculoId)
                .IsRequired();

            entity.HasMany(o => o.Servicos)
                .WithMany(s => s.OrdemServicos)
                .UsingEntity(j => j.ToTable("OrdemServicoServicos"));
        });

        modelBuilder.Entity<OrdemServicoItemEstoque>(entity =>
        {
            entity.ToTable("OrdemServicoItensEstoque");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.Quantidade).IsRequired();
            entity.Property(e => e.DataCadastro).IsRequired();

            entity.HasOne(op => op.OrdemServico)
                .WithMany(o => o.OrdemServicoItensEstoque)
                .HasForeignKey(op => op.OrdemServicoId);

            entity.HasOne(op => op.ItemEstoque)
                .WithMany(ie => ie.OrdemServicoItensEstoque)
                .HasForeignKey(op => op.ItemEstoqueId);

            // Índice para evitar duplicatas
            entity.HasIndex(op => new { op.OrdemServicoId, op.ItemEstoqueId }).IsUnique();
        });
    }
}
