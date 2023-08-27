namespace BG3.BagsOfSorting.ViewModels
{
    public class AdditionalTreasureViewModel : BaseViewModel
    {
        public enum EType
        {
            None = 0,
            Item,
            TreasureTable
        }

        public string Name
        {
            get
            {
                var prefix = Type switch
                {
                    EType.Item => "I_",
                    EType.TreasureTable => "T_",
                    _ => string.Empty
                };

                return $"{prefix}{DisplayName}";
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (SetField(ref _displayName, value))
                {
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public EType Type
        {
            get => _type;
            set
            {
                if (SetField(ref _type, value))
                {
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public int Amount
        {
            get => _amount;
            set => SetField(ref _amount, value);
        }

        private string _displayName;
        private EType _type;
        private int _amount;

        public void Update()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(Amount));
        }
    }
}