using Microsoft.EntityFrameworkCore;

namespace TiendaLinea.UI
{
    public static class GridActions
    {
        public static async Task RunDatabaseOperationAsync(Func<Task> operation, Action<string> showError)
        {
            try
            {
                await operation();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                showError($"Error de base de datos: {inner}");
            }
            catch (InvalidOperationException ex)
            {
                showError($"Error de operación: {ex.Message}");
            }
            catch (Exception ex)
            {
                showError($"Error inesperado: {ex.Message}");
            }
        }
    }
}
