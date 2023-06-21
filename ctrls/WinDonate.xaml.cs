using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using QRCoder;
using System.IO;
using System.Drawing.Imaging;

namespace TxtReader
{
    /// <summary>
    /// WinDonate.xaml 的交互逻辑
    /// </summary>
    public partial class WinDonate : Window
    {
        const string wx = "wxp://f2f0nNBbv4ZbEgDZbRVSAuFk1GvPVgHUsCnCx1mERR54iVg";
        const string zfb = "https://qr.alipay.com/fkx13243ksusku5oj4anhed";
        const string LOGO_PATH = @"src/头像.png";
        public WinDonate()
        {
            InitializeComponent();
            setQR(wx, imgWX);
            setQR(zfb, imgZFB);
            PreviewKeyDown += WinDonate_PreviewKeyDown;
        }

        private BitmapImage bmp2bmpImg(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                bmpImg.StreamSource = ms;
                bmpImg.EndInit();
                bmpImg.Freeze();
                return bmpImg;
            }
        }

        private void setQR(string msg, System.Windows.Controls.Image img)
        {
            QRCodeGenerator qrcg = new QRCodeGenerator();
            var codeData = qrcg.CreateQrCode(msg, QRCodeGenerator.ECCLevel.M, true);//, true, QRCodeGenerator.EciMode.Utf8, 1);
            var code = new QRCode(codeData);
            Bitmap icon = new Bitmap(LOGO_PATH);
            Bitmap bmp = code.GetGraphic(7, Color.Black, Color.White, icon, 20, 5, true);
            img.Source = bmp2bmpImg(bmp);
        }

        private void WinDonate_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }

    }
}
