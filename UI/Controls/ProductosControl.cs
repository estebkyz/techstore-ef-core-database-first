using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TiendaLinea.Models;

namespace TiendaLinea.UI.Controls
{
    public class ProductosControl : UserControl
    {
        private readonly AppState _appState;
        private readonly DataGridView  _grid          = new();
        private readonly BindingSource _bindingSource  = new();
        private readonly ErrorProvider _errorProvider  = new();

        private readonly NumericUpDown _numCodigo     = new() { Minimum = 1, Maximum = 999999, Value = 101, ReadOnly = true };
        private readonly TextBox       _txtNombre      = new();
        private readonly TextBox       _txtCategoria   = new();
        private readonly TextBox       _txtDescripcion = new();
        private readonly NumericUpDown _numPrecioVenta = new() { Minimum = 0, Maximum = 99999999, DecimalPlaces = 2, Value = 100 };
        private readonly NumericUpDown _numStockActual = new() { Minimum = 0, Maximum = 9999, Value = 10 };
        private readonly NumericUpDown _numStockMinimo = new() { Minimum = 0, Maximum = 9999, Value = 2 };
        private readonly NumericUpDown _numImpuesto    = new() { Minimum = 0, Maximum = 1, DecimalPlaces = 2, Value = 0.19M, Increment = 0.01M };
        private readonly CheckBox      _chkActivo      = new() { Checked = true, Text = "Sí" };

        public ProductosControl(AppState appState)
        {
            _appState = appState;
            InitializeLayout();
            _numCodigo.Value = _appState.GetNextCodigoProducto();
        }

        private void InitializeLayout()
        {
            var formPanel = FormLayoutHelper.CreateFormPanel();
            FormLayoutHelper.AddRow(formPanel, "Código:", _numCodigo);
            FormLayoutHelper.AddRow(formPanel, "Nombre:",        _txtNombre);
            FormLayoutHelper.AddRow(formPanel, "Categoría:",     _txtCategoria);
            FormLayoutHelper.AddRow(formPanel, "Descripción:",   _txtDescripcion);
            FormLayoutHelper.AddRow(formPanel, "Precio venta $:", _numPrecioVenta);
            FormLayoutHelper.AddRow(formPanel, "Stock actual:",  _numStockActual);
            FormLayoutHelper.AddRow(formPanel, "Stock mínimo:",  _numStockMinimo);
            FormLayoutHelper.AddRow(formPanel, "IVA (0-1):",     _numImpuesto);
            FormLayoutHelper.AddRow(formPanel, "Activo:",        _chkActivo);

            _numCodigo.BackColor = Color.FromArgb(232, 240, 254);

            var buttonBar = FormLayoutHelper.CreateButtonBar();

            var btnAdd    = new Button { Text = "▶  Agregar",          AutoSize = true };
            var btnToggle = new Button { Text = "⏺  Activar/Inactivar", AutoSize = true };
            var btnExport = new Button { Text = "📤  Exportar JSON",   AutoSize = true };

            FormLayoutHelper.StyleButton(btnAdd,    ButtonStyle.Primary);
            FormLayoutHelper.StyleButton(btnToggle, ButtonStyle.Neutral);
            FormLayoutHelper.StyleButton(btnExport, ButtonStyle.Success);

            btnAdd.Click    += OnAddClick;
            btnToggle.Click += OnToggleStateClick;
            btnExport.Click += OnExportClick;

            buttonBar.Controls.Add(btnAdd);
            buttonBar.Controls.Add(btnToggle);
            buttonBar.Controls.Add(btnExport);

            _grid.Dock               = DockStyle.Fill;
            _grid.AutoGenerateColumns = true;
            _grid.ReadOnly            = true;
            FormLayoutHelper.StyleGrid(_grid);

            _grid.CellFormatting += Grid_CellFormatting;

            _bindingSource.DataSource = _appState.Productos;
            _grid.DataSource          = _bindingSource;

            Controls.Add(_grid);
            Controls.Add(buttonBar);
            Controls.Add(formPanel);
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is not Producto p) return;

            if (p.StockBajo)
            {
                e.CellStyle.BackColor = FormLayoutHelper.StockAlertColor;
                e.CellStyle.ForeColor = Color.FromArgb(230, 81, 0);
            }
        }

        private async void OnToggleStateClick(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not Producto p)
            {
                MessageBox.Show("Seleccione un producto de la tabla.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verificar si tiene ventas asociadas antes de inactivar
            bool tieneVentas = await _appState.TieneVentasAsociadasAsync(p);
            string accion  = p.Activo ? "inactivar" : "activar";
            string aviso   = tieneVentas && p.Activo
                ? "\nEste producto tiene ventas registradas. Se aplicará baja lógica (inactivación)."
                : string.Empty;

            var confirm = MessageBox.Show(
                $"¿Desea {accion} el producto '{p.Nombre}'?{aviso}",
                "Confirmar cambio de estado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                p.Activo = !p.Activo;
                var success = await GridActions.RunDatabaseOperationAsync(this, "Actualizar Producto", async () =>
                {
                    await _appState.UpdateProductoAsync(p);
                });
                
                if (success)
                {
                    MessageBox.Show($"'{p.Nombre}' ahora está {(p.Activo ? "ACTIVO" : "INACTIVO")}.",
                        "Estado actualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private async void OnAddClick(object? sender, EventArgs e)
        {
            if (!ValidateFields()) return;
            var codigo = _appState.GetNextCodigoProducto();
            var p = new Producto {
                Codigo = codigo,
                Nombre = _txtNombre.Text.Trim(),
                Categoria = _txtCategoria.Text.Trim(),
                Descripcion = _txtDescripcion.Text.Trim(),
                PrecioVenta = _numPrecioVenta.Value,
                StockActual = (int)_numStockActual.Value,
                StockMinimo = (int)_numStockMinimo.Value,
                Impuesto = _numImpuesto.Value,
                Activo = _chkActivo.Checked
            };

            await GridActions.RunDatabaseOperationAsync(this, "Crear Producto", async () =>
            {
                await _appState.AddProductoAsync(p);
                p.SaveToJson(GridActions.GetOutputPath("producto", p.Codigo));
            });
            ClearFields();
        }

        private bool ValidateFields()
        {
            bool ok = true;
            if (!Validators.IsRequired(_txtNombre.Text))
            {
                _errorProvider.SetError(_txtNombre, "El nombre es obligatorio.");
                ok = false;
            }
            else _errorProvider.SetError(_txtNombre, string.Empty);

            if (!Validators.IsRequired(_txtCategoria.Text))
            {
                _errorProvider.SetError(_txtCategoria, "La categoría es obligatoria.");
                ok = false;
            }
            else _errorProvider.SetError(_txtCategoria, string.Empty);

            return ok;
        }

        private void ClearFields()
        {
            _numCodigo.Value = _appState.GetNextCodigoProducto();
            _txtNombre.Clear();
            _txtCategoria.Clear();
            _txtDescripcion.Clear();
            _numPrecioVenta.Value = 100;
            _chkActivo.Checked   = true;
        }

        private void OnExportClick(object? sender, EventArgs e) =>
            GridActions.ExportSelected<Producto>(_grid, "producto.json", (p, path) => p.SaveToJson(path));
    }
}

