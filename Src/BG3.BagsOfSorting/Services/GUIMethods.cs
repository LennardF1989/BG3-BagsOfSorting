using BG3.BagsOfSorting.Models;

namespace BG3.BagsOfSorting.Services
{
    public static class GUIMethods
    {
        public static bool ExportAtlasIcons(out List<string> log)
        {
            try
            {
                CLIMethods.ExportAtlasIcons();

                log = CLIMethods.GetLog();

                return true;
            }
            catch
            {
                log = CLIMethods.GetLog();

                return false;
            }
        }

        public static bool GenerateBags(BagConfiguration bagConfiguration, out List<string> log)
        {
            try
            {
                CLIMethods.SaveConfiguration(bagConfiguration);

                CLIMethods.GenerateBags();

                log = CLIMethods.GetLog();

                return true;
            }
            catch
            {
                log = CLIMethods.GetLog();

                return false;
            }
        }
    }
}
