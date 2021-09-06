using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Setting
{
    /// <summary>
    /// voicevoxPropertySlider.xaml の相互作用ロジック
    /// </summary>
    public partial class VoicevoxPropertySlider : UserControl
    {
        public VoicevoxPropertySlider()
        {
            InitializeComponent();
        }

        private void TextBlock_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            if (textBlock.IsEnabled)
            {
                textBlock.Foreground = Brushes.Black;
            }
            else
            {
                textBlock.Foreground = Brushes.Gray;
            }
        }
    }
}
