using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Windows;
using BG3.BagsOfSorting.Models;
using BG3.BagsOfSorting.Services;
using BG3.BagsOfSorting.ViewModels;
using BG3.BagsOfSorting.Views.UserControls;
using MessageBox = System.Windows.MessageBox;

namespace BG3.BagsOfSorting.Views
{
    // ReSharper disable once UnusedMember.Global
    public partial class MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version!;

            Title += $" v{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

            GameObjectControl.AddItemToTreasureTable += itemName =>
            {
                var result = MessageBox.Show(
                    this,
                    $"Are you sure you want to add '{itemName}' to your Treasure Table?",
                    "Add Item to Treasure Table",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) == MessageBoxResult.Yes;

                if (result)
                {
                    TreasureTableTab.AdditionalTreasures.Add(new AdditionalTreasureViewModel
                    {
                        Type = AdditionalTreasureViewModel.EType.Item,
                        DisplayName = itemName,
                        Amount = 1
                    });
                }
            };

            Closing += (_, _) =>
            {
                var result = MessageBox.Show(
                    this,
                    "Do you want to save your configuration before closing the application?",
                    "Save Configuration",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) == MessageBoxResult.Yes;

                if (result)
                {
                    CLIMethods.SaveConfiguration(GetConfiguration());
                }
            };
        }

        private void OpenContentDirectory(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "Content")
            ));
        }

        private void OpenOutputDirectory(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "Output")
            ));
        }

        private void OpenModsDirectory(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", Path.GetFullPath(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "Larian Studios", "Baldur's Gate 3", "Mods"
                )
            ));
        }

        private void GeneratePAK(object sender, RoutedEventArgs e)
        {
            BagsTab.Update();
            TreasureTableTab.Update();

            var result = MessageBox.Show(
                this,
                "Generating a PAK will save your current configuration, overwriting the old one. Do you want to continue?",
                "Generate PAK",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) == MessageBoxResult.Yes;

            if (!result)
            {
                return;
            }

            result = GUIMethods.GeneratePAK(GetConfiguration(), out var logs);

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
            var messageBoxResult = MessageBox.Show(
                this,
                "Exporting icons may take a while, consume 1GB of memory and it will cause the application to freeze in the meantime. Do you want to continue?",
                "Export Icons",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }

            var result = GUIMethods.ExportAtlasIcons(GetConfiguration(), out var logs);

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

        private void IndexPAK(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show(
                this,
                "Index PAKs may take a while, consume 1GB of memory and it will cause the application to freeze in the meantime. If an index had been generated before, it will be overwritten. Do you want to continue?",
                "Index PAKs",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }

            var searchIndex = GUIMethods.IndexPAK(GetConfiguration(), out var logs);

            var logText = string.Empty;

            if (logs != null && logs.Any())
            {
                logText = $"\r\n\r\nThe following was reported:\r\n{string.Join("\r\n", logs.Select(x => $"- {x}"))}";
            }

            if (searchIndex == null)
            {
                MessageBox.Show(
                    this,
                    $"Failed to index PAKs!{logText}",
                    "Index PAKs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return;
            }

            MessageBox.Show(
                this,
                $"PAKs successfully indexed!{logText}",
                "Index PAKs",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            ServiceLocator.SearchIndex = searchIndex;
        }

        private Configuration GetConfiguration()
        {
            var additionalTreasures = new JsonObject();

            foreach (var additionalTreasure in TreasureTableTab.AdditionalTreasures)
            {
                additionalTreasures[additionalTreasure.Name] = additionalTreasure.Amount;
            }

            var bagConfiguration = new Configuration
            {
                Bags = BagsTab.Bags.Select(x => x.Bag).ToList(),
                AdditionalTreasures = additionalTreasures,
                AlignGeneratedItemIconsRight = BagsTab.AlignGeneratedItemIconsRight,
                BundlePouchOfWonders = BagsTab.BundlePouchOfWonders,
                TreasureTable = new Configuration.TreasureTableData
                {
                    FolderName = TreasureTableTab.TreasureTableFolderName,
                    Name = TreasureTableTab.TreasureTableName
                },
                PAKPaths = SearchPAKTab.PAKPaths.ToList()
            };

            return bagConfiguration;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ReSharper disable once UnusedMember.Global
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
