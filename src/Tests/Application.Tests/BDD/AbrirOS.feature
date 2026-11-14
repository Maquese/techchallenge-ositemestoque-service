Feature: Saga de abertura da OS

Scenario: Compensar transação quando orçamento falha
    Given que o ClienteService responde com sucesso
    And que o Veiculo é criado com sucesso
    And que a Ordem de Serviço é criada com sucesso
    And que o OrcamentoService retorna erro
    When uma Ordem de Serviço é aberta
    Then as compensações devem ser executadas