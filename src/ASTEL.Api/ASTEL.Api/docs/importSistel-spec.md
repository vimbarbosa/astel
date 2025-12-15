# Especificação do Endpoint ImportSistel

## Visão Geral

O endpoint `ImportSistel` permite importar dados financeiros a partir de um arquivo Excel, utilizando a **Matrícula Sistel** para identificar os registros cadastrais e extraindo automaticamente o mês e ano do campo **DATA_PAGAMENTO**.

## Endpoint

```
POST /api/Import/importSistel
```

## Diferenças em relação ao ImportFinanceiroExcel

| Aspecto | ImportFinanceiroExcel | ImportSistel |
|---------|----------------------|--------------|
| Identificação | Usa Matrícula Astel | Usa Matrícula Sistel |
| Data | Requer campos Ano e Mês separados | Extrai de DATA_PAGAMENTO |
| Busca IdDadosCadastrais | JOIN por MatriculaAstel | JOIN por MatriculaSistel |

## Requisição

### Content-Type
```
multipart/form-data
```

### Parâmetros

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| file | File | Sim | Arquivo Excel (.xlsx ou .xls) |

### Estrutura do Arquivo Excel

O arquivo Excel deve conter uma planilha com os seguintes campos (na primeira linha como cabeçalho):

#### Layout da Planilha

A planilha deve seguir exatamente este layout (colunas na ordem especificada):

| PATROCINADORA | MATRICULA | NOME | REGPAT | DATA_PAGAMENTO | VALOR | VERBA |
|---------------|-----------|------|--------|---------------|-------|-------|

#### Campos Obrigatórios

1. **MATRICULA**
   - Nome exato (case-insensitive): "MATRICULA"
   - Tipo: Numérico (BIGINT)
   - Descrição: Matrícula Sistel do associado (somente este campo, sem variações)
   - Obrigatório: Sim

2. **DATA_PAGAMENTO**
   - Nome exato (case-insensitive): "DATA_PAGAMENTO"
   - Tipo: Data
   - Formatos aceitos:
     - DateTime do Excel
     - Número serial do Excel (OLE Automation Date)
     - String no formato DD/MM/YYYY
   - Descrição: Data do pagamento (mês e ano são extraídos automaticamente)
   - Obrigatório: Sim

3. **VALOR**
   - Nome exato (case-insensitive): "VALOR"
   - Tipo: Numérico (Double)
   - Descrição: Valor do pagamento realizado
   - Obrigatório: Sim

#### Campos Ignorados

Os seguintes campos são lidos mas não são utilizados no processamento:
- **PATROCINADORA**: Ignorado
- **NOME**: Ignorado
- **REGPAT**: Ignorado
- **VERBA**: Ignorado

### Exemplo de Estrutura do Excel

| PATROCINADORA | MATRICULA | NOME | REGPAT | DATA_PAGAMENTO | VALOR | VERBA |
|---------------|-----------|------|--------|----------------|-------|-------|
| Empresa A     | 123456789012345 | João Silva | 123456 | 15/10/2025 | 150.50 | 100 |
| Empresa B     | 987654321098765 | Maria Santos | 654321 | 20/10/2025 | 200.00 | 200 |

## Processamento

### Fluxo de Processamento

1. **Validação do Arquivo**
   - Verifica se o arquivo foi enviado
   - Valida extensão (.xlsx ou .xls)
   - Verifica se o arquivo não está vazio

2. **Leitura do Excel**
   - Abre a primeira planilha do workbook
   - Lê o cabeçalho da primeira linha
   - Mapeia os nomes das colunas (case-insensitive)

3. **Processamento das Linhas**
   Para cada linha (a partir da linha 2):
   - Extrai o campo MATRICULA (Matrícula Sistel)
   - Extrai e converte DATA_PAGAMENTO para DateTime
   - Extrai o mês e ano da data
   - Extrai o campo VALOR
   - Valida os dados:
     - MATRICULA > 0
     - DATA_PAGAMENTO válida
     - Ano > 0
     - Mês entre 1 e 12
   - Busca o IdDadosCadastrais usando JOIN com MatriculaSistel
   - Se encontrado, adiciona à lista de processamento

4. **Inserção/Atualização em Lote**
   - Cria tabela temporária
   - Faz bulk insert dos dados
   - Executa MERGE na tabela DadosFinanceiros:
     - Se o registro existe (mesmo IdDadosCadastrais + Ano + Mês):
       - Se ValorPago = 0 ou NULL: DELETE
       - Caso contrário: UPDATE ValorPago
     - Se o registro não existe: INSERT

### Geração do ID

O ID do registro em DadosFinanceiros é gerado pela concatenação:
```
Id = IdDadosCadastrais + Ano + Mês
```

Exemplo: Se IdDadosCadastrais = 123456, Ano = 2025, Mês = 10
```
Id = 123456202510
```

## Respostas

### Sucesso (200 OK)

```json
{
  "message": "150 registros de dados financeiros processados com sucesso!"
}
```

### Erro - Arquivo não enviado (400 Bad Request)

```json
{
  "message": "Nenhum arquivo foi enviado."
}
```

### Erro - Extensão inválida (400 Bad Request)

```json
{
  "message": "Arquivo deve ser Excel (.xlsx ou .xls)."
}
```

### Erro - Arquivo vazio (400 Bad Request)

```json
{
  "message": "Arquivo Excel vazio ou sem cabeçalhos válidos."
}
```

### Erro - Nenhum registro válido (400 Bad Request)

```json
{
  "message": "Nenhum registro válido encontrado no arquivo Excel."
}
```

## Regras de Negócio

1. **Validação de MATRICULA**
   - Linhas com MATRICULA = 0 ou vazia são ignoradas
   - Se a MATRICULA (Matrícula Sistel) não for encontrada em DadosCadastrais, a linha é ignorada

2. **Validação de Data**
   - Linhas sem DATA_PAGAMENTO válida são ignoradas
   - Se DATA_PAGAMENTO não estiver disponível, tenta usar campos "Ano" e "Mês" separados
   - Ano deve ser > 0
   - Mês deve estar entre 1 e 12

3. **Valor Pago**
   - Aceita valores decimais (separador: ponto ou vírgula)
   - Se ValorPago = 0 ou NULL e o registro já existe, ele é removido
   - Se ValorPago > 0, o registro é inserido ou atualizado

4. **Duplicatas**
   - Se já existir um registro com mesmo IdDadosCadastrais + Ano + Mês, o valor é atualizado
   - O sistema agrupa por IdDadosCadastrais, Ano e Mês, usando MAX(ValorPago) em caso de duplicatas

## Exemplo de Uso

### cURL

```bash
curl -X POST "http://localhost:5000/api/Import/importSistel" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@ASTEL-SP_DESCONTO_102025.xlsx"
```

### JavaScript (Fetch API)

```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);

fetch('http://localhost:5000/api/Import/importSistel', {
  method: 'POST',
  body: formData
})
.then(response => response.json())
.then(data => {
  console.log('Sucesso:', data.message);
})
.catch(error => {
  console.error('Erro:', error);
});
```

### C# (HttpClient)

```csharp
using var client = new HttpClient();
using var formData = new MultipartFormDataContent();
var fileContent = new ByteArrayContent(File.ReadAllBytes("ASTEL-SP_DESCONTO_102025.xlsx"));
fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
formData.Add(fileContent, "file", "ASTEL-SP_DESCONTO_102025.xlsx");

var response = await client.PostAsync("http://localhost:5000/api/Import/importSistel", formData);
var result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);
```

## Tratamento de Erros

### Erros Comuns e Soluções

1. **"Nenhum arquivo foi enviado"**
   - Verifique se o arquivo está sendo enviado corretamente no form-data
   - Confirme que o nome do campo é "file"

2. **"Arquivo deve ser Excel (.xlsx ou .xls)"**
   - Verifique a extensão do arquivo
   - Certifique-se de que o arquivo não está corrompido

3. **"Arquivo Excel vazio ou sem cabeçalhos válidos"**
   - Verifique se a primeira linha contém os cabeçalhos
   - Confirme que os nomes dos campos estão corretos

4. **"Nenhum registro válido encontrado"**
   - Verifique se as Matrículas Sistel existem em DadosCadastrais
   - Confirme que o campo DATA_PAGAMENTO está no formato correto
   - Verifique se há dados válidos nas linhas (não apenas cabeçalho)

## Logs e Debugging

O sistema registra erros no console durante o processamento:
- Linhas com erro são logadas com: `❌ Erro ao processar linha {row}: {ex.Message}`
- Linhas ignoradas (sem matrícula ou data válida) não geram logs

## Notas Técnicas

- O processamento é feito em lote para melhor performance
- A tabela temporária é criada e removida automaticamente
- O MERGE garante atomicidade (inserção/atualização em uma única operação)
- Suporta notação científica para números grandes (ex: 1.23E+14)

## Changelog

### Versão 1.0.0
- Implementação inicial do endpoint ImportSistel
- Suporte a importação usando Matrícula Sistel
- Extração automática de mês/ano de DATA_PAGAMENTO

