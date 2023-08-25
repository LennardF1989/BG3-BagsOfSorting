namespace BG3.BagsOfSorting.ViewModels
{
    public class AdditionalTreasureViewModel : BaseViewModel
    {
        public enum EType
        {
            Item = 0,
            TreasureTable
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public EType Type
        {
            get => _type;
            set => SetField(ref _type, value);
        }

        public int Amount
        {
            get => _amount;
            set => SetField(ref _amount, value);
        }

        private string _name;
        private EType _type;
        private int _amount;
    }
}