using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BG3.BagsOfSorting.Models;
using BG3.BagsOfSorting.Services;

namespace BG3.BagsOfSorting
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            
            ServiceLocator.Initialize();
            
            var command = args.FirstOrDefault();

            var context = new Context();

            switch (command)
            {
                case "--export-atlas-icons":
                    CLIMethods.ExportAtlasIcons(context);
                    break;
                case "--generate-bags": //NOTE: Backwards compatibility
                case "--generate-pak":
                    CLIMethods.GeneratePAK(context);
                    break;
                case "--add-bag":
                    CLIMethods.AddBag();
                    break;
                case "--index-pak":
                    CLIMethods.IndexPAK(context);
                    break;
                case "--search-pak":
                    CLIMethods
                        .SearchPAK(string.Join(" ", args.Skip(1)))
                        .Select(x => JsonSerializer.Serialize(x))
                        .ToList()
                        .ForEach(context.LogMessage);
                    break;
                default:
                    RunApp();
                    break;
            }

            WriteLogFile(context.Messages);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RunApp()
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static void WriteLogFile(IReadOnlyCollection<string> logs)
        {
            if (logs == null || !logs.Any())
            {
                return;
            }

            File.WriteAllText("log.txt", string.Join("\r\n", logs));
        }
    }
}
