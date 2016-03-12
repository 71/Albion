using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    public interface IInputConverter
    {
        Type Type { get; }

        bool CanConvert(string input);
        object Convert(string input);
    }

    public abstract class InputConverter<T> : IInputConverter
    {
        public Type Type { get { return typeof(T); } }

        public abstract bool CanConvert(string input);
        protected abstract T ConvertInternal(string input);

        public object Convert(string input)
        {
            return ConvertInternal(input);
        }
    }

    internal class IntConverter : InputConverter<int>
    {
        public override bool CanConvert(string input)
        {
            return input.ToCharArray().All(c => char.IsDigit(c));
        }

        protected override int ConvertInternal(string input)
        {
            return int.Parse(input);
        }
    }

    internal class FloatConverter : InputConverter<float>
    {
        public override bool CanConvert(string input)
        {
            return input.ToCharArray().All(c => char.IsDigit(c)) && input.Count(c => c == '.') < 2;
        }

        protected override float ConvertInternal(string input)
        {
            return float.Parse(input);
        }
    }

    internal class DoubleConverter : FloatConverter { }

    internal class CharConverter : InputConverter<char>
    {
        public override bool CanConvert(string input)
        {
            return input.Length == 1;
        }

        protected override char ConvertInternal(string input)
        {
            return char.Parse(input);
        }
    }

    internal class BoolConverter : InputConverter<bool>
    {
        public override bool CanConvert(string input)
        {
            input = input.ToLower();
            return input == "true" || input == "false";
        }

        protected override bool ConvertInternal(string input)
        {
            return bool.Parse(input);
        }
    }
}
