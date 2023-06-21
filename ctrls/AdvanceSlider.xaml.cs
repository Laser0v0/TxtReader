using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace TxtReader
{
    /// <summary>
    /// AdvanceSlider.xaml 的交互逻辑
    /// </summary>
    public partial class AdvanceSlider : UserControl
    {
        public double Min
        {
            set { SetValue(MinProperty, value); }
            get { return (double)GetValue(MinProperty); }
        }
        public double Max
        {
            set { SetValue(MaxProperty, value); }
            get { return (double)GetValue(MaxProperty); }
        }


        [Bindable(true)]
        public double Value
        {
            set { SetValue(ValueProperty, value); }
            get { return (double)GetValue(ValueProperty); }
        }

        public string Title 
        {
            set { SetValue(TitleProperty, value); }
            get { return (string)GetValue(TitleProperty); }
        }
        
        public double TitleWidth { set; get; }

        public string Unit
        {
            set { SetValue(UnitProperty, value); }
            get { return (string)GetValue(UnitProperty); }
        }
        
        public bool IsSnapToTickEnabled { get; set; }
        public Visibility ShowUnit { set; get; }
        public double UnitWidth { set; get; }



        // 注册Title
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Control),
                new PropertyMetadata(""));

        // 注册Unit
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(Control),
                new PropertyMetadata(""));


        // 注册Value
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Control),
                new PropertyMetadata(0.0));

        // 注册最大值
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(double), typeof(Control),
                new PropertyMetadata(0.0));

        // 注册最小值
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(double), typeof(Control),
                new PropertyMetadata(0.0));


        public AdvanceSlider()
        {
            TitleWidth = 25;
            InitializeComponent();
            DataContext = this;
            IsSnapToTickEnabled = true;
        }

        public void setProperty(int Min, int Max, string Title, string Unit, int Value)
        {
            this.Min = Min;
            this.Max = Max;
            this.Title = Title;
            this.Unit = Unit;
            this.Value = Value;
        }

        #region 绑定动作
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, 
                typeof(RoutedEventHandler), typeof(AdvanceSlider));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RoutedEventArgs arg = new RoutedEventArgs();
            arg.RoutedEvent = ValueChangedEvent;
            RaiseEvent(arg);
        }
        #endregion

        public string ToString()
        {
            return Value.ToString();
        }


        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            slider.Value += btn.Content.ToString() == "+" ? 1 : -1;
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            TextBox tb = sender as TextBox;
            double v;
            if(!double.TryParse(tb.Text, out v))
            {
                tb.Text = Value.ToString();
                return;
            }

            if (v > Max)
                v = Max;
            else if(v< Min)
                v = Min;
            slider.Value = v;
        }

        public void limitNumber(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex(@"[^0-9\-]");
            e.Handled = re.IsMatch(e.Text);
        }
    }
}
