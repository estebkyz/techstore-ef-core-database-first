using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaLinea.UI
{
    public enum ButtonStyle { Primary, Danger, Success, Neutral }

    internal static class FormLayoutHelper
    {
        public static readonly Color BackgroundColor = Color.FromArgb(240, 244, 248); // #F0F4F8
        public static readonly Color PanelColor      = Color.White;
        public static readonly Color AccentBlue      = Color.FromArgb(21, 101, 192);  // #1565C0
        public static readonly Color PrimaryBlue     = Color.FromArgb(25, 118, 210);  // #1976D2
        public static readonly Color DangerRed       = Color.FromArgb(211, 47, 47);   // #D32F2F
        public static readonly Color SuccessGreen    = Color.FromArgb(56, 142, 60);   // #388E3C
        public static readonly Color NeutralGray     = Color.FromArgb(84, 110, 122);  // #546E7A
        public static readonly Color TextColor       = Color.FromArgb(26, 35, 126);   // #1A237E
        public static readonly Color TextDark        = Color.FromArgb(33, 33, 33);    // #212121
        public static readonly Color StockAlertColor = Color.FromArgb(255, 243, 224); // #FFF3E0 naranja pálido
        public static readonly Color GridAltRow      = Color.FromArgb(232, 240, 254); // #E8F0FE azul muy pálido
        public static readonly Color SelectionColor  = Color.FromArgb(197, 217, 247); // Azul selección

        public static readonly Font AppFont    = new("Segoe UI", 9f);
        public static readonly Font BoldFont   = new("Segoe UI", 9f, FontStyle.Bold);
        public static readonly Font TitleFont  = new("Segoe UI", 14f, FontStyle.Bold);
        public static readonly Font HeaderFont = new("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font SmallFont  = new("Segoe UI", 8f);

        public static TableLayoutPanel CreateFormPanel()
        {
            return new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize    = true,
                Padding     = new Padding(12, 8, 12, 8),
                Dock        = DockStyle.Top,
                BackColor   = PanelColor,
                Font        = AppFont,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.AutoSize),
                    new ColumnStyle(SizeType.Percent, 100f)
                }
            };
        }

        public static void AddRow(TableLayoutPanel panel, string labelText, Control control)
        {
            var label = new Label
            {
                Text      = labelText,
                AutoSize  = true,
                Anchor    = AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = AppFont,
                ForeColor = TextColor,
                Margin    = new Padding(0, 6, 8, 0)
            };
            control.Dock   = DockStyle.Fill;
            control.Margin = new Padding(0, 4, 0, 4);
            control.Font   = AppFont;

            int row = panel.RowCount;
            panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(label, 0, row);
            panel.Controls.Add(control, 1, row);
        }

        public static void StyleButton(Button btn, ButtonStyle style = ButtonStyle.Primary)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font      = BoldFont;
            btn.ForeColor = Color.White;
            btn.Cursor    = Cursors.Hand;
            btn.Padding   = new Padding(10, 4, 10, 4);
            btn.Height    = 32;

            var normalColor = style switch
            {
                ButtonStyle.Primary => PrimaryBlue,
                ButtonStyle.Danger  => DangerRed,
                ButtonStyle.Success => SuccessGreen,
                ButtonStyle.Neutral => NeutralGray,
                _                  => PrimaryBlue
            };
            var hoverColor = ControlPaint.Dark(normalColor, 0.12f);

            btn.BackColor = normalColor;
            btn.MouseEnter += (_, _) => btn.BackColor = hoverColor;
            btn.MouseLeave += (_, _) => btn.BackColor = normalColor;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor    = PanelColor;
            grid.BorderStyle        = BorderStyle.None;
            grid.GridColor          = Color.FromArgb(224, 224, 224);
            grid.Font               = AppFont;

            grid.DefaultCellStyle.Font              = AppFont;
            grid.DefaultCellStyle.BackColor         = Color.White;
            grid.DefaultCellStyle.ForeColor         = TextDark;
            grid.DefaultCellStyle.SelectionBackColor = SelectionColor;
            grid.DefaultCellStyle.SelectionForeColor = TextColor;
            grid.DefaultCellStyle.Padding           = new Padding(4, 2, 4, 2);

            grid.AlternatingRowsDefaultCellStyle.BackColor          = GridAltRow;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = SelectionColor;

            grid.ColumnHeadersDefaultCellStyle.BackColor         = AccentBlue;
            grid.ColumnHeadersDefaultCellStyle.ForeColor         = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font              = BoldFont;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = AccentBlue;
            grid.ColumnHeadersDefaultCellStyle.Padding           = new Padding(4, 0, 0, 0);

            grid.ColumnHeadersBorderStyle              = DataGridViewHeaderBorderStyle.None;
            grid.EnableHeadersVisualStyles             = false;
            grid.CellBorderStyle                       = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowHeadersVisible                     = false;
            grid.ColumnHeadersHeightSizeMode           = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight                   = 34;
            grid.RowTemplate.Height                    = 28;
            grid.SelectionMode                         = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect                           = false;
            grid.AllowUserToAddRows                    = false;
            
            grid.DataBindingComplete += (s, e) =>
            {
                var g = (DataGridView)s!;
                foreach (DataGridViewColumn col in g.Columns)
                {
                    if (col.Name.StartsWith("Venta") || 
                        col.Name.StartsWith("Detalle") || 
                        col.Name == "ClaveHash" || 
                        col.Name == "TipoUsuario" ||
                        col.Name == "StockBajo")
                    {
                        col.Visible = false;
                    }
                }
            };
        }

        public static void StyleGroupBox(GroupBox grp)
        {
            grp.BackColor = PanelColor;
            grp.Font      = BoldFont;
            grp.ForeColor = AccentBlue;
            grp.Padding   = new Padding(10, 4, 10, 8);
        }

        public static FlowLayoutPanel CreateButtonBar()
        {
            return new FlowLayoutPanel
            {
                Dock      = DockStyle.Bottom,
                Height    = 48,
                BackColor = PanelColor,
                Padding   = new Padding(8, 8, 8, 8),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };
        }
    }
}


