using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Windows;
using BG3.BagsOfSorting.Models;
using BG3.BagsOfSorting.Services;
using BG3.BagsOfSorting.ViewModels;
using Microsoft.Win32;

namespace BG3.BagsOfSorting.Views
{
    // ReSharper disable once UnusedMember.Global
    public partial class MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<BagViewModel> Bags { get; set; }
        public ObservableCollection<AdditionalTreasureViewModel> AdditionalTreasures { get; set; }
        public bool AlignGeneratedItemIconsRight { get; set; }
        public ObservableCollection<string> BagColors { get; set; }
        public ObservableCollection<string> TreasureTypes { get; set; }

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

        public bool IsBagSelected => SelectedBag != null;
        public bool IsAdditionalTreasureSelected => SelectedAdditionalTreasure != null;

        private BagViewModel _selectedBag;
        private AdditionalTreasureViewModel _selectedAdditionalTreasure;

        public MainWindow()
        {
            var bagConfiguration = CLIMethods.LoadConfiguration();

            Bags = new ObservableCollection<BagViewModel>();
            bagConfiguration.Bags.ForEach(x =>
            {
                x.Name = CLIMethods.GetNameForBag(x);

                Bags.Add(new BagViewModel
                {
                    Bag = x
                });
            });

            AdditionalTreasures = new ObservableCollection<AdditionalTreasureViewModel>();

            AlignGeneratedItemIconsRight = bagConfiguration.AlignGeneratedItemIconsRight;

            BagColors = new ObservableCollection<string>(Enum.GetNames(typeof(BagConfiguration.EColor)));

            TreasureTypes = new ObservableCollection<string>(Enum.GetNames(typeof(AdditionalTreasureViewModel.EType)));

            InitializeComponent();

            BagsDataGrid.AutoGeneratingColumn += (_, args) =>
            {
                if (args.PropertyName == nameof(BagViewModel.Bag))
                {
                    args.Cancel = true;
                }
            };

            Closing += (_, _) =>
            {
                var result = MessageBox.Show(
                    this,
                    "Do you want to save your bags before closing the application?",
                    "Save Bags",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) == MessageBoxResult.Yes;

                if (result)
                {
                    CLIMethods.SaveConfiguration(GetBagConfiguration());
                }
            };
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
                this, 
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

        private void OpenApplicationDirectory(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", Path.GetFullPath(Directory.GetCurrentDirectory()));
        }

        private void GenerateBags(object sender, RoutedEventArgs e)
        {
            _selectedBag?.Update();

            var result = MessageBox.Show(
                this,
                "Generating bags will save your current configuration, overwriting the old one. Is that okay?",
                "Generate Bags",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) == MessageBoxResult.Yes;

            if (!result)
            {
                return;
            }

            result = GUIMethods.GenerateBags(GetBagConfiguration(), out var logs);

            var logText = string.Empty;

            if (logs != null && logs.Any())
            {
                logText = $"\r\n\r\nThe following was reported:\r\n{string.Join("\r\n", logs.Select(x => $"- {x}"))}";
            }

            if (result)
            {
                MessageBox.Show(
                    this,
                    $"Bags successfully generated!{logText}",
                    "Generate Bags",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                MessageBox.Show(
                    this,
                    $"Bags failed to generate!{logText}",
                    "Generate Bags",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ExportIcons(object sender, RoutedEventArgs e)
        {
            var result = GUIMethods.ExportAtlasIcons(out var logs);

            var logText = string.Empty;

            if (logs != null && logs.Any())
            {
                logText = $"\r\n\r\nThe following was reported:\r\n{string.Join("\r\n", logs.Select(x => $"- {x}"))}";
            }

            if (result)
            {
                MessageBox.Show(
                    this, 
                    $"Icons successfully exported!{logText}", 
                    "Export Icons",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                MessageBox.Show(
                    this,
                    $"Icons failed to export!{logText}",
                    "Export Icons",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
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

            SelectedBag.Bag.ItemIcon ??= new BagConfiguration.Icon();
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

            SelectedBag.Bag.TooltipIcon ??= new BagConfiguration.Icon();
            SelectedBag.Bag.TooltipIcon.Name = result;

            SelectedBag.Update();
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
                this,
                $"Are you sure you want to remove '{SelectedAdditionalTreasure.Name}'?",
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

        private BagConfiguration GetBagConfiguration()
        {
            var additionalTreasures = new JsonObject();
            foreach (var additionalTreasure in AdditionalTreasures)
            {
                additionalTreasures[
                    $"{(additionalTreasure.Type == AdditionalTreasureViewModel.EType.Item ? "I_" : "T_")}_{additionalTreasure.Name}"
                ] = additionalTreasure.Amount;
            }

            var bagConfiguration = new BagConfiguration
            {
                Bags = Bags.Select(x => x.Bag).ToList(),
                AdditionalTreasures = additionalTreasures,
                AlignGeneratedItemIconsRight = AlignGeneratedItemIconsRight
            };
            return bagConfiguration;
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
