using ASTEL.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASTEL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ImportFinanceiroService _financeiroService;

        public ImportController(ImportFinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        [HttpPost("importFinanceiro")]
        public async Task<IActionResult> ImportFinanceiro([FromForm] IFormFile file)
        {
            var result = await _financeiroService.ImportCsvAsync(file);
            return Ok(result);
        }
    }
}
