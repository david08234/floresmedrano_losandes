using ejemplo1.Data;
using ejemplo1.Models;
using ejemplo1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;
using System.Text.Json;

namespace ejemplo1.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var modelo = new DashboardViewModel();

            modelo.IngresosTotales = await _context.Pedidos.SumAsync(p => (decimal?)p.Total) ?? 0;
            modelo.VentasRealizadas = await _context.Pedidos.CountAsync();
            modelo.StockTotal = await _context.Productos.SumAsync(p => (int?)p.Cantidad) ?? 0;
            modelo.ClientesActivos = await _context.Usuarios.CountAsync();

            modelo.VentasRecientes = await _context.Pedidos
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaPedido)
                .Take(5)
                .ToListAsync();

            // --- INICIO LÓGICA PARA EL GRÁFICO (Chart.js) ---
            // Agrupamos las ventas por día, las ordenamos y tomamos los últimos 7 días con actividad
            var ultimos7Dias = await _context.Pedidos
                .GroupBy(p => p.FechaPedido.Date)
                .OrderBy(g => g.Key)
                .Take(7)
                .Select(g => new {
                    Fecha = g.Key.ToString("dd/MM"),
                    Total = g.Sum(p => p.Total)
                })
                .ToListAsync();

            // Convertimos las fechas y totales a texto formato JSON para que JavaScript los entienda
            ViewBag.Fechas = JsonSerializer.Serialize(ultimos7Dias.Select(x => x.Fecha));
            ViewBag.Montos = JsonSerializer.Serialize(ultimos7Dias.Select(x => x.Total));
            // --- FIN LÓGICA PARA EL GRÁFICO ---

            return View(modelo);
        }

        public async Task<IActionResult> ExportarExcel()
        {
            // 1. Traemos las ventas de la base de datos (incluyendo al usuario)
            var ventas = await _context.Pedidos
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            // 2. Creamos el archivo Excel en memoria
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ventas Registradas");
                var currentRow = 1;

                // 3. Dibujamos las cabeceras de las columnas (Fila 1)
                worksheet.Cell(currentRow, 1).Value = "ID Pedido";
                worksheet.Cell(currentRow, 2).Value = "Cliente";
                worksheet.Cell(currentRow, 3).Value = "Fecha";
                worksheet.Cell(currentRow, 4).Value = "Monto Total (Bs)";
                worksheet.Cell(currentRow, 5).Value = "Estado";

                // Ponemos las cabeceras en Negrita y con fondo gris
                worksheet.Range("A1:E1").Style.Font.Bold = true;
                worksheet.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.LightGray;

                // 4. Llenamos los datos fila por fila
                foreach (var venta in ventas)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = venta.Id;
                    worksheet.Cell(currentRow, 2).Value = venta.Usuario.Nombre;
                    worksheet.Cell(currentRow, 3).Value = venta.FechaPedido.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(currentRow, 4).Value = venta.Total;
                    worksheet.Cell(currentRow, 5).Value = venta.Estado.ToString();
                }

                // Auto-ajustar el ancho de las columnas para que se lea bien
                worksheet.Columns().AdjustToContents();

                // 5. Convertimos el Excel a un archivo descargable
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    // Retornamos el archivo al navegador del usuario
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Reporte_Ventas_LosAndes.xlsx"
                    );
                }
            }
        }
    }
}