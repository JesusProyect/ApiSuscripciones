using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Middlewares
{
    public static class LimitarPeticionesMidlewareExtension
    {
        public static IApplicationBuilder UseLimitarPeticiones(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LimitarPeticionesMidleware>();
        }
    }

    public class LimitarPeticionesMidleware
    {
        private readonly RequestDelegate _siguiente;
        private readonly IConfiguration _config;

        public LimitarPeticionesMidleware(RequestDelegate siguiente, IConfiguration config)
        {
            _siguiente = siguiente;
            _config = config;
        }


        public async Task InvokeAsync(HttpContext httpContext, ApplicationDbContext context)
        {
            //lo hace asi para poder utilizar el intelisense y accedermas rapido y no en cada lugar que lo necesite hacer la linea de _config para obtener el valor
            var limiarPeticionesConfiguraciuon = new LimitarPeticionesConfiguracion();
            _config.GetRequiredSection("limitarPeticiones").Bind(limiarPeticionesConfiguraciuon);

            var ruta = httpContext.Request.Path.ToString().ToLower();
            var estaLaRutaEnListaBlanca = limiarPeticionesConfiguraciuon.ListaBlancaRutas.Any(x => ruta.Contains(x));

            if (estaLaRutaEnListaBlanca)
            {
                await _siguiente(httpContext);
                return;
            }

            var llaveStringValues = httpContext.Request.Headers["X-Api-Key"];
            //si no tiene api key le decimos nanai
            if(llaveStringValues.Count == 0)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Debe proveer la llave en la cabecera X-Api-Key");
                return;
            }

             //que solamente haya un valor  para la llave
             if(llaveStringValues.Count > 1)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Solo una llave debe de estar presente");
                return;
            }

            var llave = llaveStringValues[0];

            //ve si existe en la db
            var llaveDb = await context.LlavesApi
                .Include(x => x.RestriccionesDominio)
                .Include(x => x.RestriccionesIP)
                .Include(x => x.Usuario)
                .AsSingleQuery()
                .FirstOrDefaultAsync(x => x.Llave == llave);

            if(llaveDb is null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llaveno existe");
                return;
            }

            //ve si está activa
            if (!llaveDb.Activa )
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave no está activa");
                return;
            }

            //si es gratuita vemos cuantas peticiones lleva
            if(llaveDb.TipoLlave == Entidades.TipoLlave.Gratuita)
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                //vemos cuantas tiene
                var cantidadPeticionesRealizadasHoy = await context.Peticiones.CountAsync(x => x.LlaveId == llaveDb.Id && x.FechaPeticion >= hoy && x.FechaPeticion < mañana);
                if(cantidadPeticionesRealizadasHoy > limiarPeticionesConfiguraciuon.PeticionesPorDiaGratuito)
                {
                    httpContext.Response.StatusCode = 429; //too many request
                    await httpContext.Response.WriteAsync("Ha superado la cantidad de peticiones por dia para una suscripcion gratuita," +
                        " para hacer mas peticiones hoy, actualice su suscripcion a una suscripcion profesional");
                    return;
                }
                
            } //si es mala paga no lo dejamos hacer nada, si es gratuita lo dejamos asi sea mala paga
            else if(llaveDb.Usuario.MalaPaga)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("El usuario es un mala paga");
                return;
            }
            

            //restricciones
            var superaRestricciones = PeticionSuperaAlgunaDeLasRestricciones(llaveDb, httpContext);
            //si no las pasa devolvemos prohibido
            if (!superaRestricciones )
            {
                httpContext.Response.StatusCode = 403;
                return;
            }

            var peticion = new Peticion() { LlaveId = llaveDb.Id, FechaPeticion = DateTime.UtcNow };
            context.Add(peticion);
            await context.SaveChangesAsync();

            await _siguiente(httpContext);

        }

        private bool PeticionSuperaAlgunaDeLasRestricciones(LlaveAPI llaveAPI, HttpContext httpContext)
        {
            var hayRestricciones = llaveAPI.RestriccionesDominio.Any() || llaveAPI.RestriccionesIP.Any();
            if (!hayRestricciones) return true; // no hay restricciones por lo cual no hay nada que hacer

            var peticionSuperaLasRestriccionesDeDominio = PeticionSuperaLasRestriccionesDeDominio(llaveAPI.RestriccionesDominio, httpContext);
            var peticionSuperaRestriccionesIp = PeticionSuperaRestriccionesDeIp(llaveAPI.RestriccionesIP, httpContext);

            return peticionSuperaLasRestriccionesDeDominio || peticionSuperaRestriccionesIp; //asi si supera alguna no hay problema y si supera otra no hay problema

        }

        private bool PeticionSuperaRestriccionesDeIp(List<RestriccionIP> restricciones, HttpContext httpContext)
        {
            //si devolvemos falso es que no a superado la prueba entonces nanai
            if (restricciones == null || restricciones.Count == 0) return false;
            var ip = httpContext.Connection.RemoteIpAddress.ToString();
            if (ip == String.Empty) return false;

            var superaRestriccion = restricciones.Any(x => x.IP == ip);
            return superaRestriccion;
        }
       

        private bool PeticionSuperaLasRestriccionesDeDominio(List<RestriccionDominio> restricciones, HttpContext  httpContext)
        {
            //si devolvemos falso es que no a superado la prueba entonces nanai
            if (restricciones == null || restricciones.Count == 0) return false;

            var referer = httpContext.Request.Headers["Referer"].ToString();

            if (referer == String.Empty) return false;

            Uri myUri = new Uri(referer);
            string host = myUri.Host;

            var superaRestriccion = restricciones.Any(x => x.Dominio == host);

            return superaRestriccion;

        }

    }
}
