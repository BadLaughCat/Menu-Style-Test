using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
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
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Menu_Style_Test
{
    public partial class MainWindow : Window
    {
        double Dpi = Graphics.FromHwnd(IntPtr.Zero).DpiX / 96;

        public MainWindow()
        {
            InitializeComponent();
            Open.Click += (sender, e) => { Refresh(); SampleMenu.IsOpen = true; };
            SampleMenu.Opened += (sender, e) => { BlurBackground(SampleMenu); };
        }

        private void Refresh()
        {
            Dpi = Graphics.FromHwnd(IntPtr.Zero).DpiX / 96;
            SampleMenu.Items.Clear();
            BringItems(null);
        }

        private void BringItems(MenuItem Parent)
        {
            foreach (string newItem in File.ReadAllText($"./{(Parent == null ? "main" : Parent.Header)}.txt").Split('\n'))
            {
                string SafeName = newItem.Trim();
                if (SafeName == "-")
                {
                    Separator Current = new Separator { Style = (Style)Application.Current.Resources["Separator"] };

                    if (Parent == null)
                        SampleMenu.Items.Add(Current);
                    else
                        Parent.Items.Add(Current);
                }
                else
                {
                    MenuItem Current = new MenuItem
                    {
                        Header = SafeName,
                        Style = (Style)Application.Current.Resources["MenuItem"]
                    };

                    if (File.Exists($"./{SafeName}.txt"))
                    {
                        BringItems(Current);
                        SampleMenu.Opened += (sender, e) => BlurBackground(Current, SampleMenu);
                    }

                    if (Parent == null)
                        SampleMenu.Items.Add(Current);
                    else
                        Parent.Items.Add(Current);
                }
            }
        }

        private void BlurBackground(MenuItem Target, ContextMenu Parent)
        {
            int PopupLeft = System.Windows.Forms.Cursor.Position.X, PopupTop = System.Windows.Forms.Cursor.Position.Y, TotalWidth = 4, TotalHeight = 12;
            List<int> TitleWidths = new List<int>();
            foreach (UIElement subElement in Target.Items)
            {
                if (subElement is Separator)
                    TotalHeight += 9;
                else if (subElement is MenuItem Current)
                {
                    TotalHeight += 24;
                    TitleWidths.Add(TextWidth(Current.Header.ToString()));
                }
            }
            TitleWidths.Sort();
            TotalWidth += TitleWidths.Last() + 57;

            PopupLeft += (int)((Parent.ActualWidth - 40) * Dpi);
            foreach (UIElement subElement in Parent.Items)
                if (subElement is Separator)
                    PopupTop += (int)(9 * Dpi);
                else if (subElement is MenuItem Current)
                {
                    if (Current == Target)
                        break;
                    PopupTop += (int)(24 * Dpi);
                }

            Bitmap bmp = new Bitmap((int)(TotalWidth * Dpi), (int)(TotalHeight * Dpi));
            Graphics grp = Graphics.FromImage(bmp);
            grp.CopyFromScreen(PopupLeft, PopupTop, 0, 0, new Size(bmp.Width, bmp.Height));
            grp.Dispose();
            bmp = new GaussianBlur(100).ProcessImage(bmp);
            Target.Background = new ImageBrush(BitmapToBitmapImage(bmp));
            bmp.Dispose();
        }

        private void BlurBackground(ContextMenu Target)
        {
            Bitmap bmp = new Bitmap((int)((Target.ActualWidth - 40) * Dpi), (int)((Target.ActualHeight - 40) * Dpi));
            Graphics grp = Graphics.FromImage(bmp);
            Point MousePos = System.Windows.Forms.Cursor.Position;
            grp.CopyFromScreen(MousePos.X, MousePos.Y, 0, 0, new Size(bmp.Width, bmp.Height));
            grp.Dispose();
            bmp = new GaussianBlur(100).ProcessImage(bmp);
            Target.Background = new ImageBrush(BitmapToBitmapImage(bmp));
            bmp.Dispose();
        }

        private int TextWidth(string Text) => (int)Math.Round(Graphics.FromImage(new Bitmap(1, 1)).MeasureString(Text, new Font("Microsoft YaHei UI", 12, GraphicsUnit.Pixel)).Width);

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
}
