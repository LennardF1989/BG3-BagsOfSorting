#pragma warning disable S101

using BG3.BagsOfSorting.Models;

namespace BG3.BagsOfSorting.Services
{
    public static class GUIMethods
    {
        public static bool ExportAtlasIcons(Configuration configuration, out List<string> log)
        {
            var context = new Context();
            log = context.Messages;

            try
            {
                context.Configuration = configuration;

                CLIMethods.ExportAtlasIcons(context);

                return true;
            }
            catch(Exception ex)
            {
                context.LogMessage($"[Fatal Error] Unhandled exception: {ex}");

                return false;
            }
        }

        public static SearchIndex IndexPAK(Configuration configuration, out List<string> log)
        {
            var context = new Context();
            log = context.Messages;

            try
            {
                context.Configuration = configuration;

                var searchIndex = CLIMethods.IndexPAK(context);

                return searchIndex;
            }
            catch (Exception ex)
            {
                context.LogMessage($"[Fatal Error] Unhandled exception: {ex}");

                return null;
            }
        }

        public static bool GeneratePAK(Configuration configuration, out List<string> log)
        {
            var context = new Context();
            log = context.Messages;

            try
            {
                //NOTE: Make sure to create a copy of the configuration, so it can be freely manipulated.
                CLIMethods.SaveConfiguration(configuration);

                context.Configuration = CLIMethods.LoadConfiguration();

                CLIMethods.GeneratePAK(context);

                return true;
            }
            catch (Exception ex)
            {
                context.LogMessage($"[Fatal Error] Unhandled exception: {ex}");

                return false;
            }
        }
    }
}
