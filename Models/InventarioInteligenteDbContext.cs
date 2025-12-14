using System.ComponentModel.DataAnnotations;

namespace Inventario_Inteligente.Models
{
    public class InventarioInteligenteDbContext
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
