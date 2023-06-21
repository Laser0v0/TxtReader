using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace TxtReader
{
    public partial class MainWindow : Window
    {
        #region 全局变量
        int nReadDelay = 1000;
        int nReadLines = 5;
        int nReadPageDelay = 2000;

        CancellationTokenSource ctsScrollRead;      // 滚动线程取消标志
        #endregion

        #region 翻页模式
        private void pageRead(bool start)
        {
            if (start)
            {
                ctsScrollRead = new CancellationTokenSource();
                Task.Run(() => pageReadTask(ctsScrollRead.Token),
                    ctsScrollRead.Token);
            }
            else
            {
                ctsScrollRead.Cancel();
            }
        }

        private void pageReadTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Task.Delay(nReadPageDelay*1000).Wait();
                Dispatcher.BeginInvoke(() =>
                {
                    tbNow.PageDown();
                });
            }
        }
        #endregion


        #region 滚动阅读/跳行
        private void scrollRead(bool start)
        {
            if (start)
            {
                ctsScrollRead = new CancellationTokenSource();
                Task.Run(() => scrollReadTask(ctsScrollRead.Token),
                    ctsScrollRead.Token);
            }
            else
                ctsScrollRead.Cancel();
        }

        private void scrollReadTask(CancellationToken cts)
        {
            while (!cts.IsCancellationRequested)
            {
                Task.Delay(nReadDelay).Wait();
                Dispatcher.BeginInvoke(() => txtNextLine());
            }
        }

        private void txtLineJump(int i)
        {
            var nChars = tbNow.CaretIndex;
            int nLine = tbNow.GetLineIndexFromCharacterIndex(nChars);
            // 如果待跳转行小于-1，则前一章节
            // 如果待跳转行大于count，则后一章节
            
            if (nLine == tbNow.LineCount - 1) jumpSec(1);
            else if (nLine == 0) jumpSec(-1);
        }

        private void txtNextLine()
        {
            var nChars = tbNow.CaretIndex;
            int nLine = tbNow.GetLineIndexFromCharacterIndex(nChars);
            if (nLine == tbNow.LineCount - 1)
            {
                jumpSec(1);
                return;
            }
            int N = Math.Min(nReadLines, tbNow.LineCount - nLine);
            tbNow.CaretIndex += Enumerable.Range(nLine, N)
                                        .Select(tbNow.GetLineLength).Sum();
        }
        
        private void txtPreLine()
        {

            var nChars = tbNow.CaretIndex;
            int nLine = tbNow.GetLineIndexFromCharacterIndex(nChars);
            if (nLine == 0)
            {
                jumpSec(-1);
                return;
            }
            int st = Math.Max(0, nLine - nReadLines);
            int N = Math.Min(st, nLine - st);
            tbNow.CaretIndex -= Enumerable.Range(st, N)
                                        .Select(tbNow.GetLineLength).Sum();
        }
        #endregion

        #region 朗读
        private void speakRead()
        {
            speech.ctrlTask();
        }

        // 朗读跳转
        private void speechJump(int flag)
        {
            txtInfo.Text = (flag * nJumpChars).ToString();
            speech.Jump(flag * nJumpChars);
            if (tbNow != null)
                tbNow.Focus();
        }
        
        // 朗读进度事件
        private void Speech_SpeakProgress(object? sender, SpeakProgressEventArgs e)
        {
            speech.charPosition = e.CharacterPosition;
            Dispatcher.Invoke(() => {
                tbNow.CaretIndex = speech.st + e.CharacterPosition;
                tbNow.SelectionStart = speech.st + e.CharacterPosition;
                tbNow.SelectionLength = e.Text.Length;

            });
        }
        
        // 朗读完成事件
        private void Speech_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            if (lvCatalog.SelectedIndex == lvCatalog.Items.Count - 1)
                MySpeech.speakOne("本书已读完");
            else
                jumpSec(1, true);       //本章阅读完成后，阅读下一章
        }

        #endregion


        #region 点击事件
        private void btnReadStart_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string mode = cbReadMode.Text;
            bool flag = btn.Content.ToString() == "▶️";
            //txt.Focus();
            tbNow.Focus();
            if (mode == "朗读")
                speakRead();
            else if (mode == "滚动")
                scrollRead(flag);
            else if (mode == "翻页")
                pageRead(flag);
            btn.Content = flag ? "⏯️" : "▶️";
        }


        private void btnJump_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int flag = btn.Content.ToString() == "⏩" ? 1 : -1;
            string mode = cbReadMode.SelectedItem.ToString();
            tbNow.Focus();
            if (mode == "朗读" && speech.IsSpeaking())
                speechJump(flag);
            else if (mode == "滚动")
            {
                if (flag == 1) txtNextLine();
                else if (flag == -1) txtPreLine();
            }
            else if (mode == "翻页")
            {
                if(flag == 1) tbNow.PageDown();
                else if (flag == -1) tbNow.PageUp();
            }
        }

        private void btnJumpSec_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int i = btn.Content.ToString() == "⏭️" ? 1 : -1;
            jumpSec(i);
        }

        #endregion



        private void AdvancedSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            string mode = cbReadMode.SelectedItem.ToString();
            if (mode == "翻页")
                nReadPageDelay = (int)sReadSpeed.Value;
            else if (mode == "滚动")
                nReadDelay = (int)sReadSpeed.Value + 0;
            nReadLines = (int)sReadLine.Value;
            if(tbNow!=null)
                tbNow.Focus();
        }

        private void cbReadMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            string mode = cb.SelectedItem.ToString();
            if (mode == "翻页")
                sReadSpeed.setProperty(1, 20, "翻页延时", "s", nReadPageDelay);
            else if (mode == "滚动")
                sReadSpeed.setProperty(100, 5000, "滚动延时", "ms", nReadDelay);
            sReadLine.IsEnabled = mode == "滚动";
            sReadSpeed.IsEnabled = mode != "朗读";
            sSoundVolume.IsEnabled = mode == "朗读";
        }

        private void txtChange(string text, int ed = -1)
        {
            if (ed > 0)
            {
                while (!TERMINATOR.Contains(text[ed++]))
                    continue;
                var test = text[ed];
                text = text[..ed];
            }
            // 原为txt
            tbNow.Text = text;
            speech = new MySpeech(text, cbSoundSource.SelectedItem.ToString(),
                (int)sSpeechRate.Value, (int)sSoundVolume.Value);
            speech.speech.SpeakProgress += Speech_SpeakProgress;
            speech.speech.SpeakCompleted += Speech_SpeakCompleted;
        }


        //章节跳转
        private void jumpSec(int i, bool isSpeaking=false)
        {
            int n = lvCatalog.SelectedIndex;
            n += i;
            if (n >= lvCatalog.Items.Count)
                n = lvCatalog.Items.Count - 1;
            if (n < 0)
                n = 0;
            if(!isSpeaking)
                isSpeaking = speech.IsSpeaking();
            if (isSpeaking)
                speech.Cancel();
            
            lvCatalog.SelectedIndex = n;
            if (isSpeaking)
                speech.Speak();
            tbNow.Focus();
        }


    }
}
