using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using System.Speech.Synthesis;
using System.Globalization;
using UtfUnknown;
using Newtonsoft.Json;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace TxtReader
{
    public partial class MainWindow: Window
    {
        #region 常量
        const string PARA_PATH = "para.json";
        static readonly string[] modeCatalog = new string[]
        {
            "行尾模式", "严格模式", "文末目录"
        };

        static readonly string[] regCatalog = new string[]
        {
            "标题前缀","标题中缀", "标题后缀"
        };

        static readonly string[] ufgCatalog = new string[]
        {
            "最短标题", "最长标题",
            "可选标题", "目录名称",
            "必备字符", "非法字符",
            "非法开头", "非法结尾",
        };

        static readonly string[] FMT_METHOD = new string[] { "清理空格", "段落拼接", "去除空行" };

        #endregion

        Dictionary<string, ComboBox> dctComboBoxes;
        Dictionary<string, AdvanceSlider> dctAdvanceSliders;

        CheckBox[] fmtCheckBoxes = FMT_METHOD.Select(
            s => new CheckBox() { Content = s, Margin = new Thickness(5) }).ToArray();

        public void initParaDct()
        {
            dctComboBoxes = new Dictionary<string, ComboBox>
            {
                {"字体语言", cbFontArea},
                {"字体", cbFont},
                {"语音语言", cbSoundCulture },
                {"音源", cbSoundSource },
                {"编码", cbEncoding }
            };
            
            dctAdvanceSliders = new Dictionary<string, AdvanceSlider>
            {
                {"尺寸", sFontSize },
                {"音量", sSoundVolume},
                {"语速", sSpeechRate},
                {"左宽", asLeftWidth},
                {"右宽", asRightWidth},
                {"分页行数", asPageLines },
                {"透明度", asTextOpacity }
            };
            
            if (!File.Exists(PARA_PATH))
                saveParaJson(PARA_PATH);
            loadParaJson(PARA_PATH);
        }

        // 布局初始化
        public void init()
        {
            initTextSet();
            initColors();

            // 阅读模式
            cbReadMode.ItemsSource = new string[] { "滚动", "翻页", "朗读" };
            cbReadMode.SelectedIndex = 0;
            
            // 加载音源
            using (var synth = new SpeechSynthesizer())
            {
                cbSoundCulture.ItemsSource = synth.GetInstalledVoices()
                                                 .Select(v => v.VoiceInfo.Culture.ToString());
                cbSoundCulture.SelectedIndex = 0;
            }
            setVoices(new CultureInfo(cbSoundCulture.SelectedItem.ToString()));

            // 设置文本编码
            cbEncoding.ItemsSource = Encoding.GetEncodings()
                       .Select(x => x.Name);
            cbEncoding.SelectedIndex = 0;

        }


        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if(chkCloseExport.IsChecked == true)
                saveParaJson(PARA_PATH);
        }

        # region 初始化颜色和字体
        private void initColors()
        {
            string[] cs = typeof(Brushes)
                       .GetProperties(BindingFlags.Static | BindingFlags.Public)
                       .Select(x => x.Name)
                       .ToArray();
            int i;
            int iWhite = Array.IndexOf(cs, "White");
            int iBlack = Array.IndexOf(cs, "Black");


            cbForeColor.ItemsSource = cs.Select(x => new ComboBoxItem { Content = x });
            cbForeColor.SelectedIndex = iBlack;

            cbBgColor.ItemsSource = cs.Select(x => new ComboBoxItem { Content = x });
            cbBgColor.SelectedIndex = iWhite;

            var bc = new BrushConverter();
            foreach (ComboBoxItem item in cbForeColor.Items)
                item.Background = (SolidColorBrush)bc.ConvertFromString(
                    item.Content.ToString());

            foreach (ComboBoxItem item in cbBgColor.Items)
                item.Background = (SolidColorBrush)bc.ConvertFromString(
                    item.Content.ToString());

            List<XmlLanguage> lstFontCultures = new List<XmlLanguage>();
            foreach (var font in Fonts.SystemFontFamilies)
                lstFontCultures.AddRange(font.FamilyNames.Keys);

            cbFontArea.ItemsSource = lstFontCultures
                .Distinct().Select(x => x.ToString());

            cbFontArea.SelectedItem = "zh-cn";
            setOneCultureFonts();

            cbFontWeight.ItemsSource = getStaticName(typeof(FontWeights));
            cbFontWeight.SelectedIndex = 5;

            cbFontStrech.ItemsSource = getStaticName(typeof(FontStretches));
            cbFontStrech.SelectedIndex = 4;

            cbFontStyle.ItemsSource = getStaticName(typeof(FontStyles));
            cbFontStyle.SelectedIndex = 0;


        }

        private string[] getStaticName(Type t)
        {
            return t
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Select(x => x.Name).ToArray();
        }


        private void setOneCultureFonts()
        {
            var culture = cbFontArea.SelectedItem.ToString();
            var L = XmlLanguage.GetLanguage(culture);
            string tmpStr;
            List<string> names = new List<string>();
            foreach (var font in Fonts.SystemFontFamilies)
                if (font.FamilyNames.TryGetValue(L, out tmpStr))
                    names.Add(tmpStr);
            cbFont.ItemsSource = names;
            cbFont.SelectedIndex = 0;
        }
        #endregion

        #region 目录初始化
        Dictionary<string, CheckBox> kvpChkCatalogSet = new Dictionary<string, CheckBox>();
        Dictionary<string, TextBoxWithLabel> tbwlDct = new Dictionary<string, TextBoxWithLabel>();

        public void initTextSet()
        {
            TextBox tb;
            Button btn;
            CheckBox chk;
            DockPanel dp;
            UniformGrid ufg;
            TextBoxWithLabel tbwl;

            ufg = new UniformGrid { Columns = 2 };
            spRegCatalog.Children.Add(ufg);
            foreach (var key in ufgCatalog)
            {
                tbwl = new TextBoxWithLabel { Label = key };
                ufg.Children.Add(tbwl);
                tbwlDct.Add(key, tbwl);
            }

            // 规范目录的选项
            foreach (var key in regCatalog)
            {
                tbwl = new TextBoxWithLabel { Label = key };
                spRegCatalog.Children.Add(tbwl);
                tbwlDct.Add(key, tbwl);
            }

            ufg = new UniformGrid { Columns = 4 };
            spRegCatalog.Children.Add(ufg);

            foreach (var key in modeCatalog)
            {
                kvpChkCatalogSet.Add(key, new CheckBox
                {
                    Content = key,
                    VerticalAlignment = VerticalAlignment.Center
                });
                ufg.Children.Add(kvpChkCatalogSet[key]);
            }

            btn = new Button { Content = "启用" };
            btn.Click += btnUseRegCatalog_Click;
            ufg.Children.Add(btn);

            // 自动目录的选项

            foreach (var ch in fmtCheckBoxes)
                ufgCheckBoxes.Children.Add(ch);

        }

        private void btnUseRegCatalog_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> dctStr = new Dictionary<string, string>();
            foreach (var item in tbwlDct)
                dctStr.Add(item.Key, item.Value.Text);

            Dictionary<string, bool> dctBool = new Dictionary<string, bool>();
            foreach (var chk in kvpChkCatalogSet.Values)
                dctBool.Add(chk.Content.ToString(), chk.IsChecked==true);

            string info = book.setRegular(dctStr, dctBool);
            txtInfo.AppendText(info);
            book.update();
            setCatalog();
        }
        #endregion

        #region 配置文件的写入和读取
        const Visibility VISIBLE = Visibility.Visible;
        const Visibility COLLAPSED = Visibility.Collapsed;

        const string UNFOLD_R = "展开右侧";
        const string UNFOLD_L = "展开左侧";
        const string WIN_H = "窗口高度";
        const string WIN_W = "窗口宽度";
        const string SEC_MODE = "章节模式";

        private void saveParaJson(string filePath)
        {
            var dct = new Dictionary<string, string>();
            foreach (var item in dctComboBoxes)
                dct[item.Key] = item.Value.Text;
            dct["前景色"] = cbForeColor.SelectedIndex.ToString();
            dct["背景色"] = cbBgColor.SelectedIndex.ToString();
            switch (secMode)
            {
                case SecMode.section: dct[SEC_MODE] = "0"; break;
                case SecMode.page: dct[SEC_MODE] = "1"; break;
                default: dct[SEC_MODE] = "2"; break;
            }

            foreach (var item in dctAdvanceSliders)
                dct[item.Key] = item.Value.ToString();
            foreach (var item in fmtCheckBoxes)
                dct[$"{item.Content}"] = item.IsChecked.ToString();

            if(bookShelf != null)
                dct["根目录"] = bookShelf.root;

            dct["已打开书籍"] = string.Join("👀", 
                books.Values.Select(b=>b.path).ToArray());
            dct["当前文件"] = this.filePath;
            dct["载入设置"] = chkOpenImport.ToString();
            
            #region 布局相关
            bool bTemp = svRight.Visibility == VISIBLE;
            dct[UNFOLD_R] = bTemp.ToString();
            bTemp = ufgLeft.Visibility == VISIBLE;
            dct[UNFOLD_L] = bTemp.ToString();

            if (ActualHeight > 0)
            {
                dct[WIN_H] = ActualHeight.ToString();
                dct[WIN_W] = ActualWidth.ToString();
            }

            dct["文本选项卡号"]  = tcTextSetting.SelectedIndex.ToString();
            #endregion

            #region 播放控制
            dct["翻页延时"] = nReadPageDelay.ToString();
            dct["滚动延时"] = nReadDelay.ToString();
            dct["滚动行数"] = nReadLines.ToString();
            dct["播放模式"] = cbReadMode.SelectedItem.ToString();
            #endregion

            #region 文本设置
            if (book == null)
                book = new Book("");
            foreach (var item in book.cataSet)
                dct[item.Key] = item.Key == "换行符号" ?
                    StaticMethods.toLiteral(item.Value) : item.Value;

            #endregion

            string js = JsonConvert.SerializeObject(dct, Formatting.Indented);
            File.WriteAllText(filePath, js, Encoding.UTF8);
        }

        private void loadParaJson(string filePath)
        {
            if (!File.Exists(filePath))
                return;
            string js = File.ReadAllText(filePath);
            var dct = JsonConvert.DeserializeObject<Dictionary<string, string>>(js);
            if(dct==null) return;

            if (dct.ContainsKey("载入设置") && dct["载入设置"] == "false")
                return;

            foreach (var item in dctComboBoxes)
                if (dct.ContainsKey(item.Key))
                    item.Value.SelectedItem = dct[item.Key];

            double d;

            if (dct.ContainsKey("前景色") && double.TryParse(dct["前景色"], out d))
                cbForeColor.SelectedIndex = (int)d;

            if (dct.ContainsKey("背景色") && double.TryParse(dct["背景色"], out d))
                cbBgColor.SelectedIndex = (int)d;

            if (dct.ContainsKey(SEC_MODE))
            {
                switch (dct[SEC_MODE])
                {
                    case "0": rbSecMode.IsChecked = true; break;
                    case "1": rbPageMode.IsChecked = true; break;
                    default: rbSecPage.IsChecked = true; break; ;
                }
            }


            // 宽高
            if (dct.ContainsKey(WIN_H) && double.TryParse(dct[WIN_H], out d))
                Height = d;

            if (dct.ContainsKey(WIN_W) && double.TryParse(dct[WIN_W], out d))
                Width = d;

            foreach (var item in dctAdvanceSliders)
            {
                if (!dct.ContainsKey(item.Key))
                    continue;
                if (!double.TryParse(dct[item.Key], out d))
                    continue;
                item.Value.Value = d;
            }

            bool b;
            foreach (var item in fmtCheckBoxes)
            {
                if (!dct.ContainsKey($"item.Content"))
                    continue;
                if (!bool.TryParse(dct[$"item.Content"], out b))
                    continue;
                item.IsChecked = b;
            }

            #region 布局相关
            if (dct.ContainsKey(UNFOLD_R) && bool.TryParse(dct[UNFOLD_R], out b))
            {
                svRight.Visibility = b ? VISIBLE : COLLAPSED;
                btnCloseRight.Content = b ? "👈" : "👉";
            }

            if (dct.ContainsKey(UNFOLD_L) && bool.TryParse(dct[UNFOLD_L], out b))
            {
                ufgLeft.Visibility = b ? VISIBLE : COLLAPSED;
                btnCloseLeft.Content = b ? "👈" : "👉";
            }

            // 宽高
            if (dct.ContainsKey(WIN_H) && double.TryParse(dct[WIN_H], out d))
                Height = d;

            if (dct.ContainsKey(WIN_W) && double.TryParse(dct[WIN_W], out d))
                Width = d;


            if (dct.ContainsKey("文本选项卡号") && double.TryParse(dct["文本选项卡号"], out d))
                tcTextSetting.SelectedIndex = (int)d;
            #endregion

            #region 播放控制
            if (dct.ContainsKey("翻页延时"))
                int.TryParse(dct["翻页延时"], out nReadPageDelay);
            if (dct.ContainsKey("滚动延时"))
                int.TryParse(dct["滚动延时"], out nReadDelay);
            if (dct.ContainsKey("滚动行数") 
                && int.TryParse(dct["滚动行数"], out nReadLines))
                    sReadLine.Value = nReadLines;
            if (dct.ContainsKey("播放模式"))
                cbReadMode.SelectedItem = dct["播放模式"];

            #endregion

            #region 目录设置
            foreach (var key in tbwlDct.Keys)
            {
                if (dct.ContainsKey(key))
                    tbwlDct[key].Text = dct[key];
            }
            if(dct.ContainsKey("换行符号"))
                tbwlLF.Text = dct["换行符号"];
            #endregion

            if (dct.ContainsKey("根目录"))
            {
                bookShelf = new BookShelf(dct["根目录"]);
                loadFolder();
            }

            if (dct.ContainsKey("已打开书籍"))
            {
                foreach (var path in dct["已打开书籍"].Split("👀"))
                {
                    createTextbox(path);
                }
            }

            if (dct.ContainsKey("当前文件"))
            {
                this.filePath = dct["当前文件"];
                openFile();
            }


        }
        #endregion

        #region 配置文件交互按钮
        private void btnSavePara_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = FMT_CFG;
            if (sfd.ShowDialog() != true) return;
            saveParaJson(sfd.FileName);
        }

        private void btnLoadPara_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = FMT_CFG;
            if (ofd.ShowDialog() != true)
                return;
            try
            {
                loadParaJson(ofd.FileName);
            }
            catch (Exception ex)
            {
                txtInfo.AppendText($"文件加载失败，原因为{ex.Message}");
            }

        }
        #endregion


    }
}
