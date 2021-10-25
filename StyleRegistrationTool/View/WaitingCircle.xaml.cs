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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StyleRegistrationTool.View
{
    /// <summary>
    /// WaitingCircle.xaml の相互作用ロジック
    /// </summary>
    public partial class WaitingCircle : UserControl
    {
        //https://araramistudio.jimdo.com/2016/11/24/wpf%E3%81%A7waitingcircle%E3%82%B3%E3%83%B3%E3%83%88%E3%83%AD%E3%83%BC%E3%83%AB%E3%82%92%E4%BD%9C%E3%82%8B/
        //から引用

        public static readonly DependencyProperty CircleColorProperty =
        DependencyProperty.Register(
        "CircleColor", // プロパティ名を指定
                    typeof(Color), // プロパティの型を指定
                    typeof(WaitingCircle), // プロパティを所有する型を指定
                    new UIPropertyMetadata(Color.FromRgb(90, 117, 153),
        (d, e) => { (d as WaitingCircle).OnCircleColorPropertyChanged(e); }));
        public Color CircleColor
        {
            get { return (Color)GetValue(CircleColorProperty); }
            set { SetValue(CircleColorProperty, value); }
        }


        public WaitingCircle()
        {
            InitializeComponent();

            double cx = 50.0;
            double cy = 50.0;
            double r = 45.0;
            int cnt = 14;
            double deg = 360.0 / (double)cnt;
            double degS = deg * 0.2;
            for (int i = 0; i < cnt; ++i)
            {
                var si1 = Math.Sin((270.0 - (double)i * deg) / 180.0 * Math.PI);
                var co1 = Math.Cos((270.0 - (double)i * deg) / 180.0 * Math.PI);
                var si2 = Math.Sin((270.0 - (double)(i + 1) * deg + degS) / 180.0 * Math.PI);
                var co2 = Math.Cos((270.0 - (double)(i + 1) * deg + degS) / 180.0 * Math.PI);
                var x1 = r * co1 + cx;
                var y1 = r * si1 + cy;
                var x2 = r * co2 + cx;
                var y2 = r * si2 + cy;

                var path = new Path();
                path.Data = Geometry.Parse(string.Format("M {0},{1} A {2},{2} 0 0 0 {3},{4}", x1, y1, r, x2, y2));
                path.Stroke = new SolidColorBrush(Color.FromArgb((byte)(255 - (i * 256 / cnt)), CircleColor.R, CircleColor.G, CircleColor.B));
                path.StrokeThickness = 10.0;
                MainCanvas.Children.Add(path);
            }

            var kf = new DoubleAnimationUsingKeyFrames();
            kf.RepeatBehavior = RepeatBehavior.Forever;
            for (int i = 0; i < cnt; ++i)
            {
                kf.KeyFrames.Add(new DiscreteDoubleKeyFrame()
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(i * 80)),
                    Value = i * deg
                });
            }
            MainTrans.BeginAnimation(RotateTransform.AngleProperty, kf);
        }

        public void OnCircleColorPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (null == MainCanvas) return;
            if (null == MainCanvas.Children) return;

            foreach (var child in MainCanvas.Children)
            {
                var shp = child as Shape;
                var sb = shp.Stroke as SolidColorBrush;
                var a = sb.Color.A;
                shp.Stroke = new SolidColorBrush(Color.FromArgb(a, CircleColor.R, CircleColor.G, CircleColor.B));
            }
        }
    }
}
