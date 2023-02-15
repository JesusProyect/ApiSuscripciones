using Microsoft.AspNetCore.Identity;

namespace WebAPIAutores.Entidades
{
    public class Usuario : IdentityUser
    {
        public bool MalaPaga { get; set; } // si es mala paga no se pueden usar las llaves profesionales
    }
}
