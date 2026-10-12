using Domain.Aggregates;

namespace Domain.Entidades;

public class OrdemServicoItemEstoque : IEntity
{
    public int Id { get; private set; }
    public int OrdemServicoId { get; private set; }
    public OrdemServico OrdemServico { get; private set; }
    public int ItemEstoqueId { get; private set; }
    public ItemEstoque ItemEstoque { get; private set; }
    public int Quantidade { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public bool Ativo { get; private set; }
    ///para v2 válido ter valor aqui


    protected OrdemServicoItemEstoque() { }

    public OrdemServicoItemEstoque(int ordemServicoId, int itemEstoqueId, int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.");

        OrdemServicoId = ordemServicoId;
        ItemEstoqueId = itemEstoqueId;
        Quantidade = quantidade;
        DataCadastro = DateTime.UtcNow;
        Ativo = true;
    }

    public void AtualizarQuantidade(int novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.");

        Quantidade = novaQuantidade;
    }
}
