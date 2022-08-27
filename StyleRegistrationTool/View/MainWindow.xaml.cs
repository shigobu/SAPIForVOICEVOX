using SFVvCommon;
using StyleRegistrationTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            viewModel.VoicevoxStyle_SelectedItems = VoicevoxStyleList.SelectedItems.Cast<VoicevoxStyle>();
        }

        private void SapiStyleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SapiStyle_SelectedItems = SapiStyleList.SelectedItems.Cast<SapiStyle>();
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader columnHeader = (GridViewColumnHeader)sender;
            string columnTag = columnHeader.Tag.ToString();
            string columnHeaderString = columnHeader.Content.ToString();

            bool isAscending = true;
            if (columnHeaderString.Contains("▼"))
            {
                isAscending = false;
            }

            IEnumerable<SapiStyle> sortedList;
            switch (columnTag)
            {
                case nameof(SapiStyle.AppName):
                    sortedList = isAscending ? viewModel.SapiStyles.OrderBy(x => x.AppName) : viewModel.SapiStyles.OrderByDescending(x => x.AppName);
                    break;
                case nameof(SapiStyle.Name):
                    sortedList = isAscending ? viewModel.SapiStyles.OrderBy(x => x.Name, new StyleComparer()) : viewModel.SapiStyles.OrderByDescending(x => x.Name, new StyleComparer());
                    break;
                case nameof(SapiStyle.StyleName):
                    sortedList = isAscending ? viewModel.SapiStyles.OrderBy(x => x.StyleName) : viewModel.SapiStyles.OrderByDescending(x => x.StyleName);
                    break;
                case nameof(SapiStyle.ID):
                    sortedList = isAscending ? viewModel.SapiStyles.OrderBy(x => x.ID) : viewModel.SapiStyles.OrderByDescending(x => x.ID);
                    break;
                case nameof(SapiStyle.Port):
                    sortedList = isAscending ? viewModel.SapiStyles.OrderBy(x => x.Port) : viewModel.SapiStyles.OrderByDescending(x => x.Port);
                    break;
                default:
                    return;
            }

            if (columnHeaderString.Contains("▼"))
            {
                columnHeaderString = columnHeaderString.Replace("▼", "▲");
            }
            else if (columnHeaderString.Contains("▲"))
            {
                columnHeaderString = columnHeaderString.Replace("▲", "▼");
            }
            else
            {
                if (isAscending)
                {
                    columnHeaderString += "▼";
                }
                else
                {
                    columnHeaderString += "▲";
                }
                columnHeader.Width += 10;
            }
            columnHeader.Content = columnHeaderString;
            //自分以外のヘッダーから▼マークを削除
            List<GridViewColumnHeader> columnHeaders = new List<GridViewColumnHeader>() { AppNameHeader, NameHeader, StyleNameHeader, IDHeader, PortHeader };
            columnHeaders.Remove(columnHeader);
            foreach (var item in columnHeaders)
            {
                string headerString = item.Content.ToString();
                headerString = headerString.Replace("▲", "");
                headerString = headerString.Replace("▼", "");
                item.Content = headerString;
            }

            viewModel.SapiStyles = new ObservableCollection<SapiStyle>(sortedList);
        }
    }
}
