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

		private OpenCvSharp.VideoCapture video = new OpenCvSharp.VideoCapture();
		private bool isStopRequest = false;

		private void OpenVideoClicked(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

			dialog.Filter = "Video|*.*";

			if (dialog.ShowDialog().GetValueOrDefault())
			{
				if (video.Open(dialog.FileName) == false)
				{
					MessageBox.Show($"Video load failed.");
				}
				else
				{
					MaxFrame = video.FrameCount;

					OpenCvSharp.Mat image = new OpenCvSharp.Mat();

					video.PosFrames = video.FrameCount / 2;

					video.Read(image);

					ImageSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(image);
				}
			}
		}

		private async void PlayButtonClicked(object sender, RoutedEventArgs e)
		{
			OpenCvSharp.Mat image = new OpenCvSharp.Mat();

			int currentFrame = 0;

			isStopRequest = false;

			video.PosFrames = 0;

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

		public bool IsDisplayROI
		{
			get { return (bool)GetValue(IsDisplayROIProperty); }
			set { SetValue(IsDisplayROIProperty, value); }
		}

		private static readonly DependencyProperty IsDisplayROIProperty =
			DependencyProperty.Register(nameof(IsDisplayROI), typeof(bool), typeof(MainWindow), new PropertyMetadata());

		/// <summary>
		/// The rectangle on screen.
		/// </summary>
		private System.Drawing.Rectangle InternalROI { get; set; }

		private bool _moving = false;
		private bool _resizing = false;
		private System.Windows.Point startPoint;

		private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var point = e.GetPosition(ImageCanvas);
			var offset = VisualTreeHelper.GetOffset(ImageCanvas);

			if (point.X < 0 || point.Y < 0 || ImageCanvas.ActualWidth < point.X || ImageCanvas.ActualHeight < point.Y)
			{
				return;
			}
			else if (InternalROI.X + 10 <= point.X && point.X <= InternalROI.Right - 10 && InternalROI.Y + 10 <= point.Y && point.Y <= InternalROI.Bottom - 10)
			{
				_moving = true;
				startPoint = point;
			}
			else
			{
				_resizing = true;

				InternalROI = new System.Drawing.Rectangle((int)point.X, (int)point.Y, 0, 0);
			}
		}

		private void CanvasDraw_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			rectCanvas.Width = 0;
			rectCanvas.Height = 0;

			_resizing = false;
			_moving = false;

			InternalROI = System.Drawing.Rectangle.Empty;
		}

		private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_resizing = false;
			_moving = false;
		}

		private void Canvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (IsDisplayROI == false) return;

			var point = e.GetPosition(ImageCanvas);
			var offset = VisualTreeHelper.GetOffset(ImageCanvas);

			if (point.X < 0 || point.Y < 0 || ImageCanvas.ActualWidth < point.X || ImageCanvas.ActualHeight < point.Y)
			{
				Cursor = Cursors.Arrow;
				_moving = _resizing = false;
				return;
			}
			else if (InternalROI.X + 1 <= point.X && point.X <= InternalROI.Right - 1 && InternalROI.Y + 1 <= point.Y && point.Y <= InternalROI.Bottom - 1)
			{
				// Inner of ROI
				Cursor = Cursors.Hand;
			}
			else
			{
				// Outer of ROI
				Cursor = Cursors.Cross;
			}

			// Mouse point guide
			lineCanvasWidth.X1 = 0;
			lineCanvasWidth.X2 = ImageCanvas.ActualWidth;
			lineCanvasWidth.Y1 = lineCanvasWidth.Y2 = point.Y;

			lineCanvasHeight.X1 = lineCanvasHeight.X2 = point.X;
			lineCanvasHeight.Y1 = 0;
			lineCanvasHeight.Y2 = ImageCanvas.ActualHeight;

			if (_resizing)
			{
				int width = (int)(point.X - InternalROI.X);
				int height = (int)(point.Y - InternalROI.Y);
				rectCanvas.Width = width >= 0 ? width : 0;
				rectCanvas.Height = height >= 0 ? height : 0;
				Canvas.SetLeft(rectCanvas, InternalROI.X + offset.X);
				Canvas.SetTop(rectCanvas, InternalROI.Y + offset.Y);

				InternalROI = new System.Drawing.Rectangle(InternalROI.X, InternalROI.Y, (int)rectCanvas.Width, (int)rectCanvas.Height);
			}
			else if (_moving)
			{
				if (point.X <= startPoint.X)
				{
					if (0 <= InternalROI.X - (startPoint.X - point.X))
					{
						Canvas.SetLeft(rectCanvas, InternalROI.X - (startPoint.X - point.X) + offset.X);
						InternalROI = new System.Drawing.Rectangle((int)(InternalROI.X - (startPoint.X - point.X)), InternalROI.Y, InternalROI.Width, InternalROI.Height);
					}
				}
				if (point.Y <= startPoint.Y)
				{
					if (0 <= InternalROI.Y - (startPoint.Y - point.Y))
					{
						Canvas.SetTop(rectCanvas, InternalROI.Y - (startPoint.Y - point.Y) + offset.Y);
						InternalROI = new System.Drawing.Rectangle(InternalROI.X, (int)(InternalROI.Y - (startPoint.Y - point.Y)), InternalROI.Width, InternalROI.Height);
					}
				}
				if (startPoint.X <= point.X)
				{
					if (InternalROI.Right + (point.X - startPoint.X) <= ImageCanvas.ActualWidth)
					{
						Canvas.SetLeft(rectCanvas, InternalROI.X + (point.X - startPoint.X) + offset.X);
						InternalROI = new System.Drawing.Rectangle((int)(InternalROI.X + (point.X - startPoint.X)), InternalROI.Y, InternalROI.Width, InternalROI.Height);
					}
				}
				if (startPoint.Y <= point.Y)
				{
					if (InternalROI.Bottom + (point.Y - startPoint.Y) <= ImageCanvas.ActualHeight)
					{
						Canvas.SetTop(rectCanvas, InternalROI.Y + (point.Y - startPoint.Y) + offset.Y);
						InternalROI = new System.Drawing.Rectangle(InternalROI.X, (int)(InternalROI.Y + (point.Y - startPoint.Y)), InternalROI.Width, InternalROI.Height);
					}
				}
				startPoint = point;
			}
		}

		private void CanvasDraw_MouseEnter(object sender, MouseEventArgs e)
		{
			lineCanvasWidth.StrokeThickness = 1;
			lineCanvasHeight.StrokeThickness = 1;
		}

		private void CanvasDraw_MouseLeave(object sender, MouseEventArgs e)
		{
			Cursor = Cursors.Arrow;

			lineCanvasWidth.StrokeThickness = 0;
			lineCanvasHeight.StrokeThickness = 0;
		}
		/// <summary>
		/// Transfer the screen-coordination to the image-coordination.
		/// </summary>
		/// <param name="internalROI"></param>
		/// <returns></returns>
		private System.Drawing.Rectangle GetROIForExternal(System.Drawing.Rectangle internalROI)
		{
			return new System.Drawing.Rectangle(
				(int)Math.Round(internalROI.X * (ImageSource.Width / ImageCanvas.ActualWidth)), // X
				(int)Math.Round(internalROI.Y * (ImageSource.Height / ImageCanvas.ActualHeight)), // Y
				(int)Math.Round(internalROI.Width * (ImageSource.Width / ImageCanvas.ActualWidth)), // Width
				(int)Math.Round(internalROI.Height * (ImageSource.Height / ImageCanvas.ActualHeight))); // Height
		}

		/// <summary>
		/// Transfer the image-cordination to the screen-coordination.
		/// </summary>
		/// <param name="externalROI"></param>
		/// <returns></returns>
		private System.Drawing.Rectangle GetROIForInternal(System.Drawing.Rectangle externalROI)
		{
			return new System.Drawing.Rectangle(
				(int)Math.Round(externalROI.X / (ImageSource.Width / ImageCanvas.ActualWidth)), // X
				(int)Math.Round(externalROI.Y / (ImageSource.Height / ImageCanvas.ActualHeight)), // Y
				(int)Math.Round(externalROI.Width / (ImageSource.Width / ImageCanvas.ActualWidth)), // Width
				(int)Math.Round(externalROI.Height / (ImageSource.Height / ImageCanvas.ActualHeight))); // Height
		}
	}
}
