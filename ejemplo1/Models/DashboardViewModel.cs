using System.Collections.Generic;

namespace ejemplo1.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal IngresosTotales { get; set; }
        public int VentasRealizadas { get; set; }
        public int StockTotal { get; set; }
        public int ClientesActivos { get; set; }

        // Lista para la tabla inferior de "Ventas Recientes"
        // Usamos el modelo Pedido que ya tiene la relación con el Usuario
        public List<Pedido> VentasRecientes { get; set; } = new List<Pedido>();
    }
}