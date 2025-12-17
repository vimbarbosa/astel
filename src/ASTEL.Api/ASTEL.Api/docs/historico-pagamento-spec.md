# Especificação do Endpoint Histórico de Pagamentos

## Visão Geral

O endpoint `GetHistoricoPagamentoPorUsuario` permite consultar todo o histórico de pagamentos de um usuário específico, identificado pelo `IdDadosCadastrais`. Retorna até 1000 registros ordenados por ano e mês (mais recentes primeiro).

## Endpoint

```
GET /api/DadosFinanceiros/historico/{idDadosCadastrais}
```

## Requisição

### Método HTTP
```
GET
```

### Parâmetros da Rota

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| idDadosCadastrais | long | Sim | ID do cadastro do usuário (path parameter) |

### Exemplo de URL

```
GET /api/DadosFinanceiros/historico/123456
```

## Respostas

### Sucesso (200 OK)

Retorna um array JSON com os registros de pagamento do usuário.

#### Estrutura da Resposta

```json
[
  {
    "id": 123456202510,
    "idDadosCadastrais": 123456,
    "ano": 2025,
    "mes": 10,
    "valorPago": 150.50
  },
  {
    "id": 123456202509,
    "idDadosCadastrais": 123456,
    "ano": 2025,
    "mes": 9,
    "valorPago": 150.50
  },
  {
    "id": 123456202508,
    "idDadosCadastrais": 123456,
    "ano": 2025,
    "mes": 8,
    "valorPago": 0.0
  }
]
```

#### Campos do Objeto de Resposta

| Campo | Tipo | Nullable | Descrição |
|-------|------|----------|-----------|
| id | long | Não | ID único do registro de pagamento (gerado pela concatenação: IdDadosCadastrais + Ano + Mês) |
| idDadosCadastrais | long | Não | ID do cadastro do usuário |
| ano | int? | Sim | Ano do pagamento |
| mes | int? | Sim | Mês do pagamento (1-12) |
| valorPago | double? | Sim | Valor pago no período (pode ser null ou 0) |

### Resposta Vazia (200 OK)

Se o usuário não possuir histórico de pagamentos, retorna um array vazio:

```json
[]
```

### Erro - Parâmetro Inválido (400 Bad Request)

Se o `idDadosCadastrais` não for um número válido:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "traceId": "..."
}
```

## Ordenação

Os registros são retornados ordenados por:
1. **Ano** (descendente - mais recente primeiro)
2. **Mês** (descendente - mais recente primeiro)

## Limite de Registros

O endpoint retorna no máximo **1000 registros** (TOP 1000) para evitar sobrecarga.

## Regras de Negócio

1. **Validação do ID**
   - O `idDadosCadastrais` deve ser um número válido (long)
   - Não há validação se o ID existe na base de dados (retorna array vazio se não houver registros)

2. **Campos Nullable**
   - `ano` e `mes` podem ser `null` (registros incompletos)
   - `valorPago` pode ser `null` ou `0` (indicando ausência de pagamento)

3. **Geração do ID**
   - O ID é gerado pela concatenação: `IdDadosCadastrais + Ano + Mês`
   - Exemplo: Se `IdDadosCadastrais = 123456`, `Ano = 2025`, `Mês = 10`
     - `Id = 123456202510`

## Exemplo de Uso

### cURL

```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/historico/123456" \
  -H "Accept: application/json"
```

### JavaScript (Fetch API)

```javascript
const idDadosCadastrais = 123456;

fetch(`http://localhost:5000/api/DadosFinanceiros/historico/${idDadosCadastrais}`, {
  method: 'GET',
  headers: {
    'Accept': 'application/json'
  }
})
.then(response => response.json())
.then(data => {
  console.log('Histórico de pagamentos:', data);
  data.forEach(pagamento => {
    console.log(`${pagamento.mes}/${pagamento.ano}: R$ ${pagamento.valorPago}`);
  });
})
.catch(error => {
  console.error('Erro:', error);
});
```

### TypeScript (Fetch API com Tipos)

```typescript
interface HistoricoPagamentoDTO {
  id: number;
  idDadosCadastrais: number;
  ano: number | null;
  mes: number | null;
  valorPago: number | null;
}

async function buscarHistoricoPagamentos(idDadosCadastrais: number): Promise<HistoricoPagamentoDTO[]> {
  const response = await fetch(
    `http://localhost:5000/api/DadosFinanceiros/historico/${idDadosCadastrais}`,
    {
      method: 'GET',
      headers: {
        'Accept': 'application/json'
      }
    }
  );

  if (!response.ok) {
    throw new Error(`Erro ao buscar histórico: ${response.status}`);
  }

  return await response.json();
}

// Uso
const historico = await buscarHistoricoPagamentos(123456);
console.log(`Total de registros: ${historico.length}`);
```

### C# (HttpClient)

```csharp
using var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5000");

var idDadosCadastrais = 123456;
var response = await client.GetAsync($"/api/DadosFinanceiros/historico/{idDadosCadastrais}");

if (response.IsSuccessStatusCode)
{
    var historico = await response.Content.ReadFromJsonAsync<List<HistoricoPagamentoDTO>>();
    
    foreach (var pagamento in historico)
    {
        Console.WriteLine($"{pagamento.Mes}/{pagamento.Ano}: R$ {pagamento.ValorPago}");
    }
}
```

### React (Hook Customizado)

```typescript
import { useState, useEffect } from 'react';

interface HistoricoPagamentoDTO {
  id: number;
  idDadosCadastrais: number;
  ano: number | null;
  mes: number | null;
  valorPago: number | null;
}

function useHistoricoPagamentos(idDadosCadastrais: number | null) {
  const [historico, setHistorico] = useState<HistoricoPagamentoDTO[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!idDadosCadastrais) return;

    setLoading(true);
    setError(null);

    fetch(`/api/DadosFinanceiros/historico/${idDadosCadastrais}`)
      .then(response => {
        if (!response.ok) {
          throw new Error('Erro ao buscar histórico');
        }
        return response.json();
      })
      .then(data => {
        setHistorico(data);
        setLoading(false);
      })
      .catch(err => {
        setError(err.message);
        setLoading(false);
      });
  }, [idDadosCadastrais]);

  return { historico, loading, error };
}

// Uso no componente
function HistoricoPagamentosComponent({ userId }: { userId: number }) {
  const { historico, loading, error } = useHistoricoPagamentos(userId);

  if (loading) return <div>Carregando...</div>;
  if (error) return <div>Erro: {error}</div>;

  return (
    <table>
      <thead>
        <tr>
          <th>Mês/Ano</th>
          <th>Valor Pago</th>
        </tr>
      </thead>
      <tbody>
        {historico.map(pagamento => (
          <tr key={pagamento.id}>
            <td>{pagamento.mes}/{pagamento.ano}</td>
            <td>R$ {pagamento.valorPago?.toFixed(2) ?? '0.00'}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
```

## Tratamento de Erros

### Erros Comuns e Soluções

1. **404 Not Found**
   - Verifique se a rota está correta
   - Confirme que o endpoint está disponível

2. **400 Bad Request**
   - Verifique se o `idDadosCadastrais` é um número válido
   - Confirme que o parâmetro está sendo passado corretamente na URL

3. **500 Internal Server Error**
   - Erro interno do servidor
   - Verifique os logs do servidor para mais detalhes

## Performance

- **Limite de registros**: 1000 registros (TOP 1000)
- **Ordenação**: Por Ano e Mês (descendente) - otimizado por índice
- **Tipo de consulta**: SQL direto (não usa Entity Framework para melhor performance)

## Notas Técnicas

- A consulta é executada diretamente no banco de dados usando SQL
- Retorna apenas os campos essenciais (Id, IdDadosCadastrais, Ano, Mes, ValorPago)
- Não inclui informações do cadastro (apenas dados financeiros)
- A ordenação é feita no banco de dados (ORDER BY Ano DESC, Mes DESC)

## Casos de Uso

1. **Visualização de Histórico Completo**
   - Exibir todos os pagamentos de um usuário em uma tabela
   - Gráfico de histórico de pagamentos ao longo do tempo

2. **Análise de Inadimplência**
   - Identificar meses sem pagamento (valorPago = null ou 0)
   - Calcular total pago no período

3. **Relatórios**
   - Gerar relatório de pagamentos por usuário
   - Exportar histórico para Excel/PDF

## Changelog

### Versão 1.0.0
- Implementação inicial do endpoint de histórico de pagamentos
- Suporte a consulta por IdDadosCadastrais
- Retorno de até 1000 registros ordenados por data

