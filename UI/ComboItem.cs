namespace TiendaLinea.UI
{
    public class ComboItem<T>
    {
        public string Text { get; }
        public T Value { get; }

        public ComboItem(string text, T value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString() => Text;
    }
}


