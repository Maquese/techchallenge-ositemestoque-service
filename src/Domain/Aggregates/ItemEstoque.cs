using Domain.Entidades;

namespace Domain.Aggregates;

public class ItemEstoque : IEntity
{
    public int Id { get; private set; }
    public string Nome { get;private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime DataAtualizacao { get; private set; }    
    public DateTime? Datavalidade { get; private set; }
    public int QuantidadeEmEstoque { get; private set; }
    public ICollection<OrdemServicoItemEstoque> OrdemServicoItensEstoque { get; private set; }
    public string Tipo { get; private set; }
    public string UnidadeMedida { get; private set; }

     protected ItemEstoque() { }

    public ItemEstoque(string tipo, string nome, string descricao, decimal valor, string unidadeMedida, DateTime? dataValidade = null)
      {
        if (string.IsNullOrEmpty(nome))
            throw new ArgumentException("O nome do item de estoque é obrigatório.");
        if (string.IsNullOrEmpty(descricao))
            throw new ArgumentException("A descrição do item de estoque é obrigatória.");
        if (valor < 0)
            throw new ArgumentException("O valor do item de estoque não pode ser negativo.");
        if(tipo != "Peça" && tipo != "Insumo")
            throw new ArgumentException("O tipo do item de estoque deve ser 'Peça' ou 'Insumo'.");
        if(string.IsNullOrEmpty(unidadeMedida))
            throw new ArgumentException("A unidade de medida do item de estoque é obrigatória.");
        
        UnidadeMedida = unidadeMedida;
        Tipo = tipo;
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
        DataCadastro = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
        Datavalidade = dataValidade;
        QuantidadeEmEstoque = 0;
        Ativo = true;
        OrdemServicoItensEstoque = new List<OrdemServicoItemEstoque>();
    }

    public void Atualizar(string nome, string descricao, decimal valor, string tipo, string unidadeMedida, DateTime? dataValidade = null)
    {
        if (string.IsNullOrEmpty(nome))
            throw new ArgumentException("O nome do item de estoque é obrigatório.");
        if (string.IsNullOrEmpty(descricao))
            throw new ArgumentException("A descrição do item de estoque é obrigatória.");
        if (valor < 0)
            throw new ArgumentException("O valor do item de estoque não pode ser negativo.");
        if(tipo != "Peça" && tipo != "Insumo")
            throw new ArgumentException("O tipo do item de estoque deve ser 'Peça' ou 'Insumo'.");
        if(string.IsNullOrEmpty(unidadeMedida))
            throw new ArgumentException("A unidade de medida do item de estoque é obrigatória.");
        Tipo = tipo;
        UnidadeMedida = unidadeMedida;
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
        DataAtualizacao = DateTime.UtcNow;
        Datavalidade = dataValidade;
    }

    public void Inativar()
    {
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AdicionarQuantidadeEstoque(int quantidade)
    {
        if (quantidade < 0)
            throw new ArgumentException("A quantidade a ser adicionada não pode ser negativa.");

        QuantidadeEmEstoque += quantidade;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DeduzirQuantidadeEstoque(int quantidade)
    {
        if (quantidade < 0)
            throw new ArgumentException("A quantidade a ser removida não pode ser negativa.");
        if (quantidade > QuantidadeEmEstoque)
            throw new InvalidOperationException("Não é possível remover mais peças do que o estoque disponível.");

        QuantidadeEmEstoque -= quantidade;
        DataAtualizacao = DateTime.UtcNow;
    }

    
}
