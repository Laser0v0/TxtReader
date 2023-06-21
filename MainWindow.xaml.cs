using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Speech.Synthesis;
using System.Globalization;
using UtfUnknown;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace TxtReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum CodeMode { combo, auto };
        enum SecMode { page, section, secpage };

        //public Catalog catalog;
        public Book book;
        public Dictionary<string, Book> books = new Dictionary<string, Book>();
        public Dictionary<string, TextBox> txtBooks = new Dictionary<string, TextBox>();
        public Dictionary<string, TabItem> tabBooks = new Dictionary<string, TabItem>();

        BookShelf bookShelf;
        public string filePath = "";
        public string doc = "";
        MySpeech speech = new MySpeech("");
        Encoding nowEncoding = Encoding.UTF8;
        const string TERMINATOR = ".!?。！？”";
        const string FMT_CFG = "配置文件(*.json)|*.json";

        int nJumpChars = 20;
        int nStartLoad = 20;

        SecMode secMode = SecMode.section;

        public MainWindow()
        {
            InitializeComponent();
            init();
            initParaDct();
            initContextMenu();
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            Closed += MainWindow_Closed;
        }

        private void saveFile(string path, CodeMode cm = CodeMode.auto)
        {
            Encoding e = cm == CodeMode.combo
                ? Encoding.GetEncoding(cbEncoding.SelectedItem.ToString())
                : nowEncoding;

            txtInfo.AppendText($"存储文件【{System.IO.Path.GetFileName(path)}】\r\n" +
                $"编码为：{nowEncoding.EncodingName}\r\n");
            File.WriteAllText(path, book.output(), e);
        }

        #region 目录相关
        // 生成目录
        private void setCatalog()
        {
            lvCatalog.SelectionChanged -= lvCatalog_SelectionChanged;
            lvCatalog.Items.Clear();
            lvCatalog.SelectionChanged += lvCatalog_SelectionChanged;
            var sec = secMode == SecMode.page ? book.pages : book.secs;
            foreach (var item in sec)
                lvCatalog.Items.Add(item.title);
        }

        // 更改目录
        private void lvCatalog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sec = secMode == SecMode.page ? book.pages : book.secs;
            int i = Math.Min(lvCatalog.SelectedIndex, sec.Count - 1);
            if (i < 0)
                return;
            txtChange(sec[i].content);
            tbNow.ScrollToHome();
        }
        #endregion

        #region 书柜设置
        private void loadFolder()
        {
            tvBookShelf.Items.Clear();
            TreeViewItem root = new TreeViewItem { Header = bookShelf.name };
            tvBookShelf.Items.Add(root);
            initTreeView(bookShelf, root);
        }


        // 生成书柜的树形图
        private void initTreeView(BookShelf bs, TreeViewItem root)
        {
            foreach (var item in bs.childs)
            {
                var mi = new TreeViewItem() { 
                    Header = item.name};
                root.Items.Add(mi);
                initTreeView(item, mi);
            }
            foreach (var b in bs.books)
            {
                root.Items.Add(
                    new TreeViewItem { Header = b, Tag=bs, ContextMenu=cmTVI });
            }
        }

        // 选择书籍
        private void tvBookShelf_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = tvBookShelf.SelectedItem as TreeViewItem;
            if (item == null)
                return;
            BookShelf bs = item.Tag as BookShelf;
            if (bs == null)
                return;
            filePath = System.IO.Path.Combine(bs.root, (string)item.Header);
            openFile();
        }
        #endregion

        #region tab控制
        TextBox tbNow;

        private void openFile(CodeMode cm = CodeMode.auto)
        {
            if (filePath == "" || !File.Exists(filePath))
                return;
            try
            {
                nowEncoding = cm == CodeMode.combo
                    ? Encoding.GetEncoding(cbEncoding.SelectedItem.ToString())
                    : CharsetDetector.DetectFromFile(filePath).Detected.Encoding;
            }
            catch (Exception ex)
            {
                txtInfo.AppendText($"打开失败，原因为{ex}");
            }

            tbStatusEncoding.Text = nowEncoding.EncodingName;

            createTextbox(filePath);

            if (book == null)
                book = new Book(filePath, nowEncoding);
            else
                book.init(File.ReadAllText(filePath, nowEncoding));
            txtChange(book.output(0, nStartLoad));
            setCatalog();
        }

        private void createTextbox(string filePath)
        {
            if (filePath == "" || !File.Exists(filePath))
                return;


            TabItem? ti;
            Button? btn;
            if (tbBooks.Items.Count == 1)
            {
                ti = tbBooks.Items[0] as TabItem;
                try
                {
                    if (ti.Header.ToString() == "无文件")
                        tbBooks.Items.Clear();
                }
                catch {
                    return;
                
                }
            }

            string name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            foreach (var item in tbBooks.Items) 
            { 
                ti = item as TabItem;
                btn = ti.Header as Button;
                if (btn.Content.ToString() == name)
                {
                    tbBooks.SelectedItem = ti;
                    tbNow = ti.Content as TextBox;
                    book = books[name];
                    //ti.IsSelected = true;
                    return;
                }
            }

            
            ti = new TabItem { AllowDrop = true };
            tbBooks.Items.Add(ti);
            tbBooks.SelectedItem = ti;

            btn = new Button { Content = name, Margin=new Thickness(0), BorderThickness = new Thickness(0)};
            var cvt = new BrushConverter();
            btn.Background = (Brush)cvt.ConvertFromString("#00000000");
            btn.Background.Opacity = 0;
            btn.MouseDoubleClick += Ti_MouseDoubleClick;
            btn.Click += Ti_Click;
            ti.Header = btn;

            TextBox tb = newTextBox();
            tbNow = tb;
            tb.SelectionChanged += tb_SlectionChanged;
            ti.Content = tb;

            txtBooks.Add(name, tb);

            nowEncoding = CharsetDetector.DetectFromFile(filePath).Detected.Encoding;
            books.Add(name, new Book(filePath, nowEncoding));
            book = books[name];

            txtChange(books[name].output(0, nStartLoad));
            setCatalog();
        }

        private void Ti_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            string name = btn.Content.ToString();
            foreach (TabItem ti in tbBooks.Items)
            {
                btn = ti.Header as Button;
                if (btn.Content.ToString() != name)
                    continue;

                tbBooks.SelectedItem = ti;
                //book = books[name];
                //tbNow = ti.Content as TextBox;
                break;
            }
            //setCatalog();
        }


        // 新建textbox
        private TextBox newTextBox()
        {
            TextBox tb = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Opacity = 0.5,
            };

            tb.ContextMenu = cmTXT;
            tb.SelectionChanged += txt_SlectionChanged;

            // 绑定字体
            Binding b = new Binding("SelectedItem");
            b.Source = cbFont;
            tb.SetBinding(FontFamilyProperty, b);

            b = new Binding("Value");
            b.Source = sFontSize;
            tb.SetBinding(FontSizeProperty, b);

            b = new Binding("SelectedItem.Content");
            b.Source = cbForeColor;
            tb.SetBinding(ForegroundProperty, b);

            b = new Binding("SelectedItem.Content");
            b.Source = cbBgColor;
            tb.SetBinding(BackgroundProperty, b);

            b = new Binding("SelectedItem");
            b.Source = cbFontWeight;
            tb.SetBinding(FontWeightProperty, b);

            b = new Binding("SelectedItem");
            b.Source = cbFontStrech;
            tb.SetBinding(FontStretchProperty, b);

            b = new Binding("Value");
            b.Source = sLineHeight;
            tb.SetBinding(TextBlock.LineHeightProperty, b);

            b = new Binding("Value");
            b.Source = asTextOpacity;
            tb.SetBinding(OpacityProperty, b);

            return tb;

        }


        //鼠标双击退出
        private void Ti_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button? btn = sender as Button;
            string name = btn.Content.ToString();
            foreach (TabItem ti in tbBooks.Items)
            {
                btn = ti.Header as Button;
                if (btn.Content.ToString() == name)
                {
                    tbBooks.Items.Remove(ti);
                    break;
                }
            }
            books.Remove(name);
            txtBooks.Remove(name);
            if(tbBooks.Items.Count == 0) 
            {
                tbBooks.Items.Add(new TabItem { Header = "无文件" });
                lvCatalog.Items.Clear();
            }
        }

        private void tb_SlectionChanged(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == null || tb.Text.Length == 0)
            {
                tbStatusSecSchedule.Text = string.Empty;
                pgBarText.Value = 0;
                return;
            }
            int n = tb.CaretIndex;
            tbStatusSecSchedule.Text = $"本节第{n}字/共{tb.Text.Length}字";
            pgBarText.Value = 100.0 * tb.CaretIndex / tb.Text.Length;
            
            if (lvCatalog.SelectedIndex < 0)
                return;
            try
            {
                wordSchedule();
            }
            catch (Exception ex)
            {
                txtInfo.AppendText($"{ex.Message}");
            }
        }

        #endregion

        #region 阅读控制

        private void sSoundVolume_ValueChanged(object sender, RoutedEventArgs e)
        {
            speech.setProp(volume: (int)sSoundVolume.Value);
            if(tbNow!=null)
                tbNow.Focus();
        }

        private void sSpeechRate_ValueChanged(object sender, RoutedEventArgs e)
        {
            speech.setProp(rate : (int)sSpeechRate.Value);
            if(tbNow!=null)
                tbNow.Focus();
        }

        private void setVoices(CultureInfo ci)
        {
            using (var synth = new SpeechSynthesizer())
            {
                cbSoundSource.ItemsSource = synth.GetInstalledVoices(ci)
                                                 .Select(v => v.VoiceInfo.Name);
            }
            cbSoundSource.SelectedIndex = 0;
        }

        private void cbSoundCulture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setVoices(new CultureInfo(
                cbSoundCulture.SelectedItem.ToString()));
        }
        
        private void cbSoundSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbSoundSource.SelectedItem;
            if(item!=null)
                speech.setVoice(item.ToString());
        }

        #endregion


        #region 工具栏
        // 关闭右侧栏
        private void btnCloseRight_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            bool flag = btn.Content.ToString() == "👉";
            Width += flag ? svRight.Width : -svRight.Width;
            btnCloseRight.Content = flag ? "👈" : "👉";
            svRight.Visibility = flag ? VISIBLE : COLLAPSED;
        }

        private void btnOpenTxt_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "文本文件(*.txt)|*.txt";
            if (ofd.ShowDialog() != true)
                return;
            filePath = ofd.FileName;
            createTextbox(filePath);
        }

        // 加载文件夹
        private void btnLoadFolder_Click(object sender, RoutedEventArgs e)
        {
            bookShelf = new BookShelf();
            loadFolder();
        }

        private void btnSaveTxt_Click(object sender, RoutedEventArgs e)
        {
            saveFile("now");
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            new WinDonate().ShowDialog();
        }

        private void btnCloseLeft_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            bool flag = btn.Content.ToString() == "👉";
            btn.Content = flag ? "👈" : "👉";
            ufgLeft.Visibility = flag ? VISIBLE : COLLAPSED;
        }


        #endregion

        // 文件清洗
        private void btnFMT_Click(object sender, RoutedEventArgs e)
        {
            if (book == null)
                return;
            if (fmtCheckBoxes[0].IsChecked == true)
                book.Trim();
            if (fmtCheckBoxes[1].IsChecked == true)
                book.mergePara(TERMINATOR);
            if (fmtCheckBoxes[2].IsChecked == true)
                book.deleteDoubleLine();
            txtInfo.Text = "清洗已完成！";
            setCatalog();
        }

        
        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            openFile(CodeMode.combo);
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            saveFile("combo");
        }

        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            book.Replace(tbOldText.Text, tbNewText.Text, 
                chkRegex.IsChecked == true);
            setCatalog();
        }

        //txt快捷键
        private void txtPreViewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyboardDevice.Modifiers == ModifierKeys.Control 
                && e.Key == Key.S)
                saveFile(filePath, CodeMode.auto);
        }

        private void btnSaveNote_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCloseNote_Click(object sender, RoutedEventArgs e)
        {
            gbNote.Visibility = COLLAPSED;
        }

        private void txt_SlectionChanged(object sender, RoutedEventArgs e)
        {
            if (tbNow.Text == null || tbNow.Text.Length == 0)
            {
                tbStatusSecSchedule.Text = string.Empty;
                pgBarText.Value = 0;
                return;
            }
            int n = tbNow.CaretIndex;
            tbStatusSecSchedule.Text = $"本节第{n}字/共{tbNow.Text.Length}字";
            pgBarText.Value = 100.0 * tbNow.CaretIndex / tbNow.Text.Length;

            if (lvCatalog.SelectedIndex < 0)
                return;
            try
            {
                wordSchedule();
            }catch (Exception ex)
            {
                txtInfo.AppendText($"{ex.Message}");
            }
        }

        //字阅读进度
        private void wordSchedule()
        {
            int n = tbNow.CaretIndex;
            n += book.preSecCount[lvCatalog.SelectedIndex];
            tbStatusTxtSchedule.Text = $"全文第{n / 1000.0:f1}k字/共{book.kwCount:f0}k字";
            double p = 100.0 * n / book.wCount;
            tbStatusPerSchedule.Text = $"已读{p:f2}%";
        }

        // 行阅读进度
        private string lineSchedule()
        {
            int nLines = tbNow.GetLineIndexFromCharacterIndex(tbNow.Text.Length) + 1;
            int iLine = tbNow.GetLineIndexFromCharacterIndex(tbNow.CaretIndex);
            double percent = 100.0 * iLine / nLines;
            return $"行进度：{iLine}/{nLines}({percent:f2}%)";
        }


        //页阅读进度
        private void pageSchedule()
        {
             
        }

        // 帮助
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            string path = @"src\help.txt";
            createTextbox(path);
        }

        private void btnComplex2Simple_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            book.Convert(btn.Content.ToString() != "转繁体");
            setCatalog();
        }

        private void btnNewTest_Click(object sender, RoutedEventArgs e)
        {
        }

        private void cbFontArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setOneCultureFonts();
        }

        private void rbSecMode_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            var strMode = rb.Content.ToString();
            bool flag = strMode == "章节模式";
            if (spRegCatalog != null)
            {
                spRegCatalog.IsEnabled = flag;
                btnSetSplitPages.IsEnabled = !flag;
            }
            switch (strMode)
            {
                case "章节模式": secMode = SecMode.section; break;
                case "分页模式": secMode = SecMode.page; break;
                default:break;
            }
        }

        // 分割
        private void btnSetSplitPages_Click(object sender, RoutedEventArgs e)
        {
            int n = (int)asPageLines.Value;
            book.setSplitParas(n, 20, 20);
            book.update();
            setCatalog();

        }

        // 书籍发生变化时的事件
        private void tbBooks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = tbBooks.SelectedItem as TabItem;
            if (item==null || item.Header == null)
                return;
            var btn = item.Header as Button;
            if(btn==null) return;
            var name = btn.Content.ToString();
            if (books.ContainsKey(name))
            {
                tbNow = txtBooks[name];
                book = books[name];
                setCatalog();
            }
        }

        private void btnSetBackImage_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "图下你个(*.jpg)|*.jpg";
            if (ofd.ShowDialog() != true)
                return;

            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri(ofd.FileName));
            tbBooks.Background = bg;
        }

        private FormattedText MeasureTextWidth(string text)
        {
            var tf = new Typeface(tbNow.FontFamily, 
                tbNow.FontStyle, tbNow.FontWeight, tbNow.FontStretch);
            FormattedText formattedText = new
                   FormattedText(text, CultureInfo.CurrentCulture,
                   tbNow.FlowDirection,
                   tf, tbNow.FontSize, Foreground);
            
            //, DpiUtil.GetPixelsPerDip);


            return formattedText;
        }

        #region 搜索
        private void btnSearchText_Click(object sender, RoutedEventArgs e)
        {
            var ti = tbBooks.SelectedItem as TabItem;
            var btn = ti.Header as Button;
            string name = btn.Content.ToString();
            string fString = txtForSearching.Text;

            string res;
            
            lvSearchResult.Items.Clear();
            lvSearchResult.Tag = fString;
            var secs = secMode == SecMode.page 
                               ? books[name].pages 
                               : books[name].secs;
            
            foreach (var sec in secs)
            {
                foreach (Match m in Regex.Matches(sec.content, fString))
                {
                    res = $"{sec.order},{m.Index}";
                    ListBoxItem lbi = new ListBoxItem { Content = res };
                    lbi.Tag = new int[] { sec.order, m.Index };
                    lvSearchResult.Items.Add(lbi);
                }
            }
        }

        private void lvSearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lbi = lvSearchResult.SelectedItem as ListBoxItem;
            if (lbi == null) return;
            var s = lvSearchResult.Tag as string;
            int N = s.Length;
            var tmp = lbi.Content.ToString().Split(",").ToArray();
            lvCatalog.SelectedIndex = int.Parse(tmp[0]);
            tbNow.Focus();
            tbNow.CaretIndex = int.Parse(tmp[1]);
            tbNow.SelectionStart = tbNow.CaretIndex;
            tbNow.SelectionLength = N;
        }
        #endregion
    }

}
