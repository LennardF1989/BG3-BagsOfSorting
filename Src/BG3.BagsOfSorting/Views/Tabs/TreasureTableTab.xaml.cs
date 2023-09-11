using BG3.BagsOfSorting.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace BG3.BagsOfSorting.Views.Tabs
{
    public partial class TreasureTableTab : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<AdditionalTreasureViewModel> AdditionalTreasures { get; set; }
        public ObservableCollection<string> TreasureTypes { get; set; }

        public AdditionalTreasureViewModel SelectedAdditionalTreasure
        {
            get => _selectedAdditionalTreasure;
            set
            {
                if (SetField(ref _selectedAdditionalTreasure, value))
                {
                    OnPropertyChanged(nameof(IsAdditionalTreasureSelected));
                }
            }
        }

        public bool IsAdditionalTreasureSelected => SelectedAdditionalTreasure != null;

        public string TreasureTableFolderName
        {
            get => _treasureTableFolderName;
            set => SetField(ref _treasureTableFolderName, value);
        }

        public string TreasureTableName
        {
            get => _treasureTableName;
            set => SetField(ref _treasureTableName, value);
        }

        private AdditionalTreasureViewModel _selectedAdditionalTreasure;
        private string _treasureTableFolderName;
        private string _treasureTableName;

        public TreasureTableTab()
        {
            var configuration = ServiceLocator.Configuration;

            AdditionalTreasures = new ObservableCollection<AdditionalTreasureViewModel>();
            foreach (var kvp in configuration.AdditionalTreasures.OrderBy(x => x.Key))
            {
                var prefix = kvp.Key[..2];

                var name = kvp.Key;
                var type = AdditionalTreasureViewModel.EType.None;

                switch (prefix)
                {
                    case "I_":
                        name = kvp.Key[2..];
                        type = AdditionalTreasureViewModel.EType.Item;
                        break;

                    case "T_":
                        name = kvp.Key[2..];
                        type = AdditionalTreasureViewModel.EType.TreasureTable;
                        break;
                }

                AdditionalTreasures.Add(new AdditionalTreasureViewModel
                {
                    DisplayName = name,
                    Type = type,
                    Amount = kvp.Value.GetValue<int>()
                });
            }

            TreasureTableFolderName = configuration.TreasureTable?.FolderName ?? Constants.DEFAULT_TREASURETABLE_FOLDERNAME;
            TreasureTableName = configuration.TreasureTable?.Name ?? Constants.DEFAULT_TREASURETABLE_NAME;

            TreasureTypes = new ObservableCollection<string>(Enum.GetNames(typeof(AdditionalTreasureViewModel.EType)));

            InitializeComponent();
        }

        public void Update()
        {
            _selectedAdditionalTreasure?.Update();
        }

        private void AddAdditionalTreasure(object sender, RoutedEventArgs e)
        {
            AdditionalTreasures.Add(new AdditionalTreasureViewModel
            {
                Amount = 1
            });
        }

        private void RemoveAdditionalTreasure(object sender, RoutedEventArgs e)
        {
            var result = SelectedAdditionalTreasure != null && MessageBox.Show(
                $"Are you sure you want to remove '{SelectedAdditionalTreasure.DisplayName}'?",
                "Remove Additional Treasure",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) == MessageBoxResult.Yes;

            if (!result)
            {
                return;
            }

            AdditionalTreasures.Remove(SelectedAdditionalTreasure);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;

            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
