using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;

namespace ASTEL.Api.Services
{
    public class ImportService
    {
        private readonly string _connectionString;

        public ImportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
        }

        public async Task<string> ImportCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Nenhum arquivo CSV foi enviado.";

            var dataTable = CriarEstruturaTabela();
            using var reader = new StreamReader(file.OpenReadStream(), DetectEncoding(file), true);

            // Lê o cabeçalho e mapeia as posições das colunas
            string? headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
                return "Arquivo CSV vazio ou inválido.";

            var headers = headerLine
                .Split(';')
                .Select((h, i) => new { Header = h.Trim().ToLower(), Index = i })
                .Where(x => !string.IsNullOrEmpty(x.Header))    // <-- ignora colunas sem nome
                .GroupBy(x => x.Header)                         // <-- agrupa nomes duplicados
                .Select(g => g.First())                         // <-- mantém a primeira ocorrência
                .ToDictionary(x => x.Header, x => x.Index);     // <-- cria o mapa seguro


            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var cols = line.Split(';');
                if (cols.Length < headers.Count)
                    continue;

                var row = PreencherDataRow(dataTable, headers, cols);
                if (row != null)
                {
                    // 🔍 Aqui você pode colocar um breakpoint e inspecionar o DataRow completo
                    dataTable.Rows.Add(row);
                }
            }

            if (dataTable.Rows.Count == 0)
                return "Nenhum registro válido encontrado no arquivo CSV.";

            await InserirOuAtualizarEmLoteAsync(dataTable);
            return $"{dataTable.Rows.Count} registros processados (inseridos/atualizados) com sucesso!";
        }



        private DataTable CriarEstruturaTabela()
        {
            var table = new DataTable();
            table.Columns.AddRange(new[]
            {
                new DataColumn("MatriculaSistel", typeof(long)),
                new DataColumn("MatriculaAstel", typeof(long)),
                new DataColumn("Nome", typeof(string)),
                new DataColumn("Endereco", typeof(string)),
                new DataColumn("Situacao", typeof(int)),
                new DataColumn("ValorBeneficio", typeof(double)),
                new DataColumn("EstadoCivil", typeof(string)),
                new DataColumn("Telefone", typeof(string)),
                new DataColumn("NomeEsposa", typeof(string)),
                new DataColumn("CPF", typeof(string)),
                new DataColumn("RG", typeof(string)),
                new DataColumn("Ativo", typeof(bool)),
                new DataColumn("DescontoFolha", typeof(bool))
            });
            return table;
        }

        private async Task InserirOuAtualizarEmLoteAsync(DataTable dataTable)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // 1️⃣ Cria uma tabela temporária
            var createTempTable = @"
                IF OBJECT_ID('tempdb..#TempDadosCadastrais') IS NOT NULL DROP TABLE #TempDadosCadastrais;
                CREATE TABLE #TempDadosCadastrais (
                    MatriculaSistel BIGINT,
                    MatriculaAstel BIGINT,
                    Nome VARCHAR(120),
                    Endereco VARCHAR(255),
                    Situacao INT,
                    ValorBeneficio FLOAT,
                    EstadoCivil VARCHAR(50),
                    Telefone VARCHAR(20),
                    NomeEsposa VARCHAR(120),
                    CPF VARCHAR(14),
                    RG VARCHAR(20),
                    Ativo BIT,
                    DescontoFolha BIT
                );
            ";
            await using (var createCmd = new SqlCommand(createTempTable, connection))
                await createCmd.ExecuteNonQueryAsync();

            // 2️⃣ Envia os dados para a tabela temporária
            using (var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "#TempDadosCadastrais",
                BatchSize = 1000
            })
            {
                foreach (DataColumn col in dataTable.Columns)
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                await bulkCopy.WriteToServerAsync(dataTable);
            }

            // 3️⃣ Faz o MERGE no SQL
            var mergeSql = @"
                MERGE INTO DadosCadastrais AS Target
                USING #TempDadosCadastrais AS Source
                ON Target.MatriculaAstel = Source.MatriculaAstel
                WHEN MATCHED THEN
                    UPDATE SET
                        Target.MatriculaSistel = Source.MatriculaSistel,
                        Target.Nome = Source.Nome,
                        Target.Endereco = Source.Endereco,
                        Target.Situacao = Source.Situacao,
                        Target.ValorBeneficio = Source.ValorBeneficio,
                        Target.EstadoCivil = Source.EstadoCivil,
                        Target.Telefone = Source.Telefone,
                        Target.NomeEsposa = Source.NomeEsposa,
                        Target.CPF = Source.CPF,
                        Target.RG = Source.RG,
                        Target.Ativo = Source.Ativo,
                        Target.DescontoFolha = Source.DescontoFolha
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT (MatriculaSistel, MatriculaAstel, Nome, Endereco, Situacao, ValorBeneficio, EstadoCivil, Telefone, NomeEsposa, CPF, RG, Ativo, DescontoFolha)
                    VALUES (Source.MatriculaSistel, Source.MatriculaAstel, Source.Nome, Source.Endereco, Source.Situacao, Source.ValorBeneficio, Source.EstadoCivil, Source.Telefone, Source.NomeEsposa, Source.CPF, Source.RG, Source.Ativo, Source.DescontoFolha);
            ";

            await using (var mergeCmd = new SqlCommand(mergeSql, connection))
                await mergeCmd.ExecuteNonQueryAsync();
        }

        private DataRow? PreencherDataRow(DataTable dataTable, Dictionary<string, int> headers, string[] cols)
        {
            try
            {
                var row = dataTable.NewRow();

                // 🔍 Função auxiliar segura
                string? Get(string key)
                {
                    key = key.ToLowerInvariant();
                    if (!headers.ContainsKey(key))
                        return null;
                    int index = headers[key];
                    return index < cols.Length ? cols[index].Trim() : null;
                }

                // 🧩 Captura de valores originais (para debug)
                string? matriculaSistelTexto = NormalizeScientificNotation(Get("matrícula sistel nº"));
                string? matriculaAstelTexto = NormalizeScientificNotation(Get("id cliente"));
                string? nomeTexto = NormalizeScientificNotation(Get("nome"));
                string? enderecoTexto = Get("endereço");
                string? situacaoTexto = Get("situação")?.Trim().ToUpperInvariant();
                string? estadoCivilTexto = Get("estado civil");
                string? telefoneTexto = Get("telefone");
                string? nomeEsposaTexto = Get("site");
                string? cpfTexto = NormalizeScientificNotation(Get("cpf / cnpj"));
                string? rgTexto = NormalizeScientificNotation(Get("rg / insc. est."));
                string? statusTexto = Get("status")?.Trim().ToUpperInvariant();

                // 🧠 Conversões seguras
                long matriculaSistel = long.TryParse(matriculaSistelTexto, out var sistel) ? sistel : 0;
                long matriculaAstel = long.TryParse(matriculaAstelTexto, out var astel) ? astel : 0;

                int? situacao = situacaoTexto switch
                {
                    "TITULAR" => 1,
                    "DEPENDENTE" => 2,
                    _ => null
                };

                bool? ativo = statusTexto switch
                {
                    "A" or "ATIVO" => true,
                    "I" or "INATIVO" => false,
                    _ => null
                };

                // 🧾 Atribuições
                row["MatriculaSistel"] = matriculaSistel;
                row["MatriculaAstel"] = matriculaAstel;
                row["Nome"] = nomeTexto ?? string.Empty;
                row["Endereco"] = enderecoTexto ?? string.Empty;
                row["Situacao"] = situacao.HasValue ? situacao.Value : DBNull.Value;
                row["ValorBeneficio"] = 0.0;
                row["EstadoCivil"] = estadoCivilTexto ?? string.Empty;
                row["Telefone"] = telefoneTexto ?? string.Empty;
                row["NomeEsposa"] = nomeEsposaTexto ?? string.Empty;
                row["CPF"] = cpfTexto ?? string.Empty;
                row["RG"] = rgTexto ?? string.Empty;
                row["Ativo"] = ativo.HasValue ? ativo.Value : DBNull.Value;
                row["DescontoFolha"] = 0;

                // ✅ Retorna a linha para permitir inspeção
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

            // Lê alguns bytes para forçar a detecção automática (BOM ou UTF-16/32)
            char[] buffer = new char[1024];
            reader.Read(buffer, 0, buffer.Length);

            // Se não detectou BOM e contém caracteres inválidos, tenta Latin1
            stream.Position = 0;
            if (buffer.Any(c => c == '�'))
                return Encoding.GetEncoding("ISO-8859-1"); // Latin1 / Windows-1252

            return Encoding.UTF8;
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

    }
}
