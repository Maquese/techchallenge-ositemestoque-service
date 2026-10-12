
using Domain.Entidades;

namespace Domain.Aggregates;

public class OrdemServico : IEntity
{   
    protected OrdemServico() { }

    public int Id { get; private set; }
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataFechamento { get; private set; }
    public int VeiculoId { get; private set; }
    //public Veiculo Veiculo { get; private set; }
    public string Status { get; private set; }
    public ICollection<Servico>? Servicos { get; private set; }
    public ICollection<OrdemServicoItemEstoque>? OrdemServicoItensEstoque { get; private set; }
    public string? MecanicoAtribuido { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFimExecucao { get; private set; }
    

    public OrdemServico(int veiculoId, ICollection<Servico>? servicos = null)
           
    {
        DataAbertura = DateTime.Now;
        DataFechamento = null;
        VeiculoId = veiculoId;
        Status = "Recebida";
        Servicos = servicos ?? new List<Servico>();
        OrdemServicoItensEstoque = new List<OrdemServicoItemEstoque>();
        MecanicoAtribuido = null;
        //Orcamentos = new List<Orcamento>();
        Ativo = true;
    }

    public void OSEmDiagnostico(string mecanico)
    {
        MecanicoAtribuido = mecanico;
        Status = "Em diagnóstico";
    }

    public void OSDiagnosticada(IList<OrdemServicoItemEstoque> itensEstoque)
    {
        OrdemServicoItensEstoque = itensEstoque;
        MecanicoAtribuido = null;
        Status = "Aguardando aprovação";
    }

    public void OSAprovada()
    {
        Status = "Aprovada";
    }
    
    public void OSNegada()
    {
        Status = "Negada";
    }

    public void EmExecucao(string mecanico)
    {
        MecanicoAtribuido = mecanico;
        Status = "Em execução";
        DataInicioExecucao = DateTime.Now;
    }

    public void FinalizarOrdemServico()
    {
        Status = "Finalizada";
        DataFimExecucao = DateTime.Now;
    }

    public void OrdemServicoEntregue()
    {
        Status = "Entregue";
        DataFechamento = DateTime.Now;
    }

    public double TempoMedioExecucao()
    {
        if (DataInicioExecucao.HasValue && DataFimExecucao.HasValue)
        {
            return (DataFimExecucao.Value - DataInicioExecucao.Value).TotalMilliseconds;
        }

        throw new InvalidOperationException("Datas de início e fim da execução não estão definidas.");
    }

    public decimal? CalcularValorOrdemServico()
    {
        return Servicos?.Sum(x => x.Valor) + OrdemServicoItensEstoque?.Sum(x => x.Quantidade * x.ItemEstoque.Valor);
    }
}
