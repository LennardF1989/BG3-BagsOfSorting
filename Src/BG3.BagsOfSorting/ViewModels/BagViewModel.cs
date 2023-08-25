using BG3.BagsOfSorting.Models;

namespace BG3.BagsOfSorting.ViewModels
{
    public class BagViewModel : BaseViewModel
    {
        public string Name => Bag.Name;
        public string DisplayName => Bag.DisplayName;
        public BagConfiguration.Bag Bag { get; set; }

        public void Update()
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Bag));
        }
    }
}
