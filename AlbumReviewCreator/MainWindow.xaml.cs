using System.Globalization;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Image = System.Drawing.Image;
using System;
using System.Diagnostics;

namespace AlbumReviewCreator
{
    public partial class MainWindow : Window
    {
        private string generatedImagePath;

        public MainWindow()
        {
            InitializeComponent();
            AlbumName.GotFocus += TextBox_GotFocus;
            AlbumName.LostFocus += TextBox_LostFocus;
            ArtistName.GotFocus += TextBox_GotFocus;
            ArtistName.LostFocus += TextBox_LostFocus;
            ReleaseDate.GotFocus += TextBox_GotFocus;
            ReleaseDate.LostFocus += TextBox_LostFocus;
            Num.GotFocus += TextBox_GotFocus;
            Num.LostFocus += TextBox_LostFocus;
            Review.GotFocus += TextBox_GotFocus;
            Review.LostFocus += TextBox_LostFocus;
            MusicGenre.GotFocus += TextBox_GotFocus;
            MusicGenre.LostFocus += TextBox_LostFocus;
        }

        // 定义用于存储数据的类
        public class AlbumData
        {
            public string AlbumName { get; set; }
            public string ArtistName { get; set; }
            public string ReleaseDate { get; set; }
            public string Num { get; set; }
            public string Review { get; set; }
            public string StarredTracksCnt { get; set; }
            public string AllTracksCnt { get; set; }
            public string SliderPosition { get; set; }
            public string AlbumCoverPath { get; set; }
            public string MusicGenre { get; set; }
        }

        // 将输入的内容保存为JSON
        private void SaveAsJson_Click(object sender, RoutedEventArgs e)
        {
            // 获取文本框的值
            string albumName = AlbumName.Text;
            string artistName = ArtistName.Text;
            string releaseDate = ReleaseDate.Text;
            string num = Num.Text;
            string review = Review.Text;
            string musicGenre = MusicGenre.Text;
            string starredTracksCnt = StarredTracksCnt.Text;
            string allTracksCnt = AllTracksCnt.Text;
            // 获取滑动条的值，根据滑动条的值判断挡位信息
            double sliderValue = CorrectionSlider.Value;
            string sliderPosition = GetSliderPosition(sliderValue);
            // 初始化专辑封面的文件名和路径
            string albumCoverFileName = "";
            string albumCoverPath = "";

            // 创建一个AlbumData类的实例
            var data = new AlbumData
            {
                AlbumName = albumName,
                ArtistName = artistName,
                ReleaseDate = releaseDate,
                Num = num,
                Review = review,
                MusicGenre = musicGenre,
                StarredTracksCnt = starredTracksCnt,
                AllTracksCnt = allTracksCnt,
                SliderPosition = sliderPosition,
                AlbumCoverPath = albumCoverPath
            };

            try
            {
                // 将对象序列化为JSON字符串
                string jsonContent = JsonConvert.SerializeObject(data, Formatting.Indented);

                // 构建文件路径
                string jsonPath = @"D:\album_review\json";
                string coverPath = @"D:\album_review\cover";
                string jsonName = $"{artistName}_{num}_{albumName}.json";
                string filePath = Path.Combine(jsonPath, jsonName);
                Directory.CreateDirectory(jsonPath);
                Directory.CreateDirectory(coverPath);

                // 获取选定的图片文件路径
                string imagePath = GetImagePath();

                if (!string.IsNullOrEmpty(imagePath))
                {
                    // 创建专辑封面的文件名
                    albumCoverFileName = $"{artistName}_{num}_{albumName}{Path.GetExtension(imagePath)}";

                    // 构建专辑封面的目标路径
                    albumCoverPath = Path.Combine(coverPath, albumCoverFileName);

                    // 复制图片到封面目录
                    //File.Copy(imagePath, albumCoverPath, true);
                }

                // 更新JSON数据中的专辑封面路径
                data.AlbumCoverPath = albumCoverPath;

                // 将对象序列化为JSON字符串
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);

                // 将JSON字符串写入文件
                File.WriteAllText(filePath, json);

                // JSON写入完成提示
                MessageBox.Show($"JSON file saved successfully at {filePath}");

                // 生成并保存图片
                generatedImagePath = GenerateAndSaveImage(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // 预览生成好的图片
        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(generatedImagePath))
                {
                    // 使用系统默认程序打开图片文件
                    Process.Start(generatedImagePath);
                }
                else
                {
                    MessageBox.Show("No generated image found. Save as JSON first.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // 获取已加载图片的路径
        private string GetImagePath()
        {
            if (AlbumCover.Background is ImageBrush imageBrush)
            {
                BitmapImage bitmapImage = (BitmapImage)imageBrush.ImageSource;
                return bitmapImage.UriSource.LocalPath;
            }
            return string.Empty; // 返回空字符串表示没有加载图片
        }

        // 将滑动条挡位转为字符串存储
        private string GetSliderPosition(double value)
        {
            // 根据滑动条的值判断挡位
            if (value < 0)
            {
                return "Low";
            }
            else if (value == 0)
            {
                return "Medium";
            }
            else
            {
                return "High";
            }
        }

        // 从JSON文件加载数据并应用到界面上
        private void LoadFromJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建文件选择对话框
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JSON Files (*.json)|*.json";
                openFileDialog.Title = "Select a JSON File";

                // 用户点击“确定”按钮时执行
                if (openFileDialog.ShowDialog() == true)
                {
                    // 获取选定的文件路径
                    string filePath = openFileDialog.FileName;

                    // 读取文件内容
                    string jsonContent = File.ReadAllText(filePath);

                    // 将 JSON 字符串反序列化为对象
                    var loadedData = JsonConvert.DeserializeObject<AlbumData>(jsonContent);

                    // 将加载的数据应用到界面上的控件
                    ApplyLoadedDataToUI(loadedData);

                    // 检查专辑封面路径是否为空或为null
                    if (!string.IsNullOrEmpty(loadedData.AlbumCoverPath))
                    {
                        // 避免在显示图片时直接使用原始图片
                        string albumCoverPath = loadedData.AlbumCoverPath;
                        string tempDirectory = @"D:\album_review\temp";
                        string fileName = Path.GetFileName(albumCoverPath);
                        string destinationPath = Path.Combine(tempDirectory, fileName);
                        File.Copy(albumCoverPath, destinationPath, true);
                        // 加载专辑封面
                        DisplayImage(destinationPath);
                    }
                    else
                    {
                        DiscConstuct1.Visibility = Visibility.Visible;
                        DiscConstuct2.Visibility = Visibility.Visible;
                    }

                    // 判断是否是默认的文本 如果不是则把颜色重置成黑色的 否则重置成灰色
                    if (loadedData.AlbumName != "Input Album Name Here")
                    {
                        AlbumName.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                        AlbumName.Foreground = System.Windows.Media.Brushes.Gray;
                    }
                    if (loadedData.ArtistName != "Input Artist Name Here")
                    {
                        ArtistName.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                        ArtistName.Foreground = System.Windows.Media.Brushes.Gray;
                    }
                    if (loadedData.ReleaseDate != "YYYY-MM-DD")
                    {
                        ReleaseDate.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                        ReleaseDate.Foreground = System.Windows.Media.Brushes.Gray;
                    }
                    if (loadedData.Num != "0")
                    {
                        Num.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                        Num.Foreground = System.Windows.Media.Brushes.Gray;
                    }
                    if (loadedData.Review != "Write Down Your Review Here")
                    {
                        Review.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                        Review.Foreground = System.Windows.Media.Brushes.Gray;
                    }
                    if (loadedData.MusicGenre != "Split With Comma")
                    {
                        MusicGenre.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                        MusicGenre.Foreground = System.Windows.Media.Brushes.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // 将加载的数据应用到界面上
        private void ApplyLoadedDataToUI(AlbumData loadedData)
        {
            AlbumName.Text = loadedData.AlbumName;
            ArtistName.Text = loadedData.ArtistName;
            ReleaseDate.Text = loadedData.ReleaseDate;
            Num.Text = loadedData.Num;
            Review.Text = loadedData.Review;
            MusicGenre.Text = loadedData.MusicGenre;
            StarredTracksCnt.Text = loadedData.StarredTracksCnt;
            AllTracksCnt.Text = loadedData.AllTracksCnt;

            // 将滑动条位置转换为相应的滑动条值
            double sliderValue = GetSliderValueFromPosition(loadedData.SliderPosition);
            CorrectionSlider.Value = sliderValue;

            // 将专辑封面路径保存的图片显示到AlbumCover框内
            DisplayImage(loadedData.AlbumCoverPath);
        }

        // 将滑动条挡位从字符串转回数字
        private double GetSliderValueFromPosition(string position)
        {
            switch (position)
            {
                case "Low":
                    return -1;
                case "Medium":
                    return 0;
                case "High":
                    return 1;
                default:
                    return 0;
            }
        }

        // 选择并展示专辑封面
        private void AlbumCover_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 弹出文件选择对话框，让用户选择图片文件
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png; *.jpg; *.jpeg; *.webp; *.bmp)|*.png;*.jpg;*.jpeg;*.webp;*.bmp|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // 获取用户选择的图片文件路径
                string originalCoverPath = openFileDialog.FileName;

                string tempDirectory = @"D:\album_review\temp";
                string albumCoverDirectory = @"D:\album_review\cover";

                string fileName = Path.GetFileName(originalCoverPath);

                // !!!必须先输入专辑名、艺人名、第x张
                string albumName = AlbumName.Text;
                string albumNumber = Num.Text;
                string artistName = ArtistName.Text;

                // 获取文件扩展名
                string fileExtension = Path.GetExtension(originalCoverPath);

                // 构造新的文件名
                string newFileName = $"{artistName}_{albumNumber}_{albumName}{fileExtension}";

                // 构造临时文件和专辑封面文件的路径
                string tempCoverPath = Path.Combine(tempDirectory, newFileName);
                string albumCoverPath = Path.Combine(albumCoverDirectory, newFileName);

                //todo::把复制到albumcoverpath里的图片的文件名改了，现在是cover.jpg
                File.Copy(originalCoverPath, tempCoverPath, true);
                File.Copy(originalCoverPath, albumCoverPath, true);
                // 加载专辑封面
                DisplayImage(tempCoverPath);
            }
        }

        // 展示专辑封面
        private void DisplayImage(string imagePath)
        {
            // 将用于模拟黑胶的控件设置为隐藏
            DiscConstuct1.Visibility = Visibility.Collapsed;
            DiscConstuct2.Visibility = Visibility.Collapsed;

            // 创建BitmapImage并设置源
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath);
            bitmapImage.EndInit();

            // 创建ImageBrush并设置源
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = bitmapImage;

            // 在AlbumCover中显示图片
            AlbumCover.Background = imageBrush;
        }

        // 清空专辑名称
        private void ClearAlbumName_Click(object sender, RoutedEventArgs e)
        {
            AlbumName.Text = "Input Album Name Here";
            AlbumName.Foreground = System.Windows.Media.Brushes.Gray;
        }

        // 格式化专辑名称
        private void FormatAlbumName_Click(object sender, RoutedEventArgs e)
        {
            // Creates a TextInfo based on the "en-US" culture
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            AlbumName.Text = myTI.ToTitleCase(AlbumName.Text);
        }

        // 清空艺术家名称
        private void ClearArtistName_Click(object sender, RoutedEventArgs e)
        {
            ArtistName.Text = "Input Artist Name Here";
            ArtistName.Foreground = System.Windows.Media.Brushes.Gray;
        }

        // 格式化艺术家名称
        private void FormatArtistName_Click(object sender, RoutedEventArgs e)
        {
            // Creates a TextInfo based on the "en-US" culture
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            ArtistName.Text = myTI.ToTitleCase(ArtistName.Text);
        }

        // 清空发行日期和第x张
        private void ClearDateAndNum_Click(object sender, RoutedEventArgs e)
        {
            ReleaseDate.Text = "YYYY-MM-DD";
            ReleaseDate.Foreground = System.Windows.Media.Brushes.Gray;
            Num.Text = "0";
            Num.Foreground = System.Windows.Media.Brushes.Gray;
        }

        // 格式化发行日期
        private void FormatDate_Click(object sender, RoutedEventArgs e)
        {
            string inputDate = ReleaseDate.Text;

            // 定义允许的日期格式数组
            string[] formats = { "yyyy-MM-d", "yyyy-M-dd", "yyyy-M-d", "yyyy-MM-dd" };

            // 解析输入的日期字符串
            DateTime date;
            if (DateTime.TryParseExact(inputDate, formats, null, DateTimeStyles.None, out date))
            {
                // 格式化日期字符串为指定格式
                string formattedDate = date.ToString("yyyy-MM-dd");
                ReleaseDate.Text = formattedDate;
            }
        }

        // 文本框焦点
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string placeholderText = GetPlaceholderText(textBox);
            if (textBox.Text == placeholderText)
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        // 文本框失焦
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string placeholderText = GetPlaceholderText(textBox);
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholderText;
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        // 获取对应默认文本
        private string GetPlaceholderText(TextBox textBox)
        {
            switch (textBox.Name)
            {
                case "AlbumName":
                    return "Input Album Name Here";
                case "ArtistName":
                    return "Input Artist Name Here";
                case "ReleaseDate":
                    return "YYYY-MM-DD";
                case "Num":
                    return "0";
                case "Review":
                    return "Write Down Your Review Here";
                case "MusicGenre":
                    return "Split With Comma";
                default:
                    return "";
            }
        }

        // 生成并保存图片
        private string GenerateAndSaveImage(AlbumData albumData)
        {
            try
            {
                // 创建指定宽度和高度的新位图
                Bitmap bitmap = new Bitmap(1200, 1700);

                // 从位图创建一个图形对象
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // 使用白色背景清除图形
                    graphics.Clear(System.Drawing.Color.White);

                    // 定义用于绘制文本的字体和刷子
                    Font albumFont = new Font(new System.Drawing.FontFamily("Times New Roman"), 24, System.Drawing.FontStyle.Bold);
                    Font artistFont = new Font(new System.Drawing.FontFamily("Times New Roman"), 20, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic);
                    Font gradeFont = new Font(new System.Drawing.FontFamily("Tiempos Headline Light"), 20);
                    Font genreFont = new Font(new System.Drawing.FontFamily("Tiempos Headline Light"), 20);
                    Font reviewFont = new Font(new System.Drawing.FontFamily("霞鹜文楷"), 20);

                    SolidBrush brush = new SolidBrush(System.Drawing.Color.Black);

                    // 在图像上绘制专辑信息
                    // 绘制专辑封面
                    if (!string.IsNullOrEmpty(albumData.AlbumCoverPath))
                    {
                        using (Image albumCover = Image.FromFile(albumData.AlbumCoverPath))
                        {
                            graphics.DrawImage(albumCover, new Rectangle(100, 100, 1000, 1000));
                        }
                    }

                    // 计算总分和评级
                    double totalScore = CalculateTotalScore(albumData);
                    string alphaGrade = CalculateGrade(totalScore);

                    // 合成创作者和发行年份
                    string artistAndReleaseDate = albumData.ArtistName + ", " + albumData.ReleaseDate[..4];

                    // 合成“Rating: ”和评级
                    string grade = "Rating: " + alphaGrade;

                    // 合成“Genre: ”和风格
                    string genre = "Genre: " + albumData.MusicGenre;

                    // 合成Rating和Genre
                    string overallRating = grade + "    " + genre;

                    // 计算文本大小
                    SizeF albumNameSize = graphics.MeasureString(albumData.AlbumName, albumFont);
                    SizeF artistNameSize = graphics.MeasureString(artistAndReleaseDate, artistFont);
                    //SizeF gradeSize = graphics.MeasureString(grade, gradeFont);
                    //SizeF genreSize = graphics.MeasureString(genre, genreFont);
                    SizeF overallRatingSize = graphics.MeasureString(overallRating, gradeFont);

                    // 计算居中显示文本需要的位置
                    float albumNameX = 100 + (1000 - albumNameSize.Width) / 2; // 左x值
                    float albumNameY = 1150; // 100 + 1000 + 25（100为专辑封面上方的间距）
                    float artistNameX = 100 + (1000 - artistNameSize.Width) / 2; // 左x值
                    float artistNameY = albumNameY + albumNameSize.Height + 15; // 专辑名下方间距15
                    float overallRatingX = 100 + (1000 - overallRatingSize.Width) / 2; // 左x值
                    float overallRatingY = 1600; // 1700 - 100
                    //float gradeX = 125; // 100 + 25
                    //float gradeY = 1300; // 1150 + 150
                    //float genreX = 1075 - genreSize.Width; // 右侧留125
                    //float genreY = 1300; // 1150 + 150

                    // 绘制专辑名、艺术家名和发行日期
                    graphics.DrawString(albumData.AlbumName, albumFont, brush, new PointF(albumNameX, albumNameY));
                    graphics.DrawString(artistAndReleaseDate, artistFont, brush, new PointF(artistNameX, artistNameY));

                    // 绘制评级和风格
                    //graphics.DrawString(grade, gradeFont, brush, new PointF(gradeX, gradeY));
                    //graphics.DrawString(genre, genreFont, brush, new PointF(genreX, genreY));
                    graphics.DrawString(overallRating, gradeFont, brush, new PointF(overallRatingX, overallRatingY));

                    // 绘制简评
                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    stringFormat.Trimming = StringTrimming.Word;
                    stringFormat.FormatFlags = StringFormatFlags.LineLimit; // 设置自动换行

                    RectangleF reviewRectangle = new RectangleF(150, 1330, 900, 200); // 指定简评的矩形区域
                    graphics.DrawString(albumData.Review, reviewFont, brush, reviewRectangle, stringFormat);

                    // 绘制上下边框
                    System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black); // 创建画笔，颜色为黑色
                    // 绘制矩形边框上边线
                    graphics.DrawLine(pen, reviewRectangle.Left, reviewRectangle.Top, reviewRectangle.Right, reviewRectangle.Top);
                    // 绘制矩形边框下边线
                    graphics.DrawLine(pen, reviewRectangle.Left, reviewRectangle.Bottom, reviewRectangle.Right, reviewRectangle.Bottom);

                    // 释放资源
                    pen.Dispose();

                    // 构建文件名
                    string fileName = $"{albumData.ArtistName}_{albumData.Num}_{albumData.AlbumName}.png";

                    // 将位图保存为文件
                    string imagePath = Path.Combine(@"D:\album_review\output", fileName);
                    bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

                    // 释放资源
                    albumFont.Dispose();
                    reviewFont.Dispose();
                    brush.Dispose();

                    // 返回生成图像的路径
                    return imagePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误: {ex.Message}");
                return string.Empty;
            }
        }

        // 计算总分
        private double CalculateTotalScore(AlbumData albumData)
        {
            double starredTracks = double.Parse(albumData.StarredTracksCnt);
            double allTracks = double.Parse(albumData.AllTracksCnt);
            double correctionFactor = GetSliderValueFromPosition(albumData.SliderPosition);

            // 计算总分
            double totalScore = (starredTracks / allTracks) * 10 + correctionFactor + 2;

            // 确保总分不超过10
            totalScore = Math.Min(totalScore, 10.0);

            return totalScore;
        }

        // 根据总分计算评级
        private string CalculateGrade(double totalScore)
        {
            if (totalScore >= 9.5)
            {
                return "A+";
            }
            else if (totalScore >= 9.0)
            {
                return "A";
            }
            else if (totalScore >= 8.5)
            {
                return "A-";
            }
            else if (totalScore >= 8.0)
            {
                return "B+";
            }
            else if (totalScore >= 7.5)
            {
                return "B";
            }
            else if (totalScore >= 7.0)
            {
                return "B-";
            }
            else if (totalScore >= 6.5)
            {
                return "C+";
            }
            else if (totalScore >= 6.0)
            {
                return "C";
            }
            else
            {
                return "Dud";
            }
        }
    }
}
