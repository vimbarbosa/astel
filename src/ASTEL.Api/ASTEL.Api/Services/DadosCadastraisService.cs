using ASTEL.Api.Data;
using ASTEL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ASTEL.Api.Services
{
    public class DadosCadastraisService
    {
        private readonly AppDbContext _context;

        public DadosCadastraisService(AppDbContext context)
        {
            _context = context;
        }

        // PAGINAÇÃO
        public async Task<List<DadosCadastrais>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.DadosCadastrais
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<(List<DadosCadastrais> registros, int totalCount)>
    GetPagedFilteredAsync(
        string? nome,
        string? cpf,
        long? matriculaAstel,
        string? formapagamento,
        string? cidade,
        string? estado,
        bool? ativo,
        int pageNumber,
        int pageSize)
        {
            var query = _context.DadosCadastrais.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(x => x.Nome.Contains(nome));

            if (!string.IsNullOrWhiteSpace(cpf))
                query = query.Where(x => x.CPF.Contains(cpf));

            if (matriculaAstel.HasValue)
                query = query.Where(x => x.MatriculaAstel == matriculaAstel.Value);

            if (!string.IsNullOrWhiteSpace(formapagamento))
                query = query.Where(x => x.FormaPagamento != null && x.FormaPagamento.Contains(formapagamento));

            if (!string.IsNullOrWhiteSpace(cidade))
                query = query.Where(x => x.Cidade != null && x.Cidade.Contains(cidade));

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(x => x.Estado != null && x.Estado.Contains(estado));

            if (ativo.HasValue)
                query = query.Where(x => x.Ativo == ativo.Value);

            var totalCount = await query.CountAsync();

            var registros = await query
                .OrderBy(x => x.Nome)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (registros, totalCount);
        }


        public async Task<int> CountAsync()
        {
            return await _context.DadosCadastrais.CountAsync();
        }

        // GET ALL (SEM PAGINAÇÃO – usado internamente)
        public async Task<List<DadosCadastrais>> GetAllAsync()
        {
            return await _context.DadosCadastrais
                .AsNoTracking()
                .ToListAsync();
        }

        // GET BY ID
        public async Task<DadosCadastrais?> GetByIdAsync(long id)
        {
            return await _context.DadosCadastrais
                .AsNoTracking()
                .Include(x => x.DadosFinanceiros)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // CREATE
        public async Task AddAsync(DadosCadastrais dados)
        {
            _context.DadosCadastrais.Add(dados);
            await _context.SaveChangesAsync();
        }

        // UPDATE
        public async Task UpdateAsync(DadosCadastrais dados)
        {
            _context.DadosCadastrais.Update(dados);
            await _context.SaveChangesAsync();
        }

        // DELETE
        public async Task<bool> DeleteAsync(long id)
        {
            var dados = await _context.DadosCadastrais.FirstOrDefaultAsync(x => x.Id == id);
            if (dados == null)
                return false;

            _context.DadosCadastrais.Remove(dados);
            await _context.SaveChangesAsync();
            return true;
        }

        // AUTocomplete - Busca nomes por parte do nome
        public async Task<List<(long Id, string Nome, long? MatriculaAstel)>> SearchNomesAsync(
            string? termo,
            bool? ativo,
            int limit = 10)
        {
            var query = _context.DadosCadastrais
                .AsNoTracking()
                .AsQueryable();
          

            if(ativo != null)
            {
                query = query.Where(x => x.Ativo == ativo);
            }

            if (!string.IsNullOrWhiteSpace(termo))
            {
                query = query.Where(x => x.Nome.Contains(termo));
            }

            return await query
                .OrderBy(x => x.Nome)
                .Take(limit)
                .Select(x => new ValueTuple<long, string, long?>(x.Id, x.Nome, x.MatriculaAstel))
                .ToListAsync();
        }
    }
}
