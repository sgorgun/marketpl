using Business.Interfaces;
using Business.Models;
using Business.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        //GET: api/products
        //GET: api/products?categoryId=1&minPrice=20&maxPrice=50
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductModel>>> Get([FromQuery] int? categoryId, int? minPrice, int? maxPrice)
        {
            if (categoryId != null || minPrice != null || maxPrice != null)
            {
                FilterSearchModel newFilter = new ()
                {
                    CategoryId = categoryId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
                return Ok(await _productService.GetByFilterAsync(newFilter));
            }

            var customers = await _productService.GetAllAsync();
            return Ok(customers);
        }


        //GET: api/products/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductModel>> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id);

            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ProductModel value)
        {
            try
            {
                await _productService.AddAsync(value);
            }
            catch (MarketException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(value);
        }

        // PUT: api/products/1
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ProductModel value)
        {
            if (await _productService.GetByIdAsync(id) == null)
                return NotFound();

            try
            {
                await _productService.UpdateAsync(value);
            }
            catch (MarketException)
            {
                return BadRequest();
            }
            return Ok();
        }

        // DELETE: api/products/1
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok();
        }

        // GET: api/products/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<ProductCategoryModel>>> GetCategories()
        {
            return Ok(await _productService.GetAllProductCategoriesAsync());
        }

        [HttpPost("categories")]
        public async Task<ActionResult> PostCategory([FromBody] ProductCategoryModel value)
        {
            try
            {
                await _productService.AddCategoryAsync(value);
            }
            catch (MarketException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(value);
        }

        // PUT: api/products/categories1
        [HttpPut("categories/{id}")]
        public async Task<ActionResult> PutCategory(int Id, [FromBody] ProductCategoryModel value)
        {
            if (value == null)
                throw new MarketException(nameof(value));

            if (Id != value.Id)
                throw new MarketException();

            await _productService.UpdateCategoryAsync(value);
            return Ok();
        }

        // DELETE: api/products/categories/1
        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            await _productService.RemoveCategoryAsync(id);
            return Ok();
        }
    }
}
