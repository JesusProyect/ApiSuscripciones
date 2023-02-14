using System;
using System.Threading.Tasks;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Servicios
{
    public class ServicioLlave
    {
        private readonly ApplicationDbContext _context;

        public ServicioLlave(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CrearLlave(string usuarioId, TipoLlave tipoLlave)
        {
            var llave = GenerarLlave();

            var llaveAPI = new LlaveAPI
            {
                Activa = true,
                TipoLlave = tipoLlave,
                Llave = llave,
                UsuarioId = usuarioId
            };

            _context.Add(llaveAPI);
            await _context.SaveChangesAsync();
        }

        public string GenerarLlave()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
