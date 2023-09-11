using BG3.BagsOfSorting.Models;
using BG3.BagsOfSorting.Services;

namespace BG3.BagsOfSorting
{
    public static class ServiceLocator
    {
        public static Configuration Configuration { get; private set; }
        public static SearchIndex SearchIndex { get; internal set; }

        public static void Initialize()
        {
            UpdateConfiguration(CLIMethods.LoadConfiguration());

            SearchIndex = CLIMethods.LoadSearchIndex();

            CLIMethods.ReleaseMemory();
        }

        public static void UpdateConfiguration(Configuration configuration)
        {
            Configuration = configuration;
        }
    }
}
