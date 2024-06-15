using Business.Interfaces;
using Business.Models;
using Business.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        public ReceiptsController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        // GET: api/receipts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReceiptModel>>> Get()
        {
            return Ok(await _receiptService.GetAllAsync());
        }


        // GET: api/receipts/period
        [HttpGet("period")]
        public async Task<ActionResult<IEnumerable<ReceiptModel>>> Get([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var allReceipts = await _receiptService.GetAllAsync();
            var filteredByDateReceipts = allReceipts
                .Where(r => r.OperationDate >= startDate.Date &&
                            r.OperationDate <= endDate.Date);
            return Ok(filteredByDateReceipts);
        }


        //GET: api/receipts/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ReceiptModel>> GetById(int id)
        {
            var result = await _receiptService.GetByIdAsync(id);

            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }

        //GET: api/products/1/details
        [HttpGet("{id}/details")]
        public async Task<ActionResult<ReceiptDetailModel>> GetByDetails(int id)
        {
            var result = await _receiptService.GetReceiptDetailsAsync(id);

            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }

        //GET: api/products/1/sum
        [HttpGet("{id}/sum")]
        public async Task<ActionResult<ReceiptModel>> GetSumById(int id)
        {
            var result = await _receiptService.ToPayAsync(id);
            return Ok(result);
        }

        // POST: api/receipts
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ReceiptModel value)
        {
            try
            {
                await _receiptService.AddAsync(value);
            }
            catch (MarketException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(value);
        }

        // PUT: api/receipts/1
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int Id, [FromBody] ReceiptModel value)
        {
            if (await _receiptService.GetByIdAsync(Id) == null)
                return NotFound();

            try
            {
                await _receiptService.UpdateAsync(value);

            }
            catch (MarketException)
            {
                return BadRequest();
            }
            return Ok();
        }

        // PUT: api/receipts/1/products/remove/1/1
        [HttpPut("{id}/products/remove/{productId}/{quantity}")]
        public async Task<ActionResult> UpdateRemoveProductFromReceipt(int id, int productId, int quantity)
        {
            if (!_receiptService.GetAllAsync().Result.Select(r => r.Id).Contains(id))
                return BadRequest();

            await _receiptService.RemoveProductAsync(productId, id, quantity);
            return Ok();
        }

        // PUT: api/receipts/1/products/add/1/1
        [HttpPut("{id}/products/add/{productId}/{quantity}")]
        public async Task<ActionResult> UpdateAddProductFromReceipt(int id, int productId, int quantity)
        {
            if (!_receiptService.GetAllAsync().Result.Select(r => r.Id).Contains(id))
                return BadRequest();

            await _receiptService.AddProductAsync(productId, id, quantity);
            return Ok();
        }

        // PUT: api/receipts/1/checkout
        [HttpPut("{id}/checkout")]
        public async Task<ActionResult> CheckOut(int id)
        {
            var targetReceipt = await _receiptService.GetByIdAsync(id);

            if (targetReceipt == null)
            {
                return NotFound();
            }
            await _receiptService.CheckOutAsync(id);

            return Ok();
        }

        // DELETE: api/receipts/1
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var toBeDeleted = await _receiptService.GetByIdAsync(id);
            if (toBeDeleted == null)
                return NotFound();

            await _receiptService.DeleteAsync(id);
            return Ok();
        }
    }
}
