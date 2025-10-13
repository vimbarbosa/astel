using ASTEL.Api.Data;
using ASTEL.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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
            return _context.DadosCadastrais.AsNoTracking().ToList();
        }

        public DadosCadastrais? GetByMatriculaSistel(int matriculaSistel)
        {
            return _context.DadosCadastrais
                           .AsNoTracking()
                           .FirstOrDefault(d => d.MatriculaSistel == matriculaSistel);
        }

        public void Add(DadosCadastrais dados)
        {
            // Evita duplicação de chave
            var existente = _context.DadosCadastrais
                                    .AsNoTracking()
                                    .FirstOrDefault(d => d.MatriculaSistel == dados.MatriculaSistel);

            if (existente != null)
                throw new InvalidOperationException($"Já existe um registro com a matrícula Sistel {dados.MatriculaSistel}.");

            _context.DadosCadastrais.Add(dados);
            _context.SaveChanges();
        }

        public void Update(DadosCadastrais dados)
        {
            var existente = _context.DadosCadastrais
                                    .FirstOrDefault(d => d.MatriculaSistel == dados.MatriculaSistel);

            if (existente == null)
                throw new KeyNotFoundException($"Registro com matrícula Sistel {dados.MatriculaSistel} não encontrado.");

            _context.Entry(existente).CurrentValues.SetValues(dados);
            _context.SaveChanges();
        }

        public void Delete(int matriculaSistel)
        {
            var dados = _context.DadosCadastrais
                                .FirstOrDefault(d => d.MatriculaSistel == matriculaSistel);

            if (dados != null)
            {
                _context.DadosCadastrais.Remove(dados);
                _context.SaveChanges();
            }
        }
    }
}
