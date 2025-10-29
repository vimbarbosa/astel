using ASTEL.Api.Data;
using ASTEL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ASTEL.Api.Services
{
    public class DadosFinanceirosService
    {
        private readonly AppDbContext _context;

        public DadosFinanceirosService(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Retorna todos os registros financeiros
        public List<DadosFinanceiros> GetAll()
        {
            return _context.DadosFinanceiros
                .AsNoTracking()
                .OrderBy(df => df.MatriculaSistel)
                .ThenBy(df => df.Ano)
                .ThenBy(df => df.Mes)
                .ToList();
        }

        // 🔹 Busca um registro específico
        public DadosFinanceiros? GetById(long matriculaSistel, long matriculaAstel, int ano, double mes)
        {
            return _context.DadosFinanceiros
                .AsNoTracking()
                .FirstOrDefault(df =>
                    df.MatriculaSistel == matriculaSistel &&
                    df.MatriculaAstel == matriculaAstel &&
                    df.Ano == ano &&
                    df.Mes == mes);
        }

        // 🔹 Adiciona um novo registro financeiro
        public void Add(DadosFinanceiros dados)
        {
            // ✅ Verifica se existe o usuário cadastrado em DadosCadastrais
            bool usuarioExiste = _context.DadosCadastrais.Any(u =>
                u.MatriculaSistel == dados.MatriculaSistel &&
                u.MatriculaAstel == dados.MatriculaAstel);

            if (!usuarioExiste)
                throw new InvalidOperationException(
                    $"Nenhum usuário encontrado com as matrículas informadas (Sistel: {dados.MatriculaSistel}, Astel: {dados.MatriculaAstel}).");

            _context.DadosFinanceiros.Add(dados);
            _context.SaveChanges();
        }

        // 🔹 Atualiza um registro existente
        public void Update(DadosFinanceiros dados)
        {
            var existing = _context.DadosFinanceiros.FirstOrDefault(df =>
                df.MatriculaSistel == dados.MatriculaSistel &&
                df.MatriculaAstel == dados.MatriculaAstel &&
                df.Ano == dados.Ano &&
                df.Mes == dados.Mes);

            if (existing == null)
                throw new InvalidOperationException("Registro financeiro não encontrado para atualização.");

            _context.Entry(existing).CurrentValues.SetValues(dados);
            _context.SaveChanges();
        }

        // 🔹 Exclui um registro de um mês específico
        public void Delete(long matriculaSistel, long matriculaAstel, int ano, double mes)
        {
            var dados = _context.DadosFinanceiros.FirstOrDefault(df =>
                df.MatriculaSistel == matriculaSistel &&
                df.MatriculaAstel == matriculaAstel &&
                df.Ano == ano &&
                df.Mes == mes);

            if (dados != null)
            {
                _context.DadosFinanceiros.Remove(dados);
                _context.SaveChanges();
            }
        }
    }
}
