using Application.Interfaces;
using Infra;
using Infra.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.UseCases.ItensEstoque;
using Application.UseCases.OrdensServico;
//using Application.Controllers;

namespace IOC;

 public static class BootStrapper
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddScoped<AuthService>();

            // var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            //     ?? configuration["ConnectionStrings:DefaultConnection"]
            //     ?? configuration.GetConnectionString("DefaultConnection");

            // if (string.IsNullOrWhiteSpace(connectionString))
            // {
            //     throw new InvalidOperationException("A connection string 'DefaultConnection' must be configured.");
            // }
            
            // // Console.WriteLine($"Using connection string: {connectionString}");
            // string connStr = "Server=localhost;Port=3306;Database=meu_db;User=root;Password=senha_root;";
            string connStr = "Server=mysql-clienteveiculo;Port=3306;Database=ClienteVeiculo;User=appuser;Password=clienteveiculoservice;";
Console.WriteLine($"Using connection string: {connStr}");
            services.AddDbContext<EFContext>(options =>
                options.UseInMemoryDatabase("connStr"));
            // using var serviceProvider = services.BuildServiceProvider();
            // using var scope = serviceProvider.CreateScope();
            // var dbContext = scope.ServiceProvider.GetRequiredService<EFContext>();
            // dbContext.Database.Migrate();


            services.AddTransient<ItemEstoqueRepository, ItemEstoqueRepositoryImp>();
            services.AddTransient<OrdemServicoRepository, OrdemServicoRepositoryImp>();
            services.AddTransient<ServicoRepository, ServicoRepositoryImp>();
            services.AddScoped<AdicionarOrdemServicoHandler>();
            services.AddScoped<AdicionarItemEstoqueHandler>();

            services.AddScoped<ConsutaStatusOSHandler>();
            services.AddScoped<ListarOrdensServicoOrdenadoHandler>();
            services.AddScoped<ObterItemEstoqueHandler>();
            services.AddScoped<ListarItemEstoqueHandler>();
            services.AddScoped<AdicionarQtdEstoqueItemEstoqueHandler>();
            services.AddScoped<InativarItemEstoqueHandler>();
            services.AddScoped<AtualizarItemEstoqueHandler>();
            services.AddScoped<AdicionarServicoHandler>();
            services.AddScoped<BuscarServicoHandler>();
            services.AddScoped<AtualizarServicoHandler>();
            services.AddScoped<InativarServicoHandler>();
            services.AddScoped<ListarServicosAtivosHandler>();
            services.AddScoped<AtribuirMecanicoDiagnosticoOSHandler>();
            services.AddScoped<FinalizarDiagnosticoOSHandler>();
            services.AddScoped<AtribuirMecanicoExecucaoOSHandler>();
            services.AddScoped<FinalizarOrdemServicoHandler>();
            services.AddScoped<StatusAtualOSClienteHandler>();
            services.AddScoped<TempoMedioExecucaoOSHandler>();

            //services.AddTransient<OrdemServicoAppController>();
        }
    }
