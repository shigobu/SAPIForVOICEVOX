using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace StyleRegistrationTool.View
{
    /// <summary>
    /// ChangePortWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ChangePortWindow : Window
    {
        /// <summary>
        /// ポート番号
        /// </summary>
        public int Port
        {
            get
            {
                string temp = portComboBox.Text;
                return int.Parse(ExtractNumber(temp));
            }
            set
            {
                portComboBox.Text = value.ToString();
            }
        }

        public ChangePortWindow(int port)
        {
            InitializeComponent();

            Port = port;
        }

        /// <summary>
        /// 与えられた文字列から、数字列を抽出し最初の数字列を返します。
        /// </summary>
        /// <param name="str">対象の文字列</param>
        /// <returns>抽出した数字列</returns>
        private string ExtractNumber(string str)
        {
            Regex regex = new Regex(@"[0-9]+", RegexOptions.IgnoreCase);
            IEnumerable<Match> matchCollection = regex.Matches(str).Cast<Match>();
            IEnumerable<string> numberWords = matchCollection.Select(match => match.Value);
            if (numberWords.Count() == 0)
            {
                return "";
            }
            return numberWords.First();
        }

        /// <summary>
        /// OKボタン押下イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string temp = portComboBox.Text;
            if (int.TryParse(ExtractNumber(temp), out int value))
            {
                DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("数値変換できません。", "スタイル登録ツール", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
