using BG3.BagsOfSorting.Models;
using BG3.BagsOfSorting.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using BG3.BagsOfSorting.Services;
using System.Windows;
using System.Runtime.CompilerServices;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace BG3.BagsOfSorting.Views.Tabs
{
    public partial class BagsTab : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<BagViewModel> Bags { get; set; }
        public ObservableCollection<string> BagColors { get; set; }
        public bool AlignGeneratedItemIconsRight { get; set; }
        public bool BundlePouchOfWonders { get; set; }

        public BagViewModel SelectedBag
        {
            get => _selectedBag;
            set
            {
                _selectedBag?.Update();

                if (SetField(ref _selectedBag, value))
                {
                    OnPropertyChanged(nameof(IsBagSelected));
                }
            }
        }

        public bool IsBagSelected => SelectedBag != null;

        private BagViewModel _selectedBag;

        public BagsTab()
        {
            var configuration = ServiceLocator.Configuration;

            Bags = new ObservableCollection<BagViewModel>();

            foreach (var bag in configuration.Bags.OrderBy(x => x.DisplayName))
            {
                bag.Name = CLIMethods.GetNameForBag(bag);

                Bags.Add(new BagViewModel
                {
                    Bag = bag
                });
            }

            BagColors = new ObservableCollection<string>(Enum.GetNames(typeof(Configuration.EColor)));

            AlignGeneratedItemIconsRight = configuration.AlignGeneratedItemIconsRight;

            BundlePouchOfWonders = configuration.BundlePouchOfWonders;

            InitializeComponent();

            BagsDataGrid.AutoGeneratingColumn += (_, args) =>
            {
                if (args.PropertyName == nameof(BagViewModel.Bag))
                {
                    args.Cancel = true;
                }
            };
        }

        public void Update()
        {
            _selectedBag?.Update();
        }

        private void AddBag(object sender, RoutedEventArgs e)
        {
            var bag = CLIMethods.GetNewBag();
            bag.Name = CLIMethods.GetNameForBag(bag);
            bag.DisplayName = "Bag";

            Bags.Add(new BagViewModel
            {
                Bag = bag
            });
        }

        private void RemoveBag(object sender, RoutedEventArgs e)
        {
            var result = SelectedBag != null && MessageBox.Show(
                $"If this bag is in use in-game, your save will become corrupted! Are you sure you want to remove '{SelectedBag.DisplayName}'?",
                "Remove Bag",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) == MessageBoxResult.Yes;

            if (!result)
            {
                return;
            }

            Bags.Remove(SelectedBag);
        }

        private void BrowseItemIcon(object sender, RoutedEventArgs e)
        {
            if (SelectedBag == null)
            {
                return;
            }

            var result = BrowseIcon(SelectedBag.Bag.ItemIcon?.Custom ?? false);

            if (result == null)
            {
                return;
            }

            SelectedBag.Bag.ItemIcon ??= new Configuration.Icon();
            SelectedBag.Bag.ItemIcon.Name = result;

            SelectedBag.Update();
        }

        private void BrowseToolTipIcon(object sender, RoutedEventArgs e)
        {
            if (SelectedBag == null)
            {
                return;
            }

            var result = BrowseIcon(SelectedBag.Bag.TooltipIcon?.Custom ?? false);

            if (result == null)
            {
                return;
            }

            SelectedBag.Bag.TooltipIcon ??= new Configuration.Icon();
            SelectedBag.Bag.TooltipIcon.Name = result;

            SelectedBag.Update();
        }

        private string BrowseIcon(bool custom)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetFullPath(
                    custom
                        ? Constants.CONTENT_CUSTOM_PATH
                        : Constants.ICONS_OUTPUT_PATH
                ),
                Filter = "PNG (*.png)|*.png"
            };

            var result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                return Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            }

            return null;
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
