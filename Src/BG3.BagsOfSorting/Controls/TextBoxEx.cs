using System.Windows;
using System.Windows.Data;

namespace BG3.BagsOfSorting.Controls
{
    public class TextBoxEx : System.Windows.Controls.TextBox
    {
        static TextBoxEx()
        {
            TextProperty.OverrideMetadata(
                typeof(TextBoxEx),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null,
                    null, 
                    true, 
                    UpdateSourceTrigger.PropertyChanged
                )
            );
        }

        public TextBoxEx()
        {
            PreviewMouseDoubleClick += (_, _) =>
            {
                SelectAll();
            };
        }
    }
}
