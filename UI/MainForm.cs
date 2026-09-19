using TiendaLinea.UI.Controls;

namespace TiendaLinea.UI
{
    public class MainForm : Form
    {
        private readonly AppState _state = new();
        private TabControl _tabs = null!;
        private readonly List<IReferenceDataConsumer> _consumers = new();

        public MainForm()
        {
            Text = "TechStore Database-First (WinForms + EF Core)";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5) };
            var btnReload = new Button { Text = "Recargar BD", Width = 100 };
            var btnDemo   = new Button { Text = "Cargar Demo", Width = 100 };
            topPanel.Controls.AddRange(new Control[] { btnReload, btnDemo });
            Controls.Add(topPanel);

            _tabs = new TabControl { Dock = DockStyle.Fill };
            Controls.Add(_tabs);

            AddTab("Clientes",        new ClientesControl(_state));
            AddTab("Empleados",       new EmpleadosControl(_state));
            AddTab("Administradores", new AdministradoresControl(_state));
            AddTab("Categorías",      new CategoriasControl(_state));
            AddTab("Productos",       new ProductosControl(_state));
            AddTab("Ventas",          new VentasControl(_state));

            _tabs.SelectedIndexChanged += (_, _) => NotifyConsumers();

            btnReload.Click += async (_, _) => await ReloadAsync();
            btnDemo.Click   += async (_, _) => { await _state.LoadDemoDataAsync(); NotifyConsumers(); };

            Load += async (_, _) => await ReloadAsync();
        }

        private void AddTab(string title, Control content)
        {
            var tab = new TabPage(title);
            content.Dock = DockStyle.Fill;
            tab.Controls.Add(content);
            _tabs.TabPages.Add(tab);
            if (content is IReferenceDataConsumer consumer) _consumers.Add(consumer);
        }

        private async Task ReloadAsync()
        {
            await _state.LoadAllFromDatabaseAsync();
            NotifyConsumers();
        }

        private void NotifyConsumers()
        {
            foreach (var c in _consumers) c.RefreshReferenceData();
        }
    }
}
