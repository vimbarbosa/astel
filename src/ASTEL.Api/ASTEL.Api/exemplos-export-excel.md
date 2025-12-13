# Exemplos de Payloads para ExportarExcel

O endpoint `GET /api/DadosFinanceiros/export/excel/xlsx` aceita os seguintes parâmetros de query:

## Parâmetros Disponíveis

### Filtros:
- `dataInicio` (DateTime?) - Formato: `yyyy-MM-dd` ou `yyyy-MM-ddTHH:mm:ss`
- `dataFim` (DateTime?) - Formato: `yyyy-MM-dd` ou `yyyy-MM-ddTHH:mm:ss`
- `nome` (string?)
- `cpf` (string?)
- `matriculaAstel` (long?)
- `inadimplente` (bool?) - `true` ou `false`
- `cidade` (string?)
- `estado` (string?)
- `email` (string?)
- `telefone` (string?)
- `descontoFolha` (bool?) - `true` ou `false`

### Colunas (columns):
Lista de strings com os nomes das colunas desejadas. Se não informado, exporta todas as colunas.

**Colunas disponíveis:**
- Id, IdCadastro, MatriculaSistel, MatriculaAstel, Nome, CPF, RG
- Logradouro, Numero, Complemento, Bairro, Cidade, Estado
- TipoEndereco, Correspondencia, CEP, Telefone, CelSkype, Email
- Situacao, EstadoCivil, Ativo, DescontoFolha, Ano, Mes, ValorPago, Inadimplente

---

## Exemplos de Requisições

### 1. Exportar todas as colunas (sem filtros)
```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx" \
  -H "Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  -o financeiro_export.xlsx
```

### 2. Exportar colunas específicas (sem filtros)
```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx?columns=CPF&columns=RG&columns=Logradouro&columns=Numero&columns=Complemento&columns=Bairro&columns=Cidade&columns=Estado&columns=TipoEndereco&columns=Correspondencia&columns=CEP&columns=Telefone&columns=Email&columns=EstadoCivil&columns=Ativo&columns=Ano&columns=Mes&columns=ValorPago&columns=Inadimplente" \
  -H "Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  -o financeiro_export.xlsx
```

### 3. Exportar com filtro de data
```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx?dataInicio=2024-01-01&dataFim=2024-12-31" \
  -H "Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  -o financeiro_export.xlsx
```

### 4. Exportar inadimplentes de uma cidade específica
```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx?inadimplente=true&cidade=São Paulo&columns=Nome&columns=CPF&columns=Cidade&columns=ValorPago&columns=Inadimplente" \
  -H "Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  -o financeiro_export.xlsx
```

### 5. Exportar por matrícula e colunas específicas
```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx?matriculaAstel=12345&columns=MatriculaAstel&columns=Nome&columns=CPF&columns=Ano&columns=Mes&columns=ValorPago" \
  -H "Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  -o financeiro_export.xlsx
```

### 6. Exportar com múltiplos filtros
```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx?dataInicio=2024-01-01&dataFim=2024-12-31&estado=SP&inadimplente=false&descontoFolha=true&columns=Nome&columns=CPF&columns=Estado&columns=ValorPago&columns=DescontoFolha" \
  -H "Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  -o financeiro_export.xlsx
```

### 7. Exportar por CPF específico
```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx?cpf=12345678900&columns=Nome&columns=CPF&columns=Email&columns=Telefone&columns=Ano&columns=Mes&columns=ValorPago" \
  -H "Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  -o financeiro_export.xlsx
```

### 8. Exportar com filtro por nome (busca parcial)
```bash
curl -X GET "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx?nome=João&columns=Nome&columns=CPF&columns=Email&columns=Telefone&columns=Cidade&columns=Estado" \
  -H "Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  -o financeiro_export.xlsx
```

---

## Exemplos em JavaScript/TypeScript (Fetch API)

### Exemplo 1: Exportar todas as colunas
```javascript
const response = await fetch('http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx', {
  method: 'GET',
  headers: {
    'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
  }
});

const blob = await response.blob();
const url = window.URL.createObjectURL(blob);
const a = document.createElement('a');
a.href = url;
a.download = 'financeiro_export.xlsx';
a.click();
```

### Exemplo 2: Exportar com filtros e colunas específicas
```javascript
const params = new URLSearchParams({
  dataInicio: '2024-01-01',
  dataFim: '2024-12-31',
  inadimplente: 'true',
  cidade: 'São Paulo'
});

// Adicionar múltiplas colunas
['Nome', 'CPF', 'Cidade', 'ValorPago', 'Inadimplente'].forEach(col => {
  params.append('columns', col);
});

const response = await fetch(
  `http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx?${params.toString()}`,
  {
    method: 'GET',
    headers: {
      'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    }
  }
);

const blob = await response.blob();
const url = window.URL.createObjectURL(blob);
const a = document.createElement('a');
a.href = url;
a.download = 'financeiro_export.xlsx';
a.click();
```

---

## Exemplos em C# (HttpClient)

```csharp
using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://localhost:5000");

var queryParams = new List<KeyValuePair<string, string>>
{
    new("dataInicio", "2024-01-01"),
    new("dataFim", "2024-12-31"),
    new("inadimplente", "true"),
    new("cidade", "São Paulo")
};

// Adicionar colunas
queryParams.Add(new("columns", "Nome"));
queryParams.Add(new("columns", "CPF"));
queryParams.Add(new("columns", "ValorPago"));

var queryString = string.Join("&", queryParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

var response = await httpClient.GetAsync($"/api/DadosFinanceiros/export/excel/xlsx?{queryString}");
var bytes = await response.Content.ReadAsByteArrayAsync();

await File.WriteAllBytesAsync("financeiro_export.xlsx", bytes);
```

---

## Exemplos em PowerShell

```powershell
$baseUrl = "http://localhost:5000/api/DadosFinanceiros/export/excel/xlsx"
$params = @{
    dataInicio = "2024-01-01"
    dataFim = "2024-12-31"
    inadimplente = "true"
    cidade = "São Paulo"
    columns = @("Nome", "CPF", "ValorPago", "Inadimplente")
}

$queryString = ""
$params.GetEnumerator() | ForEach-Object {
    if ($_.Value -is [Array]) {
        foreach ($item in $_.Value) {
            $queryString += "&$($_.Key)=$([Uri]::EscapeDataString($item))"
        }
    } else {
        $queryString += "&$($_.Key)=$([Uri]::EscapeDataString($_.Value))"
    }
}
$queryString = $queryString.TrimStart('&')

$response = Invoke-WebRequest -Uri "$baseUrl?$queryString" -Method Get
[System.IO.File]::WriteAllBytes("financeiro_export.xlsx", $response.Content)
```

---

## Notas Importantes

1. **Formato de Data**: Use `yyyy-MM-dd` ou `yyyy-MM-ddTHH:mm:ss` (ISO 8601)
2. **Booleanos**: Use `true` ou `false` (string)
3. **Colunas**: Se uma coluna não existir, ela será ignorada silenciosamente
4. **Encoding**: URLs devem ser codificadas corretamente (espaços viram `%20`, etc.)
5. **Response**: O endpoint retorna um arquivo Excel (.xlsx) com Content-Type apropriado

