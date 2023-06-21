using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UtfUnknown;
namespace TxtReader
{
    public partial class MainWindow : Window
    {
        private void initContextMenu()
        {
            initTxtMenu();
            initTreeMenu();
        }

        #region 书架的右键菜单
        ContextMenu cmTV = new ContextMenu();       //书籍目录的右键菜单
        ContextMenu cmTVI = new ContextMenu();      // 书籍的右键菜单

        private void initTreeMenu()
        {
            var mi = new MenuItem() { Header = "书籍信息" };
            mi.Click += mitvBookInfo_Click;
            cmTVI.Items.Add(mi);

            mi = new MenuItem() { Header = "进入文件夹" };
            mi.Click += mitvOpenFolder_Click;
            cmTVI.Items.Add(mi);

        }

        // 书籍信息
        private void mitvBookInfo_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = ContextMenuService.GetPlacementTarget(
                LogicalTreeHelper.GetParent(sender as MenuItem)) as TreeViewItem;

            var bs = tvi.Tag as BookShelf;
            string path = Path.Combine(bs.root, tvi.Header.ToString());
            string info = BookInfo.FromPath(path);
            txtInfo.AppendText($"{info}\r\n");

        }

        // 进入文件夹
        private void mitvOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = ContextMenuService.GetPlacementTarget(
                LogicalTreeHelper.GetParent(sender as MenuItem)) as TreeViewItem;
            var bs = tvi.Tag as BookShelf;
            System.Diagnostics.Process.Start("explorer.exe", bs.root);


        }
        #endregion


        #region txt右键菜单
        ContextMenu cmTXT = new ContextMenu();

        private void initTxtMenu()
        {
            ContextMenu cm = cmTXT;

            /*
            mi = new MenuItem() { Header = "翻译" };
            mi.Click += miTranslate_Click;
            cm.Items.Add(mi);
            */


            var mi = new MenuItem() { Header = "设为目录" };
            mi.Click += miSetCatalog_Click;
            cm.Items.Add(mi);

            mi = new MenuItem() { Header = "设为换行符" };
            mi.Click += miSetLF_Click;
            cm.Items.Add(mi);

            mi = new MenuItem() { Header = "全部删除" };
            mi.Click += miDelete_Click;
            cm.Items.Add(mi);

            mi = new MenuItem() { Header = "摘抄" };
            mi.Click += miToNote_Click;
            cm.Items.Add(mi);

            mi = new MenuItem() { Header = "朗读" };
            mi.Click += miRead_Click;
            cm.Items.Add(mi);

            mi = new MenuItem() { Header = "拼音" };
            mi.Click += miPinYin_Click;
            cm.Items.Add(mi);

            mi = new MenuItem() { Header = "关闭全部"};
            mi.Click += miCloseAll_Click;
            cm.Items.Add(mi);

            /*
            mi = new MenuItem() { Header = "复制" };
            mi.Click += miCopy_Click;
            cm.Items.Add(mi);

            mi = new MenuItem() { Header = "剪切" };
            mi.Click += miCut_Click;
            cm.Items.Add(mi);

            mi = new MenuItem() { Header = "粘贴" };
            mi.Click += miPaste_Click;
            cm.Items.Add(mi);             
            */

        }

        private void miCloseAll_Click(object sender, RoutedEventArgs e)
        {
            books.Clear();
            txtBooks.Clear();
            tbBooks.Items.Clear();
            lvCatalog.Items.Clear();
            tbBooks.Items.Add(new TabItem { Header = "无文件" });
        }

        private void miPinYin_Click(object sender, RoutedEventArgs e)
        {
            var text = string.Join("\r\n",
                tbNow.SelectedText.Select(StaticMethods.getPinYin));
            txtInfo.AppendText(text);
        }

        private void miToNote_Click(object sender, RoutedEventArgs e)
        {
            gbNote.Visibility = VISIBLE;
            tbNote.AppendText(tbNow.SelectedText + "\r\n");
        }

        private void miSetLF_Click(object sender, RoutedEventArgs e)
        {
            book.reSetLF(tbNow.SelectedText);
            setCatalog();
        }


        private void miSetCatalog_Click(object sender, RoutedEventArgs e)
        {
            if(tbNow.SelectedText!=null)
                book.addHeads(tbNow.SelectedText);
            setCatalog();
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            var text = tbNow.SelectedText;
            if (text == "")
                return;
            tbNow.Text = tbNow.Text.Replace(text, "");
            string info = book.deleteAll(text);
            setCatalog();
            txtInfo.AppendText(info);
        }


        int nReadMaxWord = 20;     // 最大阅读字数
        // 阅读文字
        private void miRead_Click(object sender, RoutedEventArgs e)
        {
            var text = tbNow.SelectedText;
            if (text.Length > nReadMaxWord)
                return;

            MySpeech.speakOne(text, (int)sSpeechRate.Value,
                (int)sSoundVolume.Value, cbSoundSource.Text);
        }

        // 翻译
        private void miTranslate_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region 复制剪贴粘贴
        private void miCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbNow.SelectedText);
        }

        private void miCut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbNow.SelectedText);
            tbNow.Text = tbNow.Text.Remove(tbNow.SelectionStart, tbNow.SelectionLength);
        }

        private void miPaste_Click(object sender, RoutedEventArgs e)
        {
            var t = tbNow.Text.Remove(tbNow.SelectionStart, tbNow.SelectionLength);
            tbNow.Text = t.Insert(tbNow.SelectionStart, Clipboard.GetText());
        }
        #endregion

        #endregion

    }
}
