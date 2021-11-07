using StyleRegistrationTool.Model;
using StyleRegistrationTool.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StyleRegistrationTool
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

#if x64
            string bitStr = "64bit版";
#else
            string bitStr = "32bit版";
#endif
            this.Title += bitStr;

            viewModel = new MainViewModel(this);
            this.DataContext = viewModel;
            this.Loaded += viewModel.MainWindow_Loaded;
        }

        /// <summary>
        /// ウィンドウハンドルを取得します。
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                return helper.Handle;
            }
        }

        private void VoicevoxStyleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.VoicevoxStyle_SelectedItems = VoicevoxStyleList.SelectedItems.Cast<SFVvCommon.VoicevoxStyle>();
        }

        private void SapiStyleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SapiStyle_SelectedItems = SapiStyleList.SelectedItems.Cast<SapiStyle>();
        }
    }
}
