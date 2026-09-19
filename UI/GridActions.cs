using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace TiendaLinea.UI
{
    internal static class GridActions
    {
        public static string GetOutputPath(string prefix, int codigo)
        {
            var outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            Directory.CreateDirectory(outputDir);
            return Path.Combine(outputDir, $"{prefix}{codigo}.json");
        }

        public static void ExportSelected<T>(DataGridView grid, string defaultFileName, Action<T, string> saveToJson) where T : class
        {
            if (grid.CurrentRow?.DataBoundItem is not T item)
            {
                MessageBox.Show("Seleccione una fila primero.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Archivos JSON (*.json)|*.json",
                DefaultExt = "json",
                FileName = defaultFileName,
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output")
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    saveToJson(item, sfd.FileName);
                    MessageBox.Show("Archivo exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static async Task<bool> RunDatabaseOperationAsync(Control owner, string caption, Func<Task> operation)
        {
            var previousCursor = owner.Cursor;
            owner.Cursor = Cursors.WaitCursor;
            try
            {
                await operation();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"Error al guardar en la base de datos:\n{dbEx.InnerException?.Message ?? dbEx.Message}",
                    caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado:\n{ex.Message}",
                    caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                owner.Cursor = previousCursor;
            }
        }
    }
}


