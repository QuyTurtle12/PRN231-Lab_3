using DataAccess.Constant.Enum;
using DataAccess.DTO.Orchid;
using Microsoft.AspNetCore.Authorization;
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

        private const string STAFF_ROLE = "3";

        public OrchidsController(IOrchidRepository orchidRepository)
        {
            _orchidRepository = orchidRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageIndex = 1, int pageSize = 10, string? idSearch = null, string? nameSearch = null, string? categorySearch = null, bool? isNatural = null)
        {
            var orchids = await _orchidRepository.GetAllAsync(pageIndex, pageSize, idSearch, nameSearch, categorySearch, isNatural);
            return Ok(orchids);
        }

        [Authorize(Roles = "3")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrchidDTO orchidDto)
        {
            if (orchidDto == null)
            {
                return BadRequest("Orchid data is required.");
            }
            await _orchidRepository.InsertAsync(orchidDto);
            return CreatedAtAction(nameof(GetAll), new { id = 0 }, orchidDto);
        }

        [Authorize(Roles = "3")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateOrchidDTO orchidDto)
        {
            if (orchidDto == null || id <= 0)
            {
                return BadRequest("Invalid orchid data or ID.");
            }
            await _orchidRepository.UpdateAsync(id, orchidDto);
            return Ok();
        }

        [Authorize(Roles = "3")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid orchid ID.");
            }
            try
            {
                await _orchidRepository.DeleteAsync(id);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
