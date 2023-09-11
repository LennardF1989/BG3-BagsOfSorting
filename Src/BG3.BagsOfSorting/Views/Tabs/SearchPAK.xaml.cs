using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using BG3.BagsOfSorting.Models;
using BG3.BagsOfSorting.Services;
using BG3.BagsOfSorting.ViewModels;

namespace BG3.BagsOfSorting.Views.Tabs
{
    public partial class SearchPAK : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Query { get; set; }
        public EnumFlagsViewModel<SearchIndexer.SearchIndexFilter> SearchIndexFilter { get; set; }
        public ObservableCollection<string> PAKPaths { get; set; }
        public ObservableCollection<SearchIndex.GameObject> GameObjects { get; set; }

        public SearchIndex.GameObject SelectedGameObject
        {
            get => _selectedGameObject;
            set => SetField(ref _selectedGameObject, value);
        }

        public string SelectedPAKPath
        {
            get => _selectedPAKPath;
            set => SetField(ref _selectedPAKPath, value);
        }

        private string _selectedPAKPath;
        private SearchIndex.GameObject _selectedGameObject;

        public SearchPAK()
        {
            var configuration = ServiceLocator.Configuration;

            SearchIndexFilter = new EnumFlagsViewModel<SearchIndexer.SearchIndexFilter>(SearchIndexer.SearchIndexFilter.All);

            PAKPaths = new ObservableCollection<string>(configuration.PAKPaths ?? new List<string>());

            InitializeComponent();
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            var searchIndex = ServiceLocator.SearchIndex;

            if (searchIndex == null)
            {
                return;
            }

            var gameObjects = SearchIndexer
                .Search(searchIndex, Query, SearchIndexFilter.Value)
                .OrderBy(x => x.ResolvedName);

            GameObjects = new ObservableCollection<SearchIndex.GameObject>(gameObjects);

            OnPropertyChanged(nameof(GameObjects));
        }

        private void AddPAKPath(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();

            var result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                PAKPaths.Add(openFileDialog.SelectedPath);
            }
        }

        private void RemovePAKPath(object sender, RoutedEventArgs e)
        {
            if (SelectedPAKPath == null)
            {
                return;
            }

            PAKPaths.Remove(SelectedPAKPath);
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
