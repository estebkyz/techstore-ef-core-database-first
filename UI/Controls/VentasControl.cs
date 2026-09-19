using TiendaLinea.Models;
using TiendaLinea.UI;

namespace TiendaLinea.UI.Controls
{
    public class VentasControl : UserControl, IReferenceDataConsumer
    {
        private readonly AppState _state;
        private readonly ErrorProvider _errors = new();

        private DataGridView _gridVentas = null!, _gridDetalles = null!;
        private TextBox _txtCodigoVenta = null!;
        private ComboBox _cmbCliente = null!, _cmbEmpleado = null!;
        private TextBox _txtCantidad = null!;
        private ComboBox _cmbProducto = null!;
        private Button _btnCrear = null!, _btnEliminar = null!, _btnAgregarDet = null!, _btnLimpiarVenta = null!;
        
        private Venta? _selectedVenta;
        private readonly List<Detallesventum> _detallesBorrador = new(); // Para armar la venta antes de guardar

        public VentasControl(AppState state) { _state = state; BuildUI(); }

        private void BuildUI()
        {
            Dock = DockStyle.Fill;
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 250 };

            // TOP PANEL (Ventas)
            var pnlTop = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, Padding = new Padding(8) };
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            void AddRow(TableLayoutPanel p, string lbl, Control ctrl, int r) { p.Controls.Add(new Label { Text = lbl, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, r); p.Controls.Add(ctrl, 1, r); }

            _txtCodigoVenta = new TextBox { Dock = DockStyle.Fill };
            _cmbCliente     = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbEmpleado    = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

            AddRow(pnlTop, "Código:",   _txtCodigoVenta, 0);
            AddRow(pnlTop, "Cliente:",  _cmbCliente,     1);
            AddRow(pnlTop, "Empleado:", _cmbEmpleado,    2);

            var btnTop = new FlowLayoutPanel { Dock = DockStyle.Fill };
            _btnCrear        = new Button { Text = "Registrar Venta", Width = 110 };
            _btnEliminar     = new Button { Text = "Eliminar Venta",  Width = 110 };
            _btnLimpiarVenta = new Button { Text = "Nueva",           Width = 90 };
            btnTop.Controls.AddRange(new Control[] { _btnCrear, _btnEliminar, _btnLimpiarVenta });
            pnlTop.Controls.Add(btnTop, 1, 3);

            _gridVentas = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, AutoGenerateColumns = false };
            _gridVentas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "Código" });
            _gridVentas.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Fecha",  HeaderText = "Fecha" });
            
            var colCli = new DataGridViewTextBoxColumn { HeaderText = "Cliente" };
            _gridVentas.Columns.Add(colCli);
            _gridVentas.CellFormatting += (s, e) => {
                if (e.ColumnIndex == 2 && e.RowIndex >= 0 && _gridVentas.Rows[e.RowIndex].DataBoundItem is Venta v)
                    e.Value = v.Cliente?.Nombre ?? "N/A";
            };

            var colEmp = new DataGridViewTextBoxColumn { HeaderText = "Empleado" };
            _gridVentas.Columns.Add(colEmp);
            _gridVentas.CellFormatting += (s, e) => {
                if (e.ColumnIndex == 3 && e.RowIndex >= 0 && _gridVentas.Rows[e.RowIndex].DataBoundItem is Venta v)
                    e.Value = v.Empleado?.Nombre ?? "N/A";
            };

            _gridVentas.DataSource = _state.Ventas;

            var pnlTopContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 350 };
            pnlTopContainer.Panel1.Controls.Add(pnlTop);
            pnlTopContainer.Panel2.Controls.Add(_gridVentas);
            split.Panel1.Controls.Add(pnlTopContainer);

            // BOTTOM PANEL (Detalles)
            var pnlBot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(8) };
            pnlBot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            pnlBot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _cmbProducto = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _txtCantidad = new TextBox { Dock = DockStyle.Fill };
            _btnAgregarDet = new Button { Text = "Añadir a Venta Actual", Width = 150 };

            AddRow(pnlBot, "Producto:", _cmbProducto, 0);
            AddRow(pnlBot, "Cantidad:", _txtCantidad, 1);
            pnlBot.Controls.Add(_btnAgregarDet, 1, 2);

            _gridDetalles = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, AutoGenerateColumns = false };
            var colProd = new DataGridViewTextBoxColumn { HeaderText = "Producto" };
            _gridDetalles.Columns.Add(colProd);
            _gridDetalles.CellFormatting += (s, e) => {
                if (e.ColumnIndex == 0 && e.RowIndex >= 0 && _gridDetalles.Rows[e.RowIndex].DataBoundItem is Detallesventum d)
                {
                    if (d.ProductoCodigoNavigation != null) e.Value = d.ProductoCodigoNavigation.Nombre;
                    else e.Value = _state.Productos.FirstOrDefault(p => p.Codigo == d.ProductoCodigo)?.Nombre ?? d.ProductoCodigo.ToString();
                }
            };
            _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Cantidad",       HeaderText = "Cantidad" });
            _gridDetalles.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PrecioUnitario", HeaderText = "Precio Unit." });

            var pnlBotContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 350 };
            pnlBotContainer.Panel1.Controls.Add(pnlBot);
            pnlBotContainer.Panel2.Controls.Add(_gridDetalles);
            split.Panel2.Controls.Add(pnlBotContainer);

            Controls.Add(split);

            _gridVentas.SelectionChanged += (_, _) => LoadSelectedVenta();
            _btnCrear.Click        += async (_, _) => await OnCrearVentaAsync();
            _btnEliminar.Click     += async (_, _) => await OnEliminarVentaAsync();
            _btnLimpiarVenta.Click += (_, _) => LimpiarVenta();
            _btnAgregarDet.Click   += (_, _) => OnAgregarBorrador();
        }

        public void RefreshReferenceData()
        {
            void BindCombo(ComboBox cmb, IEnumerable<ComboItem> items)
            {
                var old = cmb.SelectedValue;
                cmb.DataSource = items.ToList();
                cmb.DisplayMember = "Display";
                cmb.ValueMember = "Value";
                if (old != null) cmb.SelectedValue = old;
            }

            BindCombo(_cmbCliente,  _state.Clientes.Select(c => new ComboItem(c.Nombre, c.Id)).Prepend(new ComboItem("-- Seleccione --", "")));
            BindCombo(_cmbEmpleado, _state.Empleados.Select(e => new ComboItem(e.Nombre, e.Id)).Prepend(new ComboItem("-- Seleccione --", "")));
            BindCombo(_cmbProducto, _state.Productos.Select(p => new ComboItem(p.Nombre, p.Codigo)).Prepend(new ComboItem("-- Seleccione --", "")));
        }

        private void LimpiarVenta()
        {
            _selectedVenta = null;
            _txtCodigoVenta.Text = _txtCantidad.Text = "";
            _cmbCliente.SelectedIndex = _cmbEmpleado.SelectedIndex = _cmbProducto.SelectedIndex = 0;
            _txtCodigoVenta.ReadOnly = false;
            _detallesBorrador.Clear();
            _gridDetalles.DataSource = null;
            _gridDetalles.DataSource = _detallesBorrador;
            _errors.Clear();
        }

        private void LoadSelectedVenta()
        {
            if (_gridVentas.CurrentRow?.DataBoundItem is not Venta v) { _selectedVenta = null; return; }
            _selectedVenta = v;
            _txtCodigoVenta.Text = v.Codigo.ToString();
            _cmbCliente.SelectedValue = v.ClienteId ?? (object)"";
            _cmbEmpleado.SelectedValue = v.EmpleadoId ?? (object)"";
            _txtCodigoVenta.ReadOnly = true;
            
            _gridDetalles.DataSource = v.Detallesventa.ToList(); // Detalles guardados
        }

        private void OnAgregarBorrador()
        {
            if (_selectedVenta != null) { MessageBox.Show("No puede modificar los detalles de una venta existente. Cree una nueva."); return; }
            
            if (_cmbProducto.SelectedValue is not int pId || pId == 0) { MessageBox.Show("Seleccione producto."); return; }
            if (!Validators.ValidatePositiveInt(_txtCantidad.Text, "Cantidad", _errors, _txtCantidad, out var cant)) return;

            var prod = _state.Productos.First(p => p.Codigo == pId);
            _detallesBorrador.Add(new Detallesventum { ProductoCodigo = pId, Cantidad = cant, PrecioUnitario = prod.Precio });
            
            _gridDetalles.DataSource = null;
            _gridDetalles.DataSource = _detallesBorrador;
        }

        private async Task OnCrearVentaAsync()
        {
            if (!Validators.ValidatePositiveInt(_txtCodigoVenta.Text, "Código", _errors, _txtCodigoVenta, out var cod)) return;
            if (_cmbCliente.SelectedValue is not int cId || cId == 0) { MessageBox.Show("Seleccione cliente."); return; }
            if (_cmbEmpleado.SelectedValue is not int eId || eId == 0) { MessageBox.Show("Seleccione empleado."); return; }
            if (!_detallesBorrador.Any()) { MessageBox.Show("Agregue al menos un producto."); return; }

            var v = new Venta { Codigo = cod, ClienteId = cId, EmpleadoId = eId, Fecha = DateTime.Now };
            await GridActions.RunDatabaseOperationAsync(() => _state.AddVentaAsync(v, _detallesBorrador), msg => MessageBox.Show(msg, "Error"));
            LimpiarVenta();
        }

        private async Task OnEliminarVentaAsync()
        {
            if (_selectedVenta is null) return;
            if (MessageBox.Show($"¿Eliminar venta {_selectedVenta.Codigo} ({_selectedVenta.Detallesventa.Count} items)?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await GridActions.RunDatabaseOperationAsync(() => _state.DeleteVentaAsync(_selectedVenta), msg => MessageBox.Show(msg, "Error"));
            LimpiarVenta();
        }
    }
}
