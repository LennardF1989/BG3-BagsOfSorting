namespace BG3.BagsOfSorting.Models
{
    public class Context
    {
        public Configuration Configuration { get; set; }
        public List<string> Messages { get; init; }

        public Context()
        {
            Configuration = ServiceLocator.Configuration;
            Messages = new List<string>();
        }

        public void LogMessage(string message)
        {
            Messages.Add(message);
        }
    }
}