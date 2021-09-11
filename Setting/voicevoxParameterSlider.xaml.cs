using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Setting
{
    /// <summary>
    /// voicevoxPropertySlider.xaml の相互作用ロジック
    /// </summary>
    public partial class VoicevoxParameterSlider : UserControl
    {
        public VoicevoxParameterSlider()
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
