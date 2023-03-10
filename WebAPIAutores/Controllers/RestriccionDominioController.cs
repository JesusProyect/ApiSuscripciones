using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;
using WebAPIAutores.Migrations;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestriccionDominioController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;

        public RestriccionDominioController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CrearRestriccionesDominioDto crearRestriccionesDominioDto)
        {
            var llaveDb = await _context.LlavesApi.FirstOrDefaultAsync(x => x.Id == crearRestriccionesDominioDto.LlaveId);
            if (llaveDb == null) return NotFound();

            var usuarioId = ObtenerUsuarioId();
            if (llaveDb.UsuarioId != usuarioId) return StatusCode(StatusCodes.Status403Forbidden); // ES EL FORBID CREO PORQUE REDIRECCIONA SEGUN HE ENTENDIDO;

            var restriccionDominio = new RestriccionDominio()
            {
                LlaveId = crearRestriccionesDominioDto.LlaveId,
                Dominio = crearRestriccionesDominioDto.Dominio
            };

            _context.Add(restriccionDominio);
            await _context.SaveChangesAsync();

            return NoContent();

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ActualizarRestriccionDominioDto actualizarRestriccionDominioDto)
        {
            var restriccionDb = await _context.RestriccionesDominio.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);
            if (restriccionDb is null) return NotFound();

            var usuarioId = ObtenerUsuarioId();
            if (restriccionDb.Llave.UsuarioId != usuarioId) return StatusCode(StatusCodes.Status403Forbidden);

            restriccionDb.Dominio = actualizarRestriccionDominioDto.Dominio; 
            await _context.SaveChangesAsync();
            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var restriccionDb = await _context.RestriccionesDominio.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDb is null) return NotFound();

            var usuarioId = ObtenerUsuarioId();
            if (usuarioId != restriccionDb.Llave.UsuarioId) return StatusCode(StatusCodes.Status403Forbidden);

            _context.Remove(restriccionDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }



    }
}
