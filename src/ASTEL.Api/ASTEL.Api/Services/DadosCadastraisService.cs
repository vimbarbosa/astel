using ASTEL.Api.Data;
using ASTEL.Api.Models;

namespace ASTEL.Api.Services
{
    public class DadosCadastraisService
    {
        private readonly AppDbContext _context;

        public DadosCadastraisService(AppDbContext context)
        {
            _context = context;
        }

        public List<DadosCadastrais> GetAll()
        {
            return _context.DadosCadastrais.ToList();
        }

        public DadosCadastrais GetById(long id)
        {
            return _context.DadosCadastrais.Find(id);
        }

        public void Add(DadosCadastrais dados)
        {
            _context.DadosCadastrais.Add(dados);
            _context.SaveChanges();
        }

        public void Update(DadosCadastrais dados)
        {
            _context.DadosCadastrais.Update(dados);
            _context.SaveChanges();
        }

        public void Delete(long id)
        {
            var dados = _context.DadosCadastrais.Find(id);
            if (dados != null)
            {
                _context.DadosCadastrais.Remove(dados);
                _context.SaveChanges();
            }
        }
    }
}
