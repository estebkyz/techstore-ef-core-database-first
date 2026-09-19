using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TiendaLinea.UI.Controls;

namespace TiendaLinea.UI
{
    public class MainForm : Form
    {
        private readonly AppState _appState = new();
        private readonly TabControl _tabControl = new();
        private ToolStripStatusLabel _statusLabel = new();

        public MainForm()
        {
            Text             = "TechStore";
            Size             = new Size(1100, 750);
            MinimumSize      = new Size(900, 600);
            StartPosition    = FormStartPosition.CenterScreen;
            BackColor        = FormLayoutHelper.BackgroundColor;
            Font             = FormLayoutHelper.AppFont;
            InitializeLayout();
            Load += OnFormLoad;
        }

        private async void OnFormLoad(object? sender, EventArgs e)
        {
            await GridActions.RunDatabaseOperationAsync(this, "Cargando datos", async () =>
            {
                await _appState.LoadAllFromDatabaseAsync();
                foreach (TabPage page in _tabControl.TabPages)
                    foreach (Control ctrl in page.Controls)
                        if (ctrl is IReferenceDataConsumer consumer)
                            consumer.RefreshReferenceData();
            });
            UpdateStatus();
        }

        private void InitializeLayout()
        {
            // ── Header ──────────────────────────────────────────────
            var headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = FormLayoutHelper.AccentBlue
            };

            var lblTitle = new Label
            {
                Text      = "TechStore",
                ForeColor = Color.White,
                Font      = FormLayoutHelper.TitleFont,
                AutoSize  = true,
                Location  = new Point(18, 10)
            };
            var lblSub = new Label
            {
                Text      = "Sistema de Gestión de Inventario y Ventas",
                ForeColor = Color.FromArgb(187, 222, 251),
                Font      = new Font("Segoe UI", 8.5f),
                AutoSize  = true,
                Location  = new Point(20, 36)
            };

            var btnLoadDemo = new Button
            {
                Text      = "⟳  Datos de demo",
                Height    = 38,
                Width     = 145,
                Dock      = DockStyle.Right,
                BackColor = Color.FromArgb(30, 136, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = FormLayoutHelper.BoldFont,
                Cursor    = Cursors.Hand
            };
            btnLoadDemo.FlatAppearance.BorderSize = 0;
            btnLoadDemo.Click += OnLoadDemoClick;

            headerPanel.Controls.Add(lblSub);
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(btnLoadDemo);

            // ── Status strip ──────────────────────────────────────
            var statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(30, 30, 60),
                SizingGrip = false
            };
            _statusLabel = new ToolStripStatusLabel
            {
                Text      = "Listo | Productos: 0  |  Clientes: 0  |  Empleados: 0  |  Ventas: 0",
                ForeColor = Color.FromArgb(144, 202, 249),
                Font      = FormLayoutHelper.SmallFont
            };
            statusStrip.Items.Add(_statusLabel);

            // ── Tab control ────────────────────────────────────────
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _tabControl.Padding = new Point(12, 4);

            var prodControl  = new ProductosControl(_appState);
            var cliControl   = new ClientesControl(_appState);
            var empControl   = new EmpleadosControl(_appState);
            var adminControl = new AdministradoresControl(_appState);
            var ventasControl = new VentasControl(_appState);

            _tabControl.TabPages.Add(BuildTabPage("  Productos",       prodControl));
            _tabControl.TabPages.Add(BuildTabPage("  Clientes",        cliControl));
            _tabControl.TabPages.Add(BuildTabPage("  Empleados",       empControl));
            _tabControl.TabPages.Add(BuildTabPage("  Administradores", adminControl));
            _tabControl.TabPages.Add(BuildTabPage("  Ventas",          ventasControl));

            // Live counter
            _appState.Productos.ListChanged       += (_, _) => UpdateStatus();
            _appState.Clientes.ListChanged        += (_, _) => UpdateStatus();
            _appState.Empleados.ListChanged       += (_, _) => UpdateStatus();
            _appState.Administradores.ListChanged += (_, _) => UpdateStatus();
            _appState.Ventas.ListChanged          += (_, _) => UpdateStatus();

            Controls.Add(_tabControl);    // Fill
            Controls.Add(statusStrip);    // Bottom
            Controls.Add(headerPanel);    // Top
        }

        private void UpdateStatus()
        {
            _statusLabel.Text =
                $"Productos: {_appState.Productos.Count}  |  " +
                $"Clientes: {_appState.Clientes.Count}  |  " +
                $"Empleados: {_appState.Empleados.Count}  |  " +
                $"Ventas: {_appState.Ventas.Count}";
        }

        private TabPage BuildTabPage(string title, UserControl control)
        {
            control.Dock = DockStyle.Fill;
            control.BackColor = FormLayoutHelper.BackgroundColor;
            var page = new TabPage(title)
            {
                BackColor = FormLayoutHelper.BackgroundColor,
                UseVisualStyleBackColor = false
            };
            page.Controls.Add(control);
            return page;
        }

        private async void OnLoadDemoClick(object? sender, EventArgs e)
        {
            var success = await GridActions.RunDatabaseOperationAsync(this, "Cargar Demo", async () =>
            {
                await _appState.LoadDemoDataAsync();
                foreach (TabPage page in _tabControl.TabPages)
                    foreach (Control ctrl in page.Controls)
                        if (ctrl is IReferenceDataConsumer consumer)
                            consumer.RefreshReferenceData();
            });
            
            if (success)
            {
                UpdateStatus();
                MessageBox.Show("Datos de demostración cargados en la base de datos MySQL.",
                    "TechStore", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}


