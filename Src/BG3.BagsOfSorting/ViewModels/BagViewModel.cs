using BG3.BagsOfSorting.Models;

namespace BG3.BagsOfSorting.ViewModels
{
    public class BagViewModel : BaseViewModel
    {
        // ReSharper disable once UnusedMember.Global
        public string Name => Bag.Name;
        public string DisplayName => Bag.DisplayName;
        public BagConfiguration.Bag Bag { get; set; }

        public void Update()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Bag));
        }
    }
}
