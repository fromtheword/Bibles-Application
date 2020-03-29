using System;
using WPF.Tools.BaseClasses;

namespace Bibles
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControlBase
    {
        public About()
        {
            this.InitializeComponent();

            this.uxDisclaimer.Text = this.DisclaimerText();

            this.uxCompywrite.Text = this.CopywriteText();
        }

        private string DisclaimerText()
        {
            return "This product is licenced under the ‘BSD 3-Clause License’." + Environment.NewLine + Environment.NewLine +
                    "The software is free of charge and may be distributed at free will, " + Environment.NewLine +
                    "if you agree that no changes will be made to the Bible content or the default Bible studies, " + Environment.NewLine +
                    "or of the availability thereof in the application." + Environment.NewLine + Environment.NewLine +
                    "The source code my be used under the same conditions" + Environment.NewLine + Environment.NewLine +
                    "Contact us at: fromtheWord.info@gmail.com";
        }

        private string CopywriteText()
        {
            return "Revelation 22: 18 - 19" + Environment.NewLine + Environment.NewLine +
                    "18.    For I testify unto every man that heareth the words of the prophecy of this book, " + Environment.NewLine + 
                    "If any man shall add unto these things, God shall add unto him the plagues that are written in this book:" + Environment.NewLine + Environment.NewLine +
                    "19.    And if any man shall take away from the words of the book of this prophecy, God shall take away his part " + Environment.NewLine + 
                    "out of the book of life, and out of the holy city, and from the things which are written in this book.";
        }
    }
}
