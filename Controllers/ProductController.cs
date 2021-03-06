using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop
.Controllers
{
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get([FromServices] DataContext context)
        {
            var products = await context
            .Products
            .Include(x => x.Category)
            .AsNoTracking()
            .ToListAsync();

            return Ok(products);
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(int id, [FromServices] DataContext context)
        {
            var product = await context
            .Products
            .Include(x => x.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

            return Ok(product);
        }

        [HttpGet]
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategory(int id, [FromServices] DataContext context)
        {
            var products = await context
            .Products
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => x.CategoryId == id)
            .ToListAsync();

            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Post([FromBody] Product model, [FromServices] DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Products.Add(model);
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch
            {

                return BadRequest(new { message = "N??o foi poss??vel criar o produto" });
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<Product>> Put(int id, [FromBody] Product model, [FromServices] DataContext context)
        {
            // verifica se o ID informado ??  o mesmo do modelo
            if (model.Id != id)
                return NotFound(new { message = "Produto n??o encontrada" });

            // Verifica se os dados s??o validos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<Product>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);

            }
            catch (DbUpdateConcurrencyException)
            {

                return BadRequest(new { message = "Este registro j?? foi atualizado" });
            }
            catch (Exception)
            {

                return BadRequest(new { message = "N??o foi poss??vel atualizar a Produto" });
            }

        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> Delete(int id, [FromServices] DataContext context)
        {
            var Product = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (Product == null)
                return NotFound(new { message = "Produto n??o encontrada" });
            try
            {
                context.Products.Remove(Product);
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new { message = "N??o foi poss??vel remover a Produto" });
            }
        }
    }
}