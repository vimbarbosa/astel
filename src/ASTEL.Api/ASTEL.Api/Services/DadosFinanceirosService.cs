using ASTEL.Api.Data;
using ASTEL.Api.Models;

namespace ASTEL.Api.Services
{
    public class DadosFinanceirosService
    {
        private readonly AppDbContext _context;

        public DadosFinanceirosService(AppDbContext context)
        {
            _context = context;
        }

        public List<DadosFinanceiros> GetAll()
        {
            return _context.DadosFinanceiros.ToList();
        }

        public DadosFinanceiros GetById(int matriculaSistel, int matriculaAstel, int ano)
        {
            return _context.DadosFinanceiros.Find(matriculaSistel, matriculaAstel, ano);
        }

        public void Add(DadosFinanceiros dados)
        {
            _context.DadosFinanceiros.Add(dados);
            _context.SaveChanges();
        }

        public void Update(DadosFinanceiros dados)
        {
            _context.DadosFinanceiros.Update(dados);
            _context.SaveChanges();
        }

        public void Delete(int matriculaSistel, int matriculaAstel, int ano)
        {
            var dados = _context.DadosFinanceiros.Find(matriculaSistel, matriculaAstel, ano);
            if (dados != null)
            {
                _context.DadosFinanceiros.Remove(dados);
                _context.SaveChanges();
            }
        }
    }
}
