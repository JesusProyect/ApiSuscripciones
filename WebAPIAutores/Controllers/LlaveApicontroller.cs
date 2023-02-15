using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Servicios;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/llaveApi")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LlaveApicontroller : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ServicioLlave _servicioLlave;

        public LlaveApicontroller( ApplicationDbContext context, IMapper mapper, ServicioLlave servicioLlave)
        {
            _context = context;
            _mapper = mapper;
            _servicioLlave = servicioLlave;
        }


        [HttpGet]
        public async Task<List<LlaveDto>> MisLlaves()
        {
            var usuarioId = ObtenerUsuarioId();
            var llaves = await _context.LlavesApi
                .Include(x =>x.RestriccionesDominio)
                .Include(x => x.RestriccionesIP)
                .Where(x => x.UsuarioId == usuarioId)
                .ToListAsync();
            return _mapper.Map<List<LlaveDto>>(llaves);

        }

        [HttpPost]
        public async Task<ActionResult> CrearLlave(CrearLlaveDto crearLlaveDto)
        {
            var usuarioId = ObtenerUsuarioId();

            if (crearLlaveDto.TipoLlave == Entidades.TipoLlave.Gratuita)
            {
                //verificamos si ya tiene una gratuita si la tiene le decimos nanai
                var elUsuarioYaTieneUnaLlaveGratuita = await _context.LlavesApi.AnyAsync(x => x.UsuarioId == usuarioId && x.TipoLlave == Entidades.TipoLlave.Gratuita);

                if (elUsuarioYaTieneUnaLlaveGratuita) return BadRequest("El usuario ya tiene una llave gratuita");
            }

            await _servicioLlave.CrearLlave(usuarioId, crearLlaveDto.TipoLlave);

            return NoContent();

        }

        [HttpPut]
        public async Task<ActionResult> ActualizarLlave(ActualizarLlaveDto actualizarLlaveDto)
        {
            var usuarioId = ObtenerUsuarioId();

            var LlaveDb = await _context.LlavesApi.FirstOrDefaultAsync( x => x.Id == actualizarLlaveDto.LlaveId);

            if (LlaveDb == null) return NotFound();
            if (usuarioId != LlaveDb.UsuarioId) return Forbid();
            if (actualizarLlaveDto.ActualizarLlave) LlaveDb.Llave = _servicioLlave.GenerarLlave();
            LlaveDb.Activa = actualizarLlaveDto.Activa;

            await _context.SaveChangesAsync();
            return NoContent();
            
        }

    }
}
