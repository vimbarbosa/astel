using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;
using ClosedXML.Excel;

namespace ASTEL.Api.Services
{
    public class ImportFinanceiroService
    {
        private readonly string _connectionString;

        public ImportFinanceiroService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=sqlserver,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";

            //_connectionString = configuration.GetConnectionString("DefaultConnection")
            //    ?? "Server=localhost,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
        }

        public async Task<byte[]> ExportCsvAsync()
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT 
            MatriculaSistel AS [Matrícula Sistel Nº],
            MatriculaAstel AS [ID Cliente],
            Ano AS [Ano],
            Mes AS [Mês],
            ValorPago AS [Valor Pago]
        FROM DadosFinanceiros
        ORDER BY MatriculaSistel, Ano, Mes;
    ";

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var sb = new StringBuilder();
            // Cabeçalho atualizado
            sb.AppendLine("Matrícula Sistel Nº;ID Cliente;Ano;Mês;Valor Pago (R$)");

            while (await reader.ReadAsync())
            {
                long matriculaSistel = reader.GetInt64(0);
                long matriculaAstel = reader.GetInt64(1);
                int ano = reader.GetInt32(2);
                double mes = reader.GetDouble(3);
                string valorPago = reader.IsDBNull(4)
                    ? ""
                    : reader.GetDouble(4).ToString("F2", CultureInfo.InvariantCulture);

                sb.AppendLine($"{matriculaSistel};{matriculaAstel};{ano};{mes.ToString(CultureInfo.InvariantCulture)};{valorPago}");
            }

            var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return utf8WithBom.GetBytes(sb.ToString());
        }



        public async Task<string> ImportCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Nenhum arquivo CSV foi enviado.";

            var dataTable = CriarEstruturaTabela();
            using var reader = new StreamReader(file.OpenReadStream(), DetectEncoding(file), true);

            string? headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
                return "Arquivo CSV vazio ou inválido.";

            var headers = headerLine
                .Split(';')
                .Select((h, i) => new { Header = h.Trim().ToLower(), Index = i })
                .Where(x => !string.IsNullOrEmpty(x.Header))
                .GroupBy(x => x.Header)
                .Select(g => g.First())
                .ToDictionary(x => x.Header, x => x.Index);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var cols = line.Split(';');
                if (cols.Length < headers.Count)
                    continue;

                var row = PreencherDataRow(dataTable, headers, cols);
                if (row != null)
                    dataTable.Rows.Add(row);
            }

            if (dataTable.Rows.Count == 0)
                return "Nenhum registro válido encontrado no arquivo CSV.";

            await InserirOuAtualizarEmLoteAsync(dataTable);
            return $"{dataTable.Rows.Count} registros de dados financeiros processados com sucesso!";
        }

        public async Task<byte[]> ExportCadastraisCsvAsync()
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT
            MatriculaSistel AS [Matrícula Sistel Nº],
            MatriculaAstel AS [ID Cliente],
            Nome AS [Nome],
            Endereco AS [Endereço],
            Situacao AS [Situação],
            ValorBeneficio AS [Valor do Benefício],
            EstadoCivil AS [Estado Civil],
            Telefone AS [Telefone],
            NomeEsposa AS [Nome da Esposa],
            CPF AS [CPF],
            RG AS [RG],
            Ativo AS [Ativo],
            DescontoFolha AS [Desconto em Folha],
            FormaPagamento AS [Forma de Pagamento]
        FROM DadosCadastrais
        ORDER BY MatriculaSistel;
    ";

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Matrícula Sistel Nº;ID Cliente;Nome;Endereço;Situação;Valor do Benefício;Estado Civil;Telefone;Nome da Esposa;CPF;RG;Ativo;Desconto em Folha;Forma de Pagamento");

            while (await reader.ReadAsync())
            {
                string matriculaSistel = reader["Matrícula Sistel Nº"]?.ToString() ?? "";
                string matriculaAstel = reader["ID Cliente"]?.ToString() ?? "";
                string nome = reader["Nome"]?.ToString() ?? "";
                string endereco = reader["Endereço"]?.ToString() ?? "";
                string situacao = reader["Situação"]?.ToString() ?? "";
                string valorBeneficio = reader["Valor do Benefício"]?.ToString() ?? "";
                string estadoCivil = reader["Estado Civil"]?.ToString() ?? "";
                string telefone = reader["Telefone"]?.ToString() ?? "";
                string nomeEsposa = reader["Nome da Esposa"]?.ToString() ?? "";
                string cpf = reader["CPF"]?.ToString() ?? "";
                string rg = reader["RG"]?.ToString() ?? "";
                string ativo = (reader["Ativo"] is bool b1) ? (b1 ? "Sim" : "Não") : "";
                string desconto = (reader["Desconto em Folha"] is bool b2) ? (b2 ? "Sim" : "Não") : "";
                string formaPagamento = reader["Forma de Pagamento"]?.ToString() ?? "";

                sb.AppendLine($"{matriculaSistel};{matriculaAstel};{nome};{endereco};{situacao};{valorBeneficio};{estadoCivil};{telefone};{nomeEsposa};{cpf};{rg};{ativo};{desconto};{formaPagamento}");
            }

            var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return utf8WithBom.GetBytes(sb.ToString());
        }


        private DataTable CriarEstruturaTabela()
        {
            var table = new DataTable();
            table.Columns.AddRange(new[]
            {
                new DataColumn("MatriculaSistel", typeof(long)),
                new DataColumn("MatriculaAstel", typeof(long)),
                new DataColumn("Ano", typeof(int)),
                new DataColumn("Mes", typeof(double)),
                new DataColumn("ValorPago", typeof(double))
            });
            return table;
        }


        private async Task InserirOuAtualizarEmLoteAsync(DataTable dataTable)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var createTempTable = @"
        IF OBJECT_ID('tempdb..#TempDadosFinanceiros') IS NOT NULL DROP TABLE #TempDadosFinanceiros;
        CREATE TABLE #TempDadosFinanceiros (
            MatriculaSistel BIGINT,
            MatriculaAstel BIGINT,
            Ano INT,
            Mes FLOAT,
            ValorPago FLOAT
        );
    ";
            await using (var createCmd = new SqlCommand(createTempTable, connection))
                await createCmd.ExecuteNonQueryAsync();

            using (var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "#TempDadosFinanceiros",
                BatchSize = 1000
            })
            {
                foreach (DataColumn col in dataTable.Columns)
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                await bulkCopy.WriteToServerAsync(dataTable);
            }

            // MERGE usando IdDadosCadastrais obtido via JOIN com DadosCadastrais
            // Inclui Mes na cláusula ON para evitar atualização duplicada
            // Gera Id concatenando IdDadosCadastrais + Ano + Mes (mesmo padrão do método Add)
            // Usa CAST(CONCAT(...) AS BIGINT) para compatibilidade com SQL Server 2012+
            // Agrupa por IdDadosCadastrais, Ano e Mes para evitar duplicatas (pega MAX(ValorPago))
            // Quando ValorPago for 0 ou NULL, remove o registro da base
            var mergeSql = @"
                    MERGE INTO DadosFinanceiros AS Target
                    USING (
                        SELECT 
                            CAST(CONCAT(CAST(c.Id AS VARCHAR), CAST(t.Ano AS VARCHAR), CAST(CAST(t.Mes AS INT) AS VARCHAR)) AS BIGINT) AS Id,
                            c.Id AS IdDadosCadastrais,
                            t.Ano,
                            CAST(t.Mes AS INT) AS Mes,
                            MAX(t.ValorPago) AS ValorPago
                        FROM #TempDadosFinanceiros t
                        INNER JOIN DadosCadastrais c 
                            ON (t.MatriculaAstel = c.MatriculaAstel)
                        WHERE t.MatriculaAstel IS NOT NULL AND t.MatriculaAstel > 0
                           AND t.Ano IS NOT NULL
                           AND t.Mes IS NOT NULL
                        GROUP BY c.Id, t.Ano, CAST(t.Mes AS INT)
                    ) AS Source
                    ON Target.Id = Source.Id
                    WHEN MATCHED AND (Source.ValorPago IS NULL OR Source.ValorPago = 0) THEN
                        DELETE
                    WHEN MATCHED AND (Source.ValorPago IS NOT NULL AND Source.ValorPago <> 0) THEN
                        UPDATE SET 
                            Target.ValorPago = Source.ValorPago
                    WHEN NOT MATCHED BY TARGET AND (Source.ValorPago IS NOT NULL AND Source.ValorPago <> 0) THEN
                        INSERT (Id, IdDadosCadastrais, Ano, Mes, ValorPago)
                        VALUES (Source.Id, Source.IdDadosCadastrais, Source.Ano, Source.Mes, Source.ValorPago);
    ";

            await using (var mergeCmd = new SqlCommand(mergeSql, connection))
                await mergeCmd.ExecuteNonQueryAsync();
        }


        private DataRow? PreencherDataRow(DataTable table, Dictionary<string, int> headers, string[] cols)
        {
            try
            {
                var row = table.NewRow();

                string? Get(string key)
                {
                    key = key.ToLowerInvariant();
                    if (!headers.ContainsKey(key))
                        return null;
                    int index = headers[key];
                    return index < cols.Length ? cols[index].Trim() : null;
                }

                string? matriculaSistelTexto = NormalizeScientificNotation(Get("matrícula sistel nº"));
                string? matriculaAstelTexto = NormalizeScientificNotation(Get("id cliente"));

                long matriculaSistel = ParseLong(matriculaSistelTexto);
                long matriculaAstel = ParseLong(matriculaAstelTexto);
                int ano = ParseInt(Get("ano"));
                double mes = ParseDouble(Get("mês") ?? Get("mes"));
                double valorPago = ParseDouble(Get("valor pago") ?? Get("valor pago (r$)"));

                row["MatriculaSistel"] = matriculaSistel;
                row["MatriculaAstel"] = matriculaAstel;
                row["Ano"] = ano;
                row["Mes"] = mes;
                row["ValorPago"] = valorPago;

                return row;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar linha: {string.Join(';', cols)}");
                Console.WriteLine($"   Detalhes: {ex.Message}");
                return null;
            }
        }


        private static Encoding DetectEncoding(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            char[] buffer = new char[1024];
            reader.Read(buffer, 0, buffer.Length);
            stream.Position = 0;

            if (buffer.Any(c => c == '�'))
                return Encoding.GetEncoding("ISO-8859-1");

            return Encoding.UTF8;
        }

        private static long ParseLong(string? value)
        {
            if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0;
        }

        private static int ParseInt(string? value)
        {
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0;
        }

        private static double ParseDouble(string? value)
        {
            if (double.TryParse(value?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0.0;
        }

        private static string NormalizeScientificNotation(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Tenta detectar padrão tipo "1.23E+14"
            if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^[0-9]+(\.[0-9]+)?[eE][\+\-]?[0-9]+$"))
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
                    return num.ToString("0", CultureInfo.InvariantCulture); // converte pra inteiro sem notação científica
            }

            return value;
        }

        public async Task<string> ImportExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Nenhum arquivo Excel foi enviado.";

            var dataTable = CriarEstruturaTabela();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1); // Primeira planilha

            // Lê o cabeçalho
            var headerRow = worksheet.Row(1);
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            var lastColumn = worksheet.LastColumnUsed();
            int maxColumn = lastColumn != null ? lastColumn.ColumnNumber() : 0;
            
            for (int col = 1; col <= maxColumn; col++)
            {
                var headerValue = headerRow.Cell(col).GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    headers[headerValue.ToLowerInvariant()] = col;
                }
            }

            if (headers.Count == 0)
                return "Arquivo Excel vazio ou sem cabeçalhos válidos.";

            // Processa as linhas
            var lastRow = worksheet.LastRowUsed();
            int maxRow = lastRow != null ? lastRow.RowNumber() : 1;
            
            for (int row = 2; row <= maxRow; row++)
            {
                var rowData = worksheet.Row(row);
                
                // Função auxiliar para obter valor da célula
                string? GetValue(string key)
                {
                    key = key.ToLowerInvariant();
                    if (!headers.ContainsKey(key))
                        return null;
                    int colIndex = headers[key];
                    var cell = rowData.Cell(colIndex);
                    
                    // Tenta obter como string primeiro, depois como número
                    var stringValue = cell.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(stringValue))
                        return stringValue.Trim();
                    
                    // Se for número, converte para string
                    if (cell.DataType == XLDataType.Number)
                    {
                        var numValue = cell.GetValue<double>();
                        return numValue.ToString(CultureInfo.InvariantCulture);
                    }
                    
                    return null;
                }

                try
                {
                    var dataRow = dataTable.NewRow();
                    
                    string? matriculaSistelTexto = NormalizeScientificNotation(GetValue("matrícula sistel nº") ?? GetValue("matricula sistel"));
                    string? matriculaAstelTexto = NormalizeScientificNotation(GetValue("id cliente") ?? GetValue("matricula astel"));
                    
                    long matriculaSistel = ParseLong(matriculaSistelTexto);
                    long matriculaAstel = ParseLong(matriculaAstelTexto);
                    int ano = ParseInt(GetValue("ano"));
                    double mes = ParseDouble(GetValue("mês") ?? GetValue("mes"));
                    double valorPago = ParseDouble(GetValue("valor pago") ?? GetValue("valor pago (r$)") ?? GetValue("valor"));

                    // Validação básica
                    if (matriculaSistel == 0 && matriculaAstel == 0)
                        continue; // Pula linha se não tiver matrícula

                    dataRow["MatriculaSistel"] = matriculaSistel;
                    dataRow["MatriculaAstel"] = matriculaAstel;
                    dataRow["Ano"] = ano;
                    dataRow["Mes"] = mes;
                    dataRow["ValorPago"] = valorPago;

                    dataTable.Rows.Add(dataRow);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao processar linha {row}: {ex.Message}");
                    continue;
                }
            }

            if (dataTable.Rows.Count == 0)
                return "Nenhum registro válido encontrado no arquivo Excel.";

            await InserirOuAtualizarEmLoteAsync(dataTable);
            return $"{dataTable.Rows.Count} registros de dados financeiros processados com sucesso!";
        }

        public async Task<string> ImportSistelExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Nenhum arquivo Excel foi enviado.";

            var dataTable = CriarEstruturaTabela();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1); // Primeira planilha

            // Lê o cabeçalho
            var headerRow = worksheet.Row(1);
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            var lastColumn = worksheet.LastColumnUsed();
            int maxColumn = lastColumn != null ? lastColumn.ColumnNumber() : 0;
            
            for (int col = 1; col <= maxColumn; col++)
            {
                var headerValue = headerRow.Cell(col).GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    headers[headerValue.ToLowerInvariant()] = col;
                }
            }

            if (headers.Count == 0)
                return "Arquivo Excel vazio ou sem cabeçalhos válidos.";

            // Processa as linhas
            var lastRow = worksheet.LastRowUsed();
            int maxRow = lastRow != null ? lastRow.RowNumber() : 1;
            
            for (int row = 2; row <= maxRow; row++)
            {
                var rowData = worksheet.Row(row);
                
                // Função auxiliar para obter valor da célula
                string? GetValue(string key)
                {
                    key = key.ToLowerInvariant();
                    if (!headers.ContainsKey(key))
                        return null;
                    int colIndex = headers[key];
                    var cell = rowData.Cell(colIndex);
                    
                    // Tenta obter como string primeiro, depois como número
                    var stringValue = cell.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(stringValue))
                        return stringValue.Trim();
                    
                    // Se for número, converte para string
                    if (cell.DataType == XLDataType.Number)
                    {
                        var numValue = cell.GetValue<double>();
                        return numValue.ToString(CultureInfo.InvariantCulture);
                    }
                    
                    // Se for data, converte para string no formato padrão
                    if (cell.DataType == XLDataType.DateTime)
                    {
                        var dateValue = cell.GetValue<DateTime>();
                        return dateValue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }
                    
                    return null;
                }

                // Função auxiliar para obter valor de data
                DateTime? GetDateValue(string key)
                {
                    key = key.ToLowerInvariant();
                    if (!headers.ContainsKey(key))
                        return null;
                    int colIndex = headers[key];
                    var cell = rowData.Cell(colIndex);
                    
                    // Tenta obter como DateTime
                    if (cell.DataType == XLDataType.DateTime)
                    {
                        return cell.GetValue<DateTime>();
                    }
                    
                    // Tenta obter como número (data serial do Excel)
                    if (cell.DataType == XLDataType.Number)
                    {
                        var numValue = cell.GetValue<double>();
                        return DateTime.FromOADate(numValue);
                    }
                    
                    // Tenta fazer parse da string
                    var stringValue = cell.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                            return date;
                    }
                    
                    return null;
                }

                try
                {
                    var dataRow = dataTable.NewRow();
                    
                    // Busca MATRICULA (somente matrícula sistel, conforme layout da planilha)
                    // Layout: PATROCINADORA | MATRICULA | NOME | REGPAT | DATA_PAGAMENTO | VALOR | VERBA
                    string? matriculaSistelTexto = NormalizeScientificNotation(GetValue("matricula"));
                    
                    long matriculaSistel = ParseLong(matriculaSistelTexto);
                    
                    // Extrai mês e ano do campo DATA_PAGAMENTO
                    DateTime? dataPagamento = GetDateValue("data_pagamento");
                    
                    int ano = 0;
                    int mes = 0;
                    
                    if (dataPagamento.HasValue)
                    {
                        ano = dataPagamento.Value.Year;
                        mes = dataPagamento.Value.Month;
                    }
                    else
                    {
                        // Se DATA_PAGAMENTO não estiver disponível, pula a linha
                        continue;
                    }
                    
                    // Busca VALOR (campo do layout da planilha)
                    double valorPago = ParseDouble(GetValue("valor"));

                    // Validação básica
                    if (matriculaSistel == 0)
                        continue; // Pula linha se não tiver matrícula sistel
                    
                    if (ano == 0 || mes == 0)
                        continue; // Pula linha se não tiver data válida

                    dataRow["MatriculaSistel"] = matriculaSistel;
                    dataRow["MatriculaAstel"] = 0; // Não usado na importação Sistel
                    dataRow["Ano"] = ano;
                    dataRow["Mes"] = mes;
                    dataRow["ValorPago"] = valorPago;

                    dataTable.Rows.Add(dataRow);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao processar linha {row}: {ex.Message}");
                    continue;
                }
            }

            if (dataTable.Rows.Count == 0)
                return "Nenhum registro válido encontrado no arquivo Excel.";

            await InserirOuAtualizarEmLoteSistelAsync(dataTable);
            return $"{dataTable.Rows.Count} registros de dados financeiros processados com sucesso!";
        }

        private async Task InserirOuAtualizarEmLoteSistelAsync(DataTable dataTable)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var createTempTable = @"
        IF OBJECT_ID('tempdb..#TempDadosFinanceiros') IS NOT NULL DROP TABLE #TempDadosFinanceiros;
        CREATE TABLE #TempDadosFinanceiros (
            MatriculaSistel BIGINT,
            MatriculaAstel BIGINT,
            Ano INT,
            Mes FLOAT,
            ValorPago FLOAT
        );
    ";
            await using (var createCmd = new SqlCommand(createTempTable, connection))
                await createCmd.ExecuteNonQueryAsync();

            using (var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "#TempDadosFinanceiros",
                BatchSize = 1000
            })
            {
                foreach (DataColumn col in dataTable.Columns)
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                await bulkCopy.WriteToServerAsync(dataTable);
            }

            // MERGE usando IdDadosCadastrais obtido via JOIN com DadosCadastrais usando MatriculaSistel
            var mergeSql = @"
                    MERGE INTO DadosFinanceiros AS Target
                    USING (
                        SELECT 
                            CAST(CONCAT(CAST(c.Id AS VARCHAR), CAST(t.Ano AS VARCHAR), CAST(CAST(t.Mes AS INT) AS VARCHAR)) AS BIGINT) AS Id,
                            c.Id AS IdDadosCadastrais,
                            t.Ano,
                            CAST(t.Mes AS INT) AS Mes,
                            MAX(t.ValorPago) AS ValorPago
                        FROM #TempDadosFinanceiros t
                        INNER JOIN DadosCadastrais c 
                            ON (t.MatriculaSistel = c.MatriculaSistel)
                        WHERE t.MatriculaSistel IS NOT NULL AND t.MatriculaSistel > 0
                           AND t.Ano IS NOT NULL
                           AND t.Mes IS NOT NULL
                        GROUP BY c.Id, t.Ano, CAST(t.Mes AS INT)
                    ) AS Source
                    ON Target.Id = Source.Id
                    WHEN MATCHED AND (Source.ValorPago IS NULL OR Source.ValorPago = 0) THEN
                        DELETE
                    WHEN MATCHED AND (Source.ValorPago IS NOT NULL AND Source.ValorPago <> 0) THEN
                        UPDATE SET 
                            Target.ValorPago = Source.ValorPago
                    WHEN NOT MATCHED BY TARGET AND (Source.ValorPago IS NOT NULL AND Source.ValorPago <> 0) THEN
                        INSERT (Id, IdDadosCadastrais, Ano, Mes, ValorPago)
                        VALUES (Source.Id, Source.IdDadosCadastrais, Source.Ano, Source.Mes, Source.ValorPago);
    ";

            await using (var mergeCmd = new SqlCommand(mergeSql, connection))
                await mergeCmd.ExecuteNonQueryAsync();
        }
    }
}
