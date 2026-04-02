using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizAppProject.Interfaces;
using QuizAppProject.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizAppProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _svc;

        public CategoriesController(ICategoryService svc)
        {
            _svc = svc;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> List()
        {
            try
            {
                return Ok(await _svc.GetAll());
            }
            catch
            {
                throw;
            }
        }
            

        // GET: api/categories/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CategoryDto>> Get(Guid id)
        {
            try
            {
                var c = await _svc.Get(id);
                if (c == null) return NotFound();
                return Ok(c);
            }
            catch
            {
                throw;
            }
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateDto dto)
        {
            try
            {
                var res = await _svc.Add(dto);
                return CreatedAtAction(nameof(Get), new { id = res.CategoryId }, res);
            }
            catch
            {
                throw;
            }
        }

        // PUT: api/categories/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] CategoryCreateDto dto)
        {
            try
            {
                var res = await _svc.Update(id, dto);
                return Ok(res);
            }
            catch
            {
                throw;
            }
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var ok = await _svc.Delete(id);
                return ok ? NoContent() : NotFound();
            }
            catch
            {
                throw;
            }
        }
    }
}


