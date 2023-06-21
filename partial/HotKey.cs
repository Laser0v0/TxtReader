using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TxtReader
{
    public partial class MainWindow : Window
    {
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            #region 翻页相关
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers==ModifierKeys.Control)
                turnPage(1);

            int n = 0;
            switch (e.Key)
            {
                case Key.PageDown:
                case Key.Down:
                case Key.N: n = 1; break;
                case Key.PageUp:
                case Key.Up:
                case Key.L: n = -1; break;
                case Key.Home:
                case Key.H: n = -10000; break;
                case Key.End:
                case Key.E: n = 10000; break;
            }

            if (n == 0)
                return;

            switch (e.KeyboardDevice.Modifiers)
            {
                case ModifierKeys.Alt: turnPage(n); break;
                case ModifierKeys.Control: turnTitle(n); break;
            }
            #endregion
        }

        // 章节跳转
        private void turnTitle(int n)
        {
            int ind = lvCatalog.SelectedIndex + n;
            ind = Math.Max(ind, 0);
            ind = Math.Min(ind, lvCatalog.Items.Count - 1);
            lvCatalog.SelectedIndex = ind;
        }

        // 页码跳转
        private void turnPage(int n)
        {
            int st = tbNow.GetFirstVisibleLineIndex();
            int ed = tbNow.GetLastVisibleLineIndex();
            int delta = ed - st;
            st += delta * n;
            st = Math.Max(st, 0);
            st = Math.Min(tbNow.LineCount - delta, st);
            tbNow.ScrollToLine(st);
        }

    }
}
