using SignNow.Net.Interfaces;
using SignNow.Net.Model;

namespace StartedIn.CrossCutting.Customize
{
    public class EditableField : IFieldEditable
    {
        public int PageNumber { get; set; }

        public FieldType Type { get; set; }

        public string Name { get; set; }
        public string Role { get; set; }
        public bool Required { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}