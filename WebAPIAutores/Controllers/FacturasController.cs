using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;

namespace WebAPIAutores.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FacturasController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;

        public FacturasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Pagar(PagarFacturaDto pagarFacturaDto)
        {
            //de igual forma hay que comprobar que  la factura que quiere pagar le corresponde al usuario del token que entra si no va a paagr una factura que no es de el

            var facturaDb = await _context.Facturas
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == pagarFacturaDto.FacturaId);

            if (facturaDb is null) return NotFound();
            if (facturaDb.Pagada) return BadRequest("La Factura ya está pagaga");

            //logica para pagar la factura
            //suponemos que fue exitoso

            facturaDb.Pagada = true;
            await _context.SaveChangesAsync();

            //el hecho de que pague una no significa que ya sea buena paga, primero revisamos si no le quedan mas facturas que pagar

            var hayFacturasPendientesVencidas = await _context.Facturas
                .AnyAsync(x => x.UsuarioId == facturaDb.UsuarioId &&
                    !x.Pagada &&
                    x.FechaLimitePago < DateTime.Today);

            //si paga lafactura y no era mala paga no pasa nada seguira sin ser mala paga pero con su factura pagada
            if (!hayFacturasPendientesVencidas)
            {
                facturaDb.Usuario.MalaPaga = false;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
