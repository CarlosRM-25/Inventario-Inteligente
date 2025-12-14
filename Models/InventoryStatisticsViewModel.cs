namespace Inventario_Inteligente.Models
{
    public class InventoryStatisticsViewModel
    {
        public List<Controllers.Product> ProductsOrderedByPriceDesc { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<Controllers.Product> CriticalStockProducts { get; set; }
    }
}
