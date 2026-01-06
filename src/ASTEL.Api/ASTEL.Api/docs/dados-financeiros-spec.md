# Especificação da API - Dados Financeiros

## Base URL
```
/api/DadosFinanceiros
```

---

## 1. Filtrar Dados Financeiros

### Endpoint
```
GET /api/DadosFinanceiros/filtrar
```

### Descrição
Retorna uma lista paginada de dados financeiros com base em múltiplos filtros. Inclui informações de cadastro e financeiras, além de indicador de inadimplência.

### Parâmetros de Query

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `dataInicio` | DateTime? | Não | Data inicial para filtro (formato: yyyy-MM-dd ou yyyy-MM-ddTHH:mm:ss) |
| `dataFim` | DateTime? | Não | Data final para filtro (formato: yyyy-MM-dd ou yyyy-MM-ddTHH:mm:ss) |
| `nome` | string? | Não | Filtro por nome (busca parcial, case-insensitive) |
| `cpf` | string? | Não | Filtro por CPF |
| `inadimplente` | bool? | Não | Filtro por status de inadimplência (true = inadimplente, false = em dia) |
| `cidade` | string? | Não | Filtro por cidade |
| `estado` | string? | Não | Filtro por estado |
| `email` | string? | Não | Filtro por email |
| `telefone` | string? | Não | Filtro por telefone |
| `descontoFolha` | bool? | Não | Filtro por desconto em folha |
| `formapagamento` | string? | Não | Filtro por forma de pagamento |
| `pageNumber` | int | Não | Número da página (padrão: 1) |
| `pageSize` | int | Não | Tamanho da página (padrão: 10) |

### Headers de Resposta

| Header | Descrição |
|--------|-----------|
| `X-Total-Count` | Total de registros encontrados (sem paginação) |
| `X-Page-Number` | Número da página atual |
| `X-Page-Size` | Tamanho da página |
| `X-Total-Pages` | Total de páginas |
| `X-Soma-Valor-Pago` | Soma total dos valores pagos (formato: F2) |

### Resposta de Sucesso (200 OK)

```json
[
  {
    "id": "123456789012023",
    "ano": 2023,
    "mes": 12,
    "valorPago": 1500.50,
    "data_Pagamento": "2023-12-15T10:30:00",
    "idDadosCadastrais": 1234567890,
    "matriculaSistel": 987654,
    "matriculaAstel": 1234567890,
    "nome": "João Silva",
    "cpf": "123.456.789-00",
    "rg": "12.345.678-9",
    "endereco": "Rua Exemplo, 123",
    "estadoCivil": "Casado",
    "telefone": "(11) 98765-4321",
    "situacao": "Ativo",
    "ativo": true,
    "descontoFolha": false,
    "logradouro": "Rua Exemplo",
    "celSkype": "(11) 98765-4321",
    "estado": "SP",
    "cidade": "São Paulo",
    "tipoEndereco": "Residencial",
    "correspondencia": true,
    "numero": "123",
    "complemento": "Apto 45",
    "bairro": "Centro",
    "email": "joao.silva@email.com",
    "cep": "01234-567",
    "formaPagamento": "Depósito",
    "inadimplente": false,
    "somaValorPago": 15000.00,
    "totalRegistros": 10
  }
]
```

### Exemplo de Requisição

```http
GET /api/DadosFinanceiros/filtrar?dataInicio=2023-01-01&dataFim=2023-12-31&nome=João&pageNumber=1&pageSize=20
```

---

## 2. Obter Dados Financeiros por ID

### Endpoint
```
GET /api/DadosFinanceiros/{id}
```

### Descrição
Retorna os dados financeiros de um registro específico pelo ID.

### Parâmetros de Rota

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `id` | long | Sim | ID do registro financeiro |

### Resposta de Sucesso (200 OK)

```json
{
  "id": "123456789012023",
  "ano": 2023,
  "mes": 12,
  "valorPago": 1500.50,
  "data_Pagamento": "2023-12-15T10:30:00",
  "idDadosCadastrais": 1234567890,
  "matriculaSistel": 987654,
  "matriculaAstel": 1234567890,
  "nome": "João Silva",
  "cpf": "123.456.789-00",
  "rg": "12.345.678-9",
  "endereco": "Rua Exemplo, 123",
  "estadoCivil": "Casado",
  "telefone": "(11) 98765-4321",
  "situacao": "Ativo",
  "ativo": true,
  "descontoFolha": false,
  "logradouro": "Rua Exemplo",
  "celSkype": "(11) 98765-4321",
  "estado": "SP",
  "cidade": "São Paulo",
  "tipoEndereco": "Residencial",
  "correspondencia": true,
  "numero": "123",
  "complemento": "Apto 45",
  "bairro": "Centro",
  "email": "joao.silva@email.com",
  "cep": "01234-567",
  "formaPagamento": "Depósito",
  "inadimplente": false,
  "somaValorPago": null,
  "totalRegistros": null
}
```

### Resposta de Erro (404 Not Found)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

### Exemplo de Requisição

```http
GET /api/DadosFinanceiros/123456789012023
```

---

## 3. Criar Dados Financeiros

### Endpoint
```
POST /api/DadosFinanceiros
```

### Descrição
Cria um novo registro de dados financeiros. O ID é gerado automaticamente concatenando `IdDadosCadastrais + Ano + Mes`.

### Corpo da Requisição

```json
{
  "idDadosCadastrais": 1234567890,
  "ano": 2023,
  "mes": 12,
  "valorPago": 1500.50,
  "data_Pagamento": "2023-12-15T10:30:00"
}
```

### Campos do Corpo

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `idDadosCadastrais` | long | Sim | ID do cadastro relacionado |
| `ano` | int? | Não | Ano do pagamento |
| `mes` | int? | Não | Mês do pagamento (1-12) |
| `valorPago` | double? | Não | Valor pago |
| `data_Pagamento` | DateTime? | Não | Data e hora do pagamento (formato: yyyy-MM-ddTHH:mm:ss) |

**Nota:** O campo `id` não deve ser enviado, pois é gerado automaticamente.

### Resposta de Sucesso (200 OK)

```json
{
  "message": "Pagamento cadastrado com sucesso!"
}
```

### Resposta de Erro (409 Conflict)

```json
{
  "message": "Erro ao atualizar pagamento existente."
}
```

### Resposta de Erro (500 Internal Server Error)

```json
{
  "message": "Erro ao salvar os dados financeiros.",
  "detail": "Mensagem de erro detalhada"
}
```

### Exemplo de Requisição

```http
POST /api/DadosFinanceiros
Content-Type: application/json

{
  "idDadosCadastrais": 1234567890,
  "ano": 2023,
  "mes": 12,
  "valorPago": 1500.50,
  "data_Pagamento": "2023-12-15T10:30:00"
}
```

---

## 4. Atualizar Dados Financeiros

### Endpoint
```
PUT /api/DadosFinanceiros/{id}
```

### Descrição
Atualiza um registro de dados financeiros existente.

### Parâmetros de Rota

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `id` | long | Sim | ID do registro financeiro |

### Corpo da Requisição

```json
{
  "idDadosCadastrais": 1234567890,
  "ano": 2023,
  "mes": 12,
  "valorPago": 2000.00,
  "data_Pagamento": "2023-12-20T14:00:00"
}
```

### Campos do Corpo

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `idDadosCadastrais` | long | Sim | ID do cadastro relacionado |
| `ano` | int? | Não | Ano do pagamento |
| `mes` | int? | Não | Mês do pagamento (1-12) |
| `valorPago` | double? | Não | Valor pago |
| `data_Pagamento` | DateTime? | Não | Data e hora do pagamento |

**Nota:** O `id` no corpo será ignorado. O ID da rota será usado.

### Resposta de Sucesso (204 No Content)

Sem corpo de resposta.

### Exemplo de Requisição

```http
PUT /api/DadosFinanceiros/123456789012023
Content-Type: application/json

{
  "idDadosCadastrais": 1234567890,
  "ano": 2023,
  "mes": 12,
  "valorPago": 2000.00,
  "data_Pagamento": "2023-12-20T14:00:00"
}
```

---

## 5. Deletar Dados Financeiros

### Endpoint
```
DELETE /api/DadosFinanceiros/{id}
```

### Descrição
Remove um registro de dados financeiros.

### Parâmetros de Rota

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `id` | long | Sim | ID do registro financeiro |

### Resposta de Sucesso (204 No Content)

Sem corpo de resposta.

### Resposta de Erro (404 Not Found)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

### Exemplo de Requisição

```http
DELETE /api/DadosFinanceiros/123456789012023
```

---

## 6. Exportar para Excel

### Endpoint
```
GET /api/DadosFinanceiros/export/excel/xlsx
```

### Descrição
Exporta os dados financeiros filtrados para um arquivo Excel (.xlsx). Permite selecionar quais colunas exportar.

### Parâmetros de Query

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `dataInicio` | DateTime? | Não | Data inicial para filtro |
| `dataFim` | DateTime? | Não | Data final para filtro |
| `nome` | string? | Não | Filtro por nome |
| `cpf` | string? | Não | Filtro por CPF |
| `inadimplente` | bool? | Não | Filtro por inadimplência |
| `cidade` | string? | Não | Filtro por cidade |
| `estado` | string? | Não | Filtro por estado |
| `email` | string? | Não | Filtro por email |
| `telefone` | string? | Não | Filtro por telefone |
| `descontoFolha` | bool? | Não | Filtro por desconto em folha |
| `formapagamento` | string? | Não | Filtro por forma de pagamento |
| `columns` | List<string>? | Não | Lista de colunas a exportar (ex: `columns=Nome&columns=CPF&columns=ValorPago`) |

### Colunas Disponíveis

- `Id`, `IdCadastro`, `MatriculaSistel`, `MatriculaAstel`, `Nome`, `CPF`, `RG`, `Logradouro`, `Numero`, `Complemento`, `Bairro`, `Cidade`, `Estado`, `TipoEndereco`, `Correspondencia`, `CEP`, `Telefone`, `CelSkype`, `Email`, `Situacao`, `EstadoCivil`, `Ativo`, `DescontoFolha`, `FormaPagamento`, `Ano`, `Mes`, `ValorPago`, `Data_Pagamento`, `Inadimplente`

**Nota:** Se `columns` não for fornecido, todas as colunas serão exportadas.

### Resposta de Sucesso (200 OK)

- **Content-Type:** `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- **Content-Disposition:** `attachment; filename=financeiro_export.xlsx`
- Corpo: Arquivo binário Excel

### Exemplo de Requisição

```http
GET /api/DadosFinanceiros/export/excel/xlsx?dataInicio=2023-01-01&dataFim=2023-12-31&columns=Nome&columns=CPF&columns=ValorPago
```

---

## 7. Gerar Modelo de Importação

### Endpoint
```
GET /api/DadosFinanceiros/gerar-modelo-importacao
```

### Descrição
Gera um arquivo Excel com os dados filtrados que pode ser usado como modelo para importação.

### Parâmetros de Query

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `dataInicio` | DateTime? | Não | Data inicial para filtro |
| `dataFim` | DateTime? | Não | Data final para filtro |
| `nome` | string? | Não | Filtro por nome |
| `cpf` | string? | Não | Filtro por CPF |
| `inadimplente` | bool? | Não | Filtro por inadimplência |
| `cidade` | string? | Não | Filtro por cidade |
| `estado` | string? | Não | Filtro por estado |
| `email` | string? | Não | Filtro por email |
| `telefone` | string? | Não | Filtro por telefone |
| `descontoFolha` | bool? | Não | Filtro por desconto em folha |
| `formapagamento` | string? | Não | Filtro por forma de pagamento |

### Resposta de Sucesso (200 OK)

- **Content-Type:** `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- **Content-Disposition:** `attachment; filename=modelo_importacao.xlsx`
- Corpo: Arquivo binário Excel

### Exemplo de Requisição

```http
GET /api/DadosFinanceiros/gerar-modelo-importacao?dataInicio=2023-01-01&dataFim=2023-12-31
```

---

## 8. Obter Histórico de Pagamento por Usuário

### Endpoint
```
GET /api/DadosFinanceiros/historico/{idDadosCadastrais}
```

### Descrição
Retorna o histórico de pagamentos de um usuário específico. Retorna até 1000 registros mais recentes.

### Parâmetros de Rota

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `idDadosCadastrais` | long | Sim | ID do cadastro do usuário |

### Resposta de Sucesso (200 OK)

```json
[
  {
    "id": 123456789012023,
    "idDadosCadastrais": 1234567890,
    "ano": 2023,
    "mes": 12,
    "valorPago": 1500.50,
    "data_Pagamento": "2023-12-15T10:30:00"
  },
  {
    "id": 1234567890112023,
    "idDadosCadastrais": 1234567890,
    "ano": 2023,
    "mes": 11,
    "valorPago": 1500.50,
    "data_Pagamento": "2023-11-15T10:30:00"
  }
]
```

### Campos da Resposta

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | long | ID do registro financeiro |
| `idDadosCadastrais` | long | ID do cadastro |
| `ano` | int? | Ano do pagamento |
| `mes` | int? | Mês do pagamento |
| `valorPago` | double? | Valor pago |
| `data_Pagamento` | DateTime? | Data e hora do pagamento |

**Nota:** Os registros são ordenados por ano e mês em ordem decrescente (mais recentes primeiro).

### Exemplo de Requisição

```http
GET /api/DadosFinanceiros/historico/1234567890
```

---

## 9. Obter Dados Financeiros por Cadastro

### Endpoint
```
GET /api/DadosFinanceiros/por-cadastro
```

### Descrição
Retorna registros financeiros com base em filtros de cadastro e data. Se todos os parâmetros forem nulos, retorna os últimos 1000 registros da base.

### Parâmetros de Query

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `idDadosCadastrais` | long? | Não | ID do cadastro para filtrar |
| `dataInicio` | DateTime? | Não | Data inicial para filtro (formato: yyyy-MM-dd ou yyyy-MM-ddTHH:mm:ss) |
| `dataFim` | DateTime? | Não | Data final para filtro (formato: yyyy-MM-dd ou yyyy-MM-ddTHH:mm:ss) |

### Comportamento Especial

- **Se todos os parâmetros forem nulos:** Retorna os últimos 1000 registros da base, ordenados por ano e mês decrescente.
- **Se apenas `idDadosCadastrais` for fornecido:** Retorna todos os registros daquele usuário (sem limite de 1000).
- **Se houver qualquer filtro de data:** Retorna todos os registros que correspondem aos filtros (sem limite de 1000).

### Resposta de Sucesso (200 OK)

```json
[
  {
    "id": 123456789012023,
    "idDadosCadastrais": 1234567890,
    "nome": "João Silva",
    "ano": 2023,
    "mes": 12,
    "valorPago": 1500.50,
    "data_Pagamento": "2023-12-15T10:30:00"
  },
  {
    "id": 1234567890112023,
    "idDadosCadastrais": 1234567890,
    "nome": "João Silva",
    "ano": 2023,
    "mes": 11,
    "valorPago": 1500.50,
    "data_Pagamento": "2023-11-15T10:30:00"
  }
]
```

### Campos da Resposta

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | long | ID do registro financeiro |
| `idDadosCadastrais` | long | ID do cadastro |
| `nome` | string? | Nome do usuário (obtido da tabela DadosCadastrais) |
| `ano` | int? | Ano do pagamento |
| `mes` | int? | Mês do pagamento |
| `valorPago` | double? | Valor pago |
| `data_Pagamento` | DateTime? | Data e hora do pagamento |

**Nota:** Os registros são ordenados por ano e mês em ordem decrescente (mais recentes primeiro).

### Exemplos de Requisição

```http
# Retornar últimos 1000 registros
GET /api/DadosFinanceiros/por-cadastro

# Filtrar por usuário específico
GET /api/DadosFinanceiros/por-cadastro?idDadosCadastrais=1234567890

# Filtrar por usuário e período
GET /api/DadosFinanceiros/por-cadastro?idDadosCadastrais=1234567890&dataInicio=2023-01-01&dataFim=2023-12-31

# Filtrar apenas por período
GET /api/DadosFinanceiros/por-cadastro?dataInicio=2023-01-01&dataFim=2023-12-31
```

---

## Modelos de Dados

### DadosFinanceirosDTO

```typescript
interface DadosFinanceirosDTO {
  // Financeiro
  id?: string;
  ano?: number;
  mes?: number;
  valorPago?: number;
  data_Pagamento?: string; // ISO 8601 DateTime

  // Cadastro
  idDadosCadastrais: number;
  matriculaSistel?: number;
  matriculaAstel?: number;
  nome: string;
  cpf: string;
  rg: string;
  endereco: string;
  estadoCivil: string;
  telefone: string;
  situacao: string;
  ativo?: boolean;
  descontoFolha?: boolean;

  // Campos adicionais
  logradouro?: string;
  celSkype?: string;
  estado?: string;
  cidade?: string;
  tipoEndereco?: string;
  correspondencia?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  email?: string;
  cep?: string;
  formaPagamento?: string;

  // Flags e totais
  inadimplente: boolean;
  somaValorPago?: number; // Preenchido apenas no endpoint Filtrar
  totalRegistros?: number; // Preenchido apenas no endpoint Filtrar
}
```

### HistoricoPagamentoDTO

```typescript
interface HistoricoPagamentoDTO {
  id: number;
  idDadosCadastrais: number;
  nome?: string; // Disponível apenas no endpoint por-cadastro
  ano?: number;
  mes?: number;
  valorPago?: number;
  data_Pagamento?: string; // ISO 8601 DateTime
}
```

### DadosFinanceiros (Modelo para Create/Update)

```typescript
interface DadosFinanceiros {
  id?: number; // Gerado automaticamente no Create, ignorado no Update
  idDadosCadastrais: number;
  ano?: number;
  mes?: number;
  valorPago?: number;
  data_Pagamento?: string; // ISO 8601 DateTime
}
```

---

## Códigos de Status HTTP

| Código | Descrição |
|--------|-----------|
| 200 | Sucesso - Requisição processada com sucesso |
| 204 | Sucesso - Sem conteúdo (Delete, Update) |
| 404 | Não encontrado - Recurso não existe |
| 409 | Conflito - Erro ao processar (ex: duplicata) |
| 500 | Erro interno do servidor |

---

## Observações Importantes

1. **Geração de ID:** O ID dos dados financeiros é gerado automaticamente concatenando `IdDadosCadastrais + Ano + Mes` (ex: `1234567890 + 2023 + 12 = 123456789012023`).

2. **Inadimplência:** O cálculo de inadimplência é baseado na existência de pagamento no último mês do período filtrado (ou no mês atual, se não houver filtro de data).

3. **Paginação:** O endpoint `Filtrar` suporta paginação. Use os headers de resposta para navegação.

4. **Data_Pagamento:** Campo opcional que armazena a data e hora exata do pagamento. Pode ser `null` se não foi informado.

5. **Ordenação:** 
   - Endpoint `Filtrar`: Ordenado por ano e mês decrescente
   - Endpoint `historico/{id}`: Ordenado por ano e mês decrescente
   - Endpoint `por-cadastro`: Ordenado por ano e mês decrescente

6. **Filtros de Data:** Os filtros de data são aplicados usando `DATEFROMPARTS(Ano, Mes, 1)`, comparando apenas ano e mês, não o dia específico.

---

## Exemplos de Integração

### JavaScript/TypeScript (Fetch API)

```typescript
// Filtrar dados financeiros
const response = await fetch('/api/DadosFinanceiros/filtrar?pageNumber=1&pageSize=20&nome=João');
const data = await response.json();
const totalCount = response.headers.get('X-Total-Count');

// Criar novo pagamento
const novoPagamento = {
  idDadosCadastrais: 1234567890,
  ano: 2023,
  mes: 12,
  valorPago: 1500.50,
  data_Pagamento: "2023-12-15T10:30:00"
};

const createResponse = await fetch('/api/DadosFinanceiros', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(novoPagamento)
});

// Atualizar pagamento
const updateResponse = await fetch('/api/DadosFinanceiros/123456789012023', {
  method: 'PUT',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ ...novoPagamento, valorPago: 2000.00 })
});

// Deletar pagamento
const deleteResponse = await fetch('/api/DadosFinanceiros/123456789012023', {
  method: 'DELETE'
});
```

### cURL

```bash
# Filtrar
curl -X GET "http://localhost:5000/api/DadosFinanceiros/filtrar?pageNumber=1&pageSize=20&nome=João"

# Criar
curl -X POST "http://localhost:5000/api/DadosFinanceiros" \
  -H "Content-Type: application/json" \
  -d '{"idDadosCadastrais":1234567890,"ano":2023,"mes":12,"valorPago":1500.50}'

# Atualizar
curl -X PUT "http://localhost:5000/api/DadosFinanceiros/123456789012023" \
  -H "Content-Type: application/json" \
  -d '{"idDadosCadastrais":1234567890,"ano":2023,"mes":12,"valorPago":2000.00}'

# Deletar
curl -X DELETE "http://localhost:5000/api/DadosFinanceiros/123456789012023"
```

