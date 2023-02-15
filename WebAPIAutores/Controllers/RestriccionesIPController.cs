using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestriccionesIPController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;

        public RestriccionesIPController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CrearRestriccionIpDto restriccionIpDto)
        {
            var llaveDb = await _context.LlavesApi.FirstOrDefaultAsync(x => x.Id == restriccionIpDto.LlaveId);
            if (llaveDb is null) return NotFound();

            var usuarioId = ObtenerUsuarioId();
            if (llaveDb.UsuarioId != usuarioId) return StatusCode(StatusCodes.Status403Forbidden);

            var restriccionIp = new RestriccionIP()
            {
                LlaveId = llaveDb.Id,
                IP = restriccionIpDto.IP
            };

            _context.Add(restriccionIp);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ActualizarRestriccionIpDto actualizarrestriccion)
        {
            var restriccionDb = await _context.RestriccionesIP
                .Include(x => x.Llave)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDb is null) return NotFound();

            var usuarioId = ObtenerUsuarioId();
            if (restriccionDb.Llave.UsuarioId != usuarioId) return StatusCode(StatusCodes.Status403Forbidden);

            restriccionDb.IP = actualizarrestriccion.IP;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var restriccionDb = await _context.RestriccionesIP.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDb is null) return NotFound();

            var usuarioId = ObtenerUsuarioId();

            if (usuarioId != restriccionDb.Llave.UsuarioId) return StatusCode(StatusCodes.Status403Forbidden);

            _context.Remove(restriccionDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
