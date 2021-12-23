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

		private async void PlayButtonClicked(object sender, RoutedEventArgs e)
		{
			OpenCvSharp.VideoCapture video = new OpenCvSharp.VideoCapture("sample1.mp4");

			OpenCvSharp.Mat image = new OpenCvSharp.Mat();

			while (video.Read(image))
			{
				ImageSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(image);
				await Task.Delay(1);
			}

		}

		public BitmapSource ImageSource
		{
			get { return (BitmapSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

		private static readonly DependencyProperty ImageSourceProperty =
			DependencyProperty.Register(nameof(ImageSource), typeof(BitmapSource), typeof(MainWindow), new PropertyMetadata());

	}
}
