using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TiendaLinea.Models;
using TiendaLinea.Models;
using TiendaLinea.Models;

namespace TiendaLinea.UI.Controls
{
    public class VentasControl : UserControl, IReferenceDataConsumer
    {
        private readonly AppState _appState;
        private readonly DataGridView  _gridVentas    = new();
        private readonly BindingSource _bindingSource  = new();

        private readonly DataGridView      _gridDetalles  = new();
        private readonly List<Detallesventum> _detallesActuales = new();

        private readonly ComboBox       _cmbCliente  = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox       _cmbEmpleado = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _dtpFecha    = new();

        private readonly ComboBox       _cmbProducto = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown  _numCantidad = new() { Minimum = 1, Maximum = 9999, Value = 1 };

        private readonly Label _lblSubtotal = new() { AutoSize = true, Font = FormLayoutHelper.BoldFont };
        private readonly Label _lblIVA      = new() { AutoSize = true, Font = FormLayoutHelper.BoldFont };
        private readonly Label _lblTotal    = new() { AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold) };

        private readonly Label _lblCodigo = new() { AutoSize = true, Font = FormLayoutHelper.BoldFont };

        public VentasControl(AppState appState)
        {
            _appState = appState;
            InitializeLayout();
            Enter += (_, _) => RefreshReferenceData();
        }

        private void InitializeLayout()
        {
            var grpAgregacion = new GroupBox
            {
                Text    = "Seleccionar Cliente y Empleado ya existentes",
                Dock    = DockStyle.Top,
                Height  = 90,
                Padding = new Padding(8, 4, 8, 4)
            };
            FormLayoutHelper.StyleGroupBox(grpAgregacion);

            var tblAgr = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 6,
                RowCount    = 1,
                Font        = FormLayoutHelper.AppFont
            };
            tblAgr.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblAgr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            tblAgr.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblAgr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            tblAgr.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblAgr.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            tblAgr.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            void AddLabel(string text, int col) => tblAgr.Controls.Add(new Label
            {
                Text = text, AutoSize = true, Anchor = AnchorStyles.Left,
                TextAlign = ContentAlignment.MiddleLeft, Font = FormLayoutHelper.AppFont,
                ForeColor = FormLayoutHelper.TextColor, Margin = new Padding(0, 4, 6, 0)
            }, col, 0);

            AddLabel("Cliente:", 0);
            _cmbCliente.Dock = DockStyle.Fill;
            tblAgr.Controls.Add(_cmbCliente, 1, 0);

            AddLabel("Empleado:", 2);
            _cmbEmpleado.Dock = DockStyle.Fill;
            tblAgr.Controls.Add(_cmbEmpleado, 3, 0);

            AddLabel("Fecha:", 4);
            _dtpFecha.Dock = DockStyle.Fill;
            tblAgr.Controls.Add(_dtpFecha, 5, 0);

            grpAgregacion.Controls.Add(tblAgr);

            var grpComposicion = new GroupBox
            {
                Text    = "Añadir ítems a la venta",
                Dock    = DockStyle.Top,
                Height  = 65,
                Padding = new Padding(8, 4, 8, 4)
            };
            FormLayoutHelper.StyleGroupBox(grpComposicion);

            var tblComp = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 6,
                RowCount    = 1,
                Font        = FormLayoutHelper.AppFont
            };
            tblComp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblComp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            tblComp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblComp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
            tblComp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblComp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
            tblComp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            tblComp.Controls.Add(new Label
            {
                Text = "Producto:", AutoSize = true, Anchor = AnchorStyles.Left,
                TextAlign = ContentAlignment.MiddleLeft, Font = FormLayoutHelper.AppFont,
                ForeColor = FormLayoutHelper.TextColor, Margin = new Padding(0, 4, 6, 0)
            }, 0, 0);
            _cmbProducto.Dock = DockStyle.Fill;
            tblComp.Controls.Add(_cmbProducto, 1, 0);

            tblComp.Controls.Add(new Label
            {
                Text = "Cantidad:", AutoSize = true, Anchor = AnchorStyles.Left,
                TextAlign = ContentAlignment.MiddleLeft, Font = FormLayoutHelper.AppFont,
                ForeColor = FormLayoutHelper.TextColor, Margin = new Padding(8, 4, 6, 0)
            }, 2, 0);
            _numCantidad.Dock = DockStyle.Fill;
            tblComp.Controls.Add(_numCantidad, 3, 0);

            var btnAnadir = new Button { Text = "＋  Añadir ítem", AutoSize = true, Dock = DockStyle.Fill };
            FormLayoutHelper.StyleButton(btnAnadir, ButtonStyle.Primary);
            btnAnadir.Click += OnAnadirDetalleClick;
            tblComp.Controls.Add(btnAnadir, 5, 0);

            grpComposicion.Controls.Add(tblComp);

            _gridDetalles.Dock               = DockStyle.Top;
            _gridDetalles.Height             = 120;
            _gridDetalles.AutoGenerateColumns = false;
            _gridDetalles.ReadOnly           = true;
            _gridDetalles.AllowUserToAddRows  = false;
            _gridDetalles.AllowUserToDeleteRows = false;
            FormLayoutHelper.StyleGrid(_gridDetalles);

            _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colProd",   HeaderText = "Producto",    Width = 220, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCant",   HeaderText = "Cant.",       Width = 60 });
            _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrecio", HeaderText = "Precio unit.", Width = 100 });
            _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIVA",    HeaderText = "IVA",         Width = 60 });
            _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotal",  HeaderText = "Total ítem",  Width = 100 });
            var colQuitar = new DataGridViewButtonColumn
            {
                Name = "colQuitar", HeaderText = "", Text = "✕", UseColumnTextForButtonValue = true, Width = 40
            };
            _gridDetalles.Columns.Add(colQuitar);

            _gridDetalles.CellClick += OnDetallesCellClick;

            var pnlResumen = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 85,
                BackColor = Color.White,
                Padding   = new Padding(10, 6, 10, 6)
            };

            var tblResumen = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 2
            };
            tblResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            tblResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            tblResumen.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            tblResumen.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            var pnlCodigo = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            pnlCodigo.Controls.Add(new Label { Text = "Código venta:", AutoSize = true, Font = FormLayoutHelper.AppFont, ForeColor = FormLayoutHelper.TextColor, Margin = new Padding(0, 3, 4, 0) });
            _lblCodigo.Text      = $"#{_appState.GetNextCodigoVenta()}";
            _lblCodigo.ForeColor = FormLayoutHelper.AccentBlue;
            pnlCodigo.Controls.Add(_lblCodigo);
            tblResumen.Controls.Add(pnlCodigo, 0, 0);

            var pnlTotales = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };

            _lblSubtotal.Text      = "Subtotal: $0.00";
            _lblSubtotal.ForeColor = FormLayoutHelper.NeutralGray;
            _lblIVA.Text           = "  IVA: $0.00";
            _lblIVA.ForeColor      = FormLayoutHelper.NeutralGray;
            _lblTotal.Text         = "  TOTAL: $0.00";
            _lblTotal.ForeColor    = FormLayoutHelper.AccentBlue;

            pnlTotales.Controls.Add(_lblSubtotal);
            pnlTotales.Controls.Add(_lblIVA);
            pnlTotales.Controls.Add(_lblTotal);
            tblResumen.Controls.Add(pnlTotales, 1, 0);

            var pnlBotones = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding       = new Padding(0)
            };

            var btnConfirmar   = new Button { Text = "✓  Confirmar Venta", AutoSize = true };
            var btnComprobante = new Button { Text = "📋 Ver Comprobante",  AutoSize = true };
            var btnExport      = new Button { Text = "📤  Exportar JSON",   AutoSize = true };
            var btnLimpiar     = new Button { Text = "⟳  Limpiar",         AutoSize = true };

            FormLayoutHelper.StyleButton(btnConfirmar,   ButtonStyle.Success);
            FormLayoutHelper.StyleButton(btnComprobante, ButtonStyle.Neutral);
            FormLayoutHelper.StyleButton(btnExport,      ButtonStyle.Neutral);
            FormLayoutHelper.StyleButton(btnLimpiar,     ButtonStyle.Danger);

            btnConfirmar.Click   += OnConfirmarVentaClick;
            btnComprobante.Click += OnVerComprobanteClick;
            btnExport.Click      += OnExportClick;
            btnLimpiar.Click     += (_, _) => LimpiarFormulario();

            pnlBotones.Controls.Add(btnConfirmar);
            pnlBotones.Controls.Add(btnComprobante);
            pnlBotones.Controls.Add(btnExport);
            pnlBotones.Controls.Add(btnLimpiar);
            tblResumen.Controls.Add(pnlBotones, 0, 1);
            tblResumen.SetColumnSpan(pnlBotones, 2);

            pnlResumen.Controls.Add(tblResumen);

            _gridVentas.Dock               = DockStyle.Fill;
            _gridVentas.AutoGenerateColumns = false;
            _gridVentas.ReadOnly           = true;
            FormLayoutHelper.StyleGrid(_gridVentas);

            _gridVentas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo",     HeaderText = "# Venta", Width = 70 });
            _gridVentas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FechaVenta", HeaderText = "Fecha",   Width = 120 });
            _gridVentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCliente",  HeaderText = "Usuario",   Width = 160, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridVentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmpleado", HeaderText = "Usuario",  Width = 140 });
            _gridVentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItems",    HeaderText = "Ítems",     Width = 55 });
            _gridVentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotal",    HeaderText = "Total",     Width = 100 });

            _gridVentas.CellFormatting += GridVentas_CellFormatting;

            _bindingSource.DataSource = _appState.Ventas;
            _gridVentas.DataSource    = _bindingSource;

            Controls.Add(_gridVentas);      
            Controls.Add(pnlResumen);       
            Controls.Add(_gridDetalles);     
            Controls.Add(grpComposicion);    
            Controls.Add(grpAgregacion);     
        }

        public void RefreshReferenceData()
        {
            _cmbCliente.Items.Clear();
            foreach (var cli in _appState.Clientes.Where(c => c.Activo))
                _cmbCliente.Items.Add(new ComboItem<Usuario>($"{cli.Nombre}", cli));

            _cmbEmpleado.Items.Clear();
            foreach (var emp in _appState.Empleados.Where(e => e.Activo))
                _cmbEmpleado.Items.Add(new ComboItem<Usuario>($"{emp.Nombre}", emp));

            _cmbProducto.Items.Clear();
            foreach (var p in _appState.Productos.Where(p => p.Activo))
                _cmbProducto.Items.Add(new ComboItem<Producto>($"{p.Nombre} — ${p.PrecioVenta:N2} (Stock: {p.StockActual})", p));
        }

        private void OnAnadirDetalleClick(object? sender, EventArgs e)
        {
            if (_cmbProducto.SelectedItem is not ComboItem<Producto> item)
            {
                MessageBox.Show("Seleccione un producto.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var producto  = item.Value;
            var cantidad  = (int)_numCantidad.Value;

            if (producto.StockActual < cantidad)
            {
                MessageBox.Show(
                    $"Stock insuficiente para '{producto.Nombre}'.\n" +
                    $"Disponible: {producto.StockActual} | Solicitado: {cantidad}",
                    "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existente = _detallesActuales.FirstOrDefault(d => d.Producto.Codigo == producto.Codigo);
            if (existente != null)
                existente.Cantidad += cantidad;
            else
                _detallesActuales.Add(new Detallesventum(producto, cantidad)); 

            RefreshDetallesGrid();
            _cmbProducto.SelectedIndex = -1;
            _numCantidad.Value = 1;
        }

        private void OnDetallesCellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_gridDetalles.Columns[e.ColumnIndex].Name == "colQuitar")
            {
                _detallesActuales.RemoveAt(e.RowIndex);
                RefreshDetallesGrid();
            }
        }

        private void RefreshDetallesGrid()
        {
            _gridDetalles.Rows.Clear();
            foreach (var d in _detallesActuales)
            {
                _gridDetalles.Rows.Add(
                    d.Producto.Nombre,
                    d.Cantidad,
                    d.Producto.PrecioVenta.ToString("C"),
                    $"{d.Producto.Impuesto * 100:0}%",
                    d.TotalItem.ToString("C")
                );
            }

            var subtotal = _detallesActuales.Sum(d => d.SubtotalItem);
            var iva      = _detallesActuales.Sum(d => d.IVAItem);
            var total    = _detallesActuales.Sum(d => d.TotalItem);

            _lblSubtotal.Text = $"Subtotal: {subtotal:C}";
            _lblIVA.Text      = $"  IVA: {iva:C}";
            _lblTotal.Text    = $"  TOTAL: {total:C}";
        }

        private async void OnConfirmarVentaClick(object? sender, EventArgs e)
        {
            if (_cmbCliente.SelectedItem is not ComboItem<Usuario> cliItem)
            {
                MessageBox.Show("Debe seleccionar un Usuario.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_cmbEmpleado.SelectedItem is not ComboItem<Usuario> empItem)
            {
                MessageBox.Show("Debe seleccionar un Usuario/vendedor.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_detallesActuales.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un producto a la venta.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var stockError = _appState.ValidarStockParaVenta(_detallesActuales);
            if (stockError != null)
            {
                MessageBox.Show($"Stock insuficiente: {stockError}", "Stock insuficiente",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var codigo = _appState.GetNextCodigoVenta();
            var venta = new Venta(codigo, cliItem.Value, empItem.Value, _dtpFecha.Value);

            var success = await GridActions.RunDatabaseOperationAsync(this, "Crear Venta", async () =>
            {
                await _appState.AddVentaAsync(venta, _detallesActuales);
                venta.SaveToJson(GridActions.GetOutputPath("venta", venta.Codigo));
            });

            if (success)
            {
                RefreshReferenceData();
                _lblCodigo.Text = $"#{_appState.GetNextCodigoVenta()}";
                MessageBox.Show($"Venta #{codigo} creada.\nTotal: {venta.Detallesventa.Sum(d => d.Cantidad * d.Producto.PrecioVenta):C}",
                    "Venta registrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
            }
        }

        private void OnVerComprobanteClick(object? sender, EventArgs e)
        {
            Venta? venta = _gridVentas.CurrentRow?.DataBoundItem as Venta;
            if (venta == null)
            {
                MessageBox.Show("Seleccione una venta del historial para ver su comprobante.",
                    "Sin selección", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var frmComprobante = new Form
            {
                Text        = $"Comprobante de Venta #{venta.Codigo}",
                Size        = new Size(520, 520),
                StartPosition = FormStartPosition.CenterParent,
                BackColor   = Color.White,
                Font        = new Font("Courier New", 10f)
            };
            var txtComp = new TextBox
            {
                Multiline   = true,
                ReadOnly    = true,
                Dock        = DockStyle.Fill,
                Font        = new Font("Courier New", 10f),
                Text        = venta.GenerarTextoComprobante(),
                ScrollBars  = ScrollBars.Vertical,
                BackColor   = Color.White,
                ForeColor   = FormLayoutHelper.TextColor,
                BorderStyle = BorderStyle.None,
                Padding     = new Padding(10)
            };
            frmComprobante.Controls.Add(txtComp);
            frmComprobante.ShowDialog(this);
        }

        private void OnExportClick(object? sender, EventArgs e) =>
            GridActions.ExportSelected<Venta>(_gridVentas, "venta.json", (v, path) => v.SaveToJson(path));

        private void LimpiarFormulario()
        {
            _cmbCliente.SelectedIndex  = -1;
            _cmbEmpleado.SelectedIndex = -1;
            _cmbProducto.SelectedIndex = -1;
            _numCantidad.Value         = 1;
            _dtpFecha.Value            = DateTime.Now;
            _detallesActuales.Clear();
            RefreshDetallesGrid();
        }

        private void GridVentas_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_gridVentas.Rows[e.RowIndex].DataBoundItem is not Venta v) return;

            switch (_gridVentas.Columns[e.ColumnIndex].Name)
            {
                case "colCliente":
                    e.Value = v.Cliente?.Nombre ?? "—";
                    e.FormattingApplied = true;
                    break;
                case "colEmpleado":
                    e.Value = v.Empleado?.Nombre ?? "—";
                    e.FormattingApplied = true;
                    break;
                case "colItems":
                    e.Value = v.Detallesventa.Sum(d => d.Cantidad).ToString();
                    e.FormattingApplied = true;
                    break;
                case "colTotal":
                    e.Value = v.Detallesventa.Sum(d => d.Cantidad * d.Producto.PrecioVenta).ToString("C");
                    e.FormattingApplied = true;
                    break;
            }
        }
    }
}


