using System.IO;
using System.Runtime.CompilerServices;
using BG3.BagsOfSorting.Services;

namespace BG3.BagsOfSorting
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var command = args.FirstOrDefault();

            switch (command)
            {
                case "--export-atlas-icons":
                    CLIMethods.ExportAtlasIcons();
                    WriteLogFile(CLIMethods.GetLog());
                    break;
                case "--generate-bags":
                    CLIMethods.GenerateBags();
                    WriteLogFile(CLIMethods.GetLog());
                    break;
                case "--add-bag":
                    CLIMethods.AddBag();
                    WriteLogFile(CLIMethods.GetLog());
                    break;
                default:
                    RunApp();
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RunApp()
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static void WriteLogFile(List<string> logs)
        {
            if (logs == null || !logs.Any())
            {
                return;
            }

            File.WriteAllText("log.txt", string.Join("\r\n", logs));
        }
    }
}
