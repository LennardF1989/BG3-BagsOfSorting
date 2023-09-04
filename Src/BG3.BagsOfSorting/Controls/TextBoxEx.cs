using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BG3.BagsOfSorting.Controls
{
    public class TextBoxEx : TextBox
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
    }
}
