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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KeepBeingCenter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private bool isStopRequest = false;
		private async void PlayButtonClicked(object sender, RoutedEventArgs e)
		{
			OpenCvSharp.VideoCapture video = new OpenCvSharp.VideoCapture("sample1.mp4");

			MaxFrame = video.FrameCount;

			OpenCvSharp.Mat image = new OpenCvSharp.Mat();

			int currentFrame = 0;

			while (video.Read(image))
			{
				PresentFrame = currentFrame++;
				ImageSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(image);
				
				if (currentFrame > MaxFrame || isStopRequest) break;

				await Task.Delay(1);
			}
		}

		private void StopButtonClicked(object sender, RoutedEventArgs e)
		{
			isStopRequest = true;
		}

		public BitmapSource ImageSource
		{
			get { return (BitmapSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

		private static readonly DependencyProperty ImageSourceProperty =
			DependencyProperty.Register(nameof(ImageSource), typeof(BitmapSource), typeof(MainWindow), new PropertyMetadata());

		public int PresentFrame
		{
			get { return (int)GetValue(PresentFrameProperty); }
			set { SetValue(PresentFrameProperty, value); }
		}

		private static readonly DependencyProperty PresentFrameProperty =
			DependencyProperty.Register(nameof(PresentFrame), typeof(int), typeof(MainWindow), new PropertyMetadata());

		public int MaxFrame
		{
			get { return (int)GetValue(MaxFrameProperty); }
			set { SetValue(MaxFrameProperty, value); }
		}

		private static readonly DependencyProperty MaxFrameProperty =
			DependencyProperty.Register(nameof(MaxFrame), typeof(int), typeof(MainWindow), new PropertyMetadata());
	}
}
