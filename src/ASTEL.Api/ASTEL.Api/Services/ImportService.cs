using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace ASTEL.Api.Services
{
    public class ImportService
    {
        private readonly string _connectionString;

        public ImportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AstelDb")
                ?? "Server=localhost,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
        }

        public async Task<string> ImportCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Nenhum arquivo CSV foi enviado.";

            var dataTable = CriarEstruturaTabela();
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);

            string? header = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(header))
                return "Arquivo CSV vazio ou inválido.";

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var cols = line.Split(';');
                if (cols.Length < 13)
                    continue;

                try
                {
                    // ❗ valida campos obrigatórios
                    if (string.IsNullOrWhiteSpace(cols[0]) ||
                        string.IsNullOrWhiteSpace(cols[1]) ||
                        string.IsNullOrWhiteSpace(cols[2]))
                        continue; // ignora linha inválida

                    var row = dataTable.NewRow();

                    row["MatriculaSistel"] = int.TryParse(cols[0], out var sistel) ? sistel : 0;
                    row["MatriculaAstel"] = int.TryParse(cols[1], out var astel) ? astel : 0;
                    row["Nome"] = cols[2].Trim();

                    row["Endereco"] = string.IsNullOrWhiteSpace(cols[3]) ? DBNull.Value : cols[3].Trim();
                    row["Situacao"] = int.TryParse(cols[4], out var sit) ? sit : DBNull.Value;
                    row["ValorBeneficio"] = double.TryParse(cols[5], out var val) ? val : DBNull.Value;
                    row["EstadoCivil"] = string.IsNullOrWhiteSpace(cols[6]) ? DBNull.Value : cols[6].Trim();
                    row["Telefone"] = string.IsNullOrWhiteSpace(cols[7]) ? DBNull.Value : cols[7].Trim();
                    row["NomeEsposa"] = string.IsNullOrWhiteSpace(cols[8]) ? DBNull.Value : cols[8].Trim();
                    row["CPF"] = string.IsNullOrWhiteSpace(cols[9]) ? DBNull.Value : cols[9].Trim();
                    row["RG"] = string.IsNullOrWhiteSpace(cols[10]) ? DBNull.Value : cols[10].Trim();
                    row["Ativo"] = cols[11].Trim() == "1";
                    row["DescontoFolha"] = cols[12].Trim() == "1";

                    dataTable.Rows.Add(row);
                }
                catch
                {
                    // ignora linha inválida
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
                new DataColumn("MatriculaSistel", typeof(int)),
                new DataColumn("MatriculaAstel", typeof(int)),
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
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var createTempTable = @"
                IF OBJECT_ID('tempdb..#TempDadosCadastrais') IS NOT NULL DROP TABLE #TempDadosCadastrais;
                CREATE TABLE #TempDadosCadastrais (
                    MatriculaSistel INT,
                    MatriculaAstel INT,
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

            // Bulk insert para staging
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

            // Merge na tabela real
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
    }
}
