using DataAccess.DTO.Category;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interface;

namespace Lab03_IdentityAjaxASP.NETCoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageIndex = 1, int pageNumber = 10, string? idSearch = null, string? nameSearch = null)
        {
            var categories = await _categoryRepository.GetAllAsync(pageIndex, pageNumber, idSearch, nameSearch);
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDTO category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.InsertAsync(category);
                return CreatedAtAction(nameof(GetAll), new { id = 1 }, category);
            }
            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDTO category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(id, category);
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            return Ok();
        }
    }
}
