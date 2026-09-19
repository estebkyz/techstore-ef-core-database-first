namespace TiendaLinea.UI
{
    public class ComboItem
    {
        public string Display { get; }
        public object Value   { get; }

        public ComboItem(string display, object value)
        {
            Display = display;
            Value   = value;
        }

        public override string ToString() => Display;
    }
}
