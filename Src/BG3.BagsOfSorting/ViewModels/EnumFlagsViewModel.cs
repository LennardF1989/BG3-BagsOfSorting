#pragma warning disable S3237
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable ValueParameterNotUsed

namespace BG3.BagsOfSorting.ViewModels
{
    //Based on: https://stackoverflow.com/a/49559152/890815
    public class EnumFlagsViewModel<T> : BaseViewModel where T : struct, IComparable, IFormattable, IConvertible
    {
        private T _value;

        public T Value
        {
            get { return _value; }
            set
            {
                if (SetField(ref _value, value))
                {
                    OnPropertyChanged("Item[]");
                }
            }
        }
        
        public bool this[T key]
        {
            get
            {
                return ((int)(object)_value & (int)(object)key) == (int)(object)key;
            }
            set
            {
                var newValue = (T)(object)((int)(object)_value ^ (int)(object)key);

                if (SetField(ref _value, newValue))
                {
                    OnPropertyChanged("Item[]");
                }
            }
        }

        public EnumFlagsViewModel(T t)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"{nameof(T)} must be an enum type");
            }

            _value = t;
        }
    }
}
