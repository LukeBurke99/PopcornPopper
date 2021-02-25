using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Threading;

namespace PopcornPopper
{
	public partial class MainWindow : Window
	{
		private double ApplicationWidth { get; } = 0;
		private DispatcherTimer MovePopcornTimer { get; set; }
		private DispatcherTimer DisplayPopcornTimer { get; set; }
		private Image[] PopcornPieces { get; }
		private List<double> DistanceH { get; set; }
		private List<double> DistanceV { get; set; }
		private List<int> Direction { get; set; }


		private int secondsEllapsed = 1;

		/// <summary>
		/// Initialise the Popcorn Popper application to fling popcorn across your screen.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			// Save the popcorn images into the list
			PopcornPieces = new Image[] { Image_Popcorn, Image_Popcorn2, Image_Popcorn3, Image_Popcorn4 };

			// Loop over all screens and work out what the length of the application should be
			try {
				double? minimumScreenLeft = null;
				double? minimumScreenHeight = null;
				for (int i = 0; i < Screen.AllScreens.Length; i++) {
					System.Drawing.Rectangle screenBounds = Screen.AllScreens[i].WorkingArea;
					ApplicationWidth += screenBounds.Width;

					// Asign the left most screen to this variable. Example: -1920
					if (minimumScreenLeft == null || screenBounds.Left < minimumScreenLeft) {
						minimumScreenLeft = screenBounds.Left;
					}
					if (minimumScreenHeight == null || screenBounds.Height < minimumScreenHeight) {
						minimumScreenHeight = screenBounds.Height;
					}
				}
				// Set the bounds of the application so that popcorn can be shown on all screens
				this.Left = minimumScreenLeft ?? 0;
				this.Width = ApplicationWidth;
				this.Height = minimumScreenHeight ?? 800;

				// Move all of the popcorn images to below the screen
				foreach (Image p in PopcornPieces) {
					Canvas.SetTop(p, this.Height + 80);
				}

				// Set a timer to display the popcorn every 10 seconds
				DisplayPopcornTimer = new DispatcherTimer {
					Interval = new TimeSpan(0, 0, 10)
				};
				DisplayPopcornTimer.Tick += DisplayPopcorn;
				DisplayPopcornTimer.Start();
			} catch (Exception ex) {
				Console.WriteLine("Error calculating screen width." + ex.Message);
			}
		}

		/// <summary>
		/// Loop over all of the popcorn images and set random positions for each one.
		/// Randomly generate the height and width that the popcorn will be flung and select which way the popcorn is going to fling (left or right).
		/// </summary>
		/// <param name="sender">DispatcherTimer</param>
		/// <param name="e">The event args</param>
		private void DisplayPopcorn(object sender, EventArgs e)
		{
			Direction = new List<int>();
			DistanceH = new List<double>();
			DistanceV = new List<double>();
			Random rnd = new Random();

			// Loop over all of the popcorn images and set a random position and choose which way to fling them
			for (int i = 0; i < PopcornPieces.Length; i++) {
				// Select a random position on the screens and place the popcorn there
				Canvas.SetLeft(PopcornPieces[i], rnd.Next(80, (int)ApplicationWidth - 80));
				// select a random direction for the popcorn to fling
				Direction.Add(rnd.Next(2));
				// select a random distance for the popcorn to fling
				DistanceH.Add(rnd.Next(20, 35) / 10);
				DistanceV.Add(rnd.Next(7, 15));
			}

			// Setup the timer to move the popcorns
			MovePopcornTimer = new DispatcherTimer {
				Interval = new TimeSpan(0, 0, 0, 0, 15)
			};
			MovePopcornTimer.Tick += MovePopcorn;
			MovePopcornTimer.Start();
		}

		/// <summary>
		/// Animate the popcorn so that it is flinging across the screen.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MovePopcorn(object sender, EventArgs e)
		{
			double topChange;
			// Keep moving the popcorn until it has been 1 second
			if (secondsEllapsed < 1000) {

				// Loop over each piece of popcorn and aniate them
				for (int i = 0; i < PopcornPieces.Length; i++) {
					// Work out how much to change the top position by
					topChange = DistanceH[i] * (10 - (secondsEllapsed / 47));
					Canvas.SetTop(PopcornPieces[i], Canvas.GetTop(PopcornPieces[i]) - topChange);

					// Work out how much to rotate the image by
					double rotationInDegrees = 6;
					RotateTransform rotation = PopcornPieces[i].RenderTransform as RotateTransform;


					if (Direction[i] == 1) {
						// Move the piece of popcorn right
						Canvas.SetLeft(PopcornPieces[i], Canvas.GetLeft(PopcornPieces[i]) + DistanceV[i]);

						// Rotate the image clockwise
						if (rotation != null) {
							rotationInDegrees = rotation.Angle + DistanceV[i];
						}
					} else {
						// Move the piece of popcorn left
						Canvas.SetLeft(PopcornPieces[i], Canvas.GetLeft(PopcornPieces[i]) - DistanceV[i]);

						// Rotate the image anti-clockwise
						if (rotation != null) {
							rotationInDegrees = rotation.Angle - DistanceV[i];
						}
					}
					PopcornPieces[i].RenderTransform = new RotateTransform(rotationInDegrees);
				}
				secondsEllapsed += 10;
			} else {
				// The images have rotated enough so reset their original possitions (off screen)
				foreach (var item in PopcornPieces) {
					Canvas.SetTop(item, this.Height + 80);
				}

				// Reset the timers
				secondsEllapsed = 1;
				MovePopcornTimer.Stop();
				DisplayPopcornTimer.Stop();
				DisplayPopcornTimer.Start();
			}
		}
	}
}
