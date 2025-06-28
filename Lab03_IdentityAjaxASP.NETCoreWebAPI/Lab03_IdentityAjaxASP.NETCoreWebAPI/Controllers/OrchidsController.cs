using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interface;

namespace Lab03_IdentityAjaxASP.NETCoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrchidsController : ControllerBase
    {
        private readonly IOrchidRepository _orchidRepository;

        public OrchidsController(IOrchidRepository orchidRepository)
        {
            _orchidRepository = orchidRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageIndex = 1, int pageNumber = 10, string? idSearch = null, string? nameSearch = null, string? categorySearch = null, bool? isNatural = null)
        {
            var orchids = await _orchidRepository.GetAllAsync(pageIndex, pageNumber, idSearch, nameSearch, categorySearch, isNatural);
            return Ok(orchids);
        }
    }
}
