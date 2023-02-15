using System.ComponentModel.DataAnnotations;

namespace WebAPIAutores.DTOs
{
    public class ActualizarRestriccionIpDto
    {
        [Required]
        public string IP { get; set; }
    }
}
