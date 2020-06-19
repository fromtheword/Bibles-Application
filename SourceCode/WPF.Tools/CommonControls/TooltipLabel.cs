using System.Windows;
using System.Windows.Forms;

namespace WPF.Tools.CommonControls
{
    public class TooltipLabel : TextBoxItem
    {
        public TooltipLabel()
        {
            this.BorderThickness = new Thickness(0);
        }

        new public string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                base.Text = value;

                if (base.TextLenght > Screen.PrimaryScreen.Bounds.Width)
                {
                    this.TextWrapping = System.Windows.TextWrapping.Wrap;

                    this.MaxWidth = Screen.PrimaryScreen.Bounds.Width - 200;

                    this.Width = Screen.PrimaryScreen.Bounds.Width - 200;
                }
            }
        }
    }
}
