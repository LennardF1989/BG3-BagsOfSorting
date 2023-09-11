using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using BG3.BagsOfSorting.Models;

namespace BG3.BagsOfSorting.Views.UserControls
{
    public partial class GameObjectControl : INotifyPropertyChanged
    {
        public static event Action<string> AddItemToTreasureTable;

        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty GameObjectProperty = 
            DependencyProperty.Register(
                nameof(GameObject), 
                typeof(SearchIndex.GameObject), 
                typeof(GameObjectControl),
                new FrameworkPropertyMetadata(
                    null, 
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (dependencyObject, args) =>
                    {
                        ((GameObjectControl)dependencyObject).SelectedGameObject = (SearchIndex.GameObject)args.NewValue;
                    }
                )
            );

        public SearchIndex.GameObject GameObject
        {
            get => (SearchIndex.GameObject)GetValue(GameObjectProperty);
            set => SetValue(GameObjectProperty, value);
        }

        public SearchIndex.GameObject SelectedGameObject
        {
            get => _gameObject;
            set => SetField(ref _gameObject, value);
        }

        private static readonly Dictionary<SearchIndex.GameObject, Window> _windows = new();

        private SearchIndex.GameObject _gameObject;

        public GameObjectControl()
        {
            InitializeComponent();
        }

        private void BrowseToGameObject(object sender, RoutedEventArgs e)
        {
            var gameObject = (SearchIndex.GameObject)((FrameworkElement)sender).DataContext;

            if (_windows.TryGetValue(gameObject, out var window))
            {
                //HACK: Force window to front
                window.Topmost = true;
                window.Topmost = false;

                return;
            }

            var userControl = new GameObjectControl
            {
                Margin = new Thickness(5),
                GameObject = gameObject
            };

            //NOTE: DataContext has to be set to self!
            userControl.DataContext = userControl;
            
            window = new Window
            {
                Title = gameObject.Name,
                Content = userControl,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = 480,
                Height = 480
            };

            window.Closed += (_, _) =>
            {
                _windows.Remove(gameObject);
            };

            window.Show();

            _windows[gameObject] = window;
        }

        private void OnAddToTreasureTable(object sender, RoutedEventArgs e)
        {
            var gameObject = (SearchIndex.GameObject)((FrameworkElement)sender).DataContext;

            if (gameObject == null)
            {
                return;
            }

            AddItemToTreasureTable?.Invoke(gameObject.Stats ?? gameObject.Name);
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
