using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using CraigerEightOhEighter.Models;
using CraigerEightOhEighter.ViewModels;
using CraigerEightOhEighter.Views;

namespace CraigerEightOhEighter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private StreamingPlayer _streamingPlayer;
		private RythmMachineApp _mMachine;
        // ReSharper disable once InconsistentNaming
	    public MainUiViewModel MainUIViewModel { get; private set; }
		private bool _playing;
		private IEnumerable<Track> _tracks;
		public MainWindow()
		{
			InitializeComponent();
            MainUIViewModel = new MainUiViewModel(77) { TempoFine = 5 };
		    DataContext = MainUIViewModel;
		    //DataContext = MainUIViewModel;
		    //var binding = new Binding {Source = MainUIViewModel.FullTempo, Mode = BindingMode.TwoWay};
		    //TempoBox.SetBinding(TextBox.TextProperty,binding);
		}

		//private PropertyChangedEventHandler _handler;

		void OnLoad(object sender, RoutedEventArgs e)
		{
			var windowPtr = new WindowInteropHelper(this).Handle;
			_streamingPlayer = new StreamingPlayer(windowPtr, 22050, 16, 2);
            _mMachine = new RythmMachineApp(windowPtr, _streamingPlayer, MainUIViewModel);
		    _tracks = _mMachine.Mixer.Tracks;
			BuildGridOnUi(_tracks);
			//_handler = ClickHandler;
		}

        
		private void BuildGridOnUi(IEnumerable<Track> tracks)
		{
			var trackNumber = 0;
			var tickNumber = 0;
			
			foreach (var track in tracks)
			{
				var activeTrack = GetActiveTrack(trackNumber);
				foreach (var tick in track.Pattern)
				{
					if (tick && activeTrack != null)
					{
					    var drumOn = (DrumOn)GetDrumButton(trackNumber, tickNumber, ButtonType.On);
						drumOn.ButClick += ButtonClicked;
						activeTrack.GridStack.Children.Add(drumOn);
					}
					else if(activeTrack != null)
					{
					    var drumBut = (DrumButton)GetDrumButton(trackNumber, tickNumber, ButtonType.Off);
						drumBut.ButClick += ButtonClicked;
						activeTrack.GridStack.Children.Add(drumBut);
					}
                    tickNumber++;
				    if (tickNumber == track.Pattern.Length)
				        tickNumber = 0;
				}
				
				trackNumber++;
			}

		}

        private void RebuildTrackUi(int trackNumber)
        {
            var tickNumber = 0;
            var track = _tracks.ToArray()[trackNumber];
            var activeTrack = GetActiveTrack(trackNumber);
            foreach (var tick in track.Pattern)
            {
                if (tick && activeTrack != null)
                {
                    var drumOn = (DrumOn)GetDrumButton(trackNumber, tickNumber, ButtonType.On);
                    drumOn.ButClick += ButtonClicked;
                    activeTrack.GridStack.Children.Add(drumOn);
                }
                else if (activeTrack != null)
                {
                    var drumBut = (DrumButton)GetDrumButton(trackNumber, tickNumber, ButtonType.Off);
                    drumBut.ButClick += ButtonClicked;
                    activeTrack.GridStack.Children.Add(drumBut);
                }
                tickNumber++;
                if (tickNumber == track.Pattern.Length)
                    tickNumber = 0;
            }
	    }

	    private static IDrumButton GetDrumButton(int trackNumber, int tickNumber, ButtonType buttonType)
	    {
	        switch (buttonType)
	        {
                    
	            case ButtonType.Off:
	            {
                    RadialGradientBrush gradientBrush = null;
                    var stopCollection = new GradientStopCollection();
	                if (tickNumber < 4)
	                {
                        stopCollection.Add(new GradientStop(Color.FromRgb(222, 25, 25),0.0f));
                        stopCollection.Add(new GradientStop(Color.FromRgb(255, 5, 25),0.85f));
	                    gradientBrush = new RadialGradientBrush(stopCollection);
	                }
                    if (tickNumber >= 4 && tickNumber < 8)
                    {
                        stopCollection.Add(new GradientStop(Color.FromRgb(255, 175, 5), 0.0f));
                        stopCollection.Add(new GradientStop(Color.FromRgb(255, 75, 75), 0.85f));
                        gradientBrush = new RadialGradientBrush(stopCollection);   
                    }
                    if (tickNumber >= 8 && tickNumber < 12)
                    {
                        stopCollection.Add(new GradientStop(Color.FromRgb(210, 190, 85), 0.0f));
                        stopCollection.Add(new GradientStop(Color.FromRgb(210, 215, 75), 0.85f));
                        gradientBrush = new RadialGradientBrush(stopCollection);
                    }
                    if (tickNumber >= 12)
                    {
                        stopCollection.Add(new GradientStop(Color.FromRgb(255, 255, 255), 0.0f));
                        stopCollection.Add(new GradientStop(Color.FromRgb(255, 235, 225), 0.85f));
                        gradientBrush = new RadialGradientBrush(stopCollection);
                        
                    }
	                var button = new DrumButton
	                {
	                    Visibility = Visibility.Visible,
	                    Name = "button" + tickNumber,
	                    Width = 40,
	                    Height = 40,
	                    Margin = new Thickness(5),
	                    Track = trackNumber,
	                    SequenceNumber = tickNumber,
	                    BackGrid = {Background = gradientBrush}
	                };
	                return button;
	            }
	            case ButtonType.On:
	            {
	                return new DrumOn
	                {
	                    Visibility = Visibility.Visible,
	                    Name = "button" + tickNumber,
	                    Width = 40,
	                    Height = 40,
	                    Margin = new Thickness(5),
	                    Track = trackNumber,
	                    SequenceNumber = tickNumber

	                };
	            }
	            default:
	                return null;
	        }
	    }

	    private void ButtonClicked(object sender, RoutedEventArgs e)
		{
			var button = sender as IDrumButton;
		    if (button == null) return;
		    var parentTrack = button.Track;
		    var buttonSequence = button.SequenceNumber;
		    var track = _tracks.ToArray()[parentTrack];
		    var butCast = button as DrumOn;
		    track[buttonSequence] = butCast == null;
		    var trackGrid = GetActiveTrack(parentTrack);
            trackGrid.GridStack.Children.Clear();
            RebuildTrackUi(parentTrack);

		}

		private DrumLane GetActiveTrack(int trackNumber)
		{
			var controls = DrumGridHolder.Children;
			var trackNum = trackNumber + 1;
			return (from object control in controls where control.GetType() == typeof (DrumLane) select control as DrumLane).FirstOrDefault(drumLane => drumLane != null && drumLane.Name == "Track" + trackNum);
		}

		private void PlayStop(object sender, RoutedEventArgs e)
		{
		    if (!_playing)
		    {
		        _mMachine.Mixer.Play();
		        PlayBut.Content = "Stop";
		        _playing = true;
		    }
		    else
		    {
		        _playing = false;
		        _mMachine.Mixer.Stop();
                PlayBut.Content = "Play";
		    }
		}

        private void MainTempoClicked(object sender, RoutedEventArgs e)
        {
            var bindingExpression = BindingOperations.GetBindingExpressionBase(
               TempoBox, TextBox.TextProperty);
            var button = sender as Button;
            if (button == null)
                return;
            if (button.Content.ToString() == "^")
            {
                MainUIViewModel.SetMainTempo(MainUIViewModel.TempoMain + 1);
                if (bindingExpression != null)
                    bindingExpression.UpdateTarget();
                return;
            }
            MainUIViewModel.SetMainTempo(MainUIViewModel.TempoMain - 1);

            if (bindingExpression != null)
                bindingExpression.UpdateTarget();
        }

        private void FineTempoClicked(object sender, RoutedEventArgs e)
        {
            var bindingExpression = BindingOperations.GetBindingExpressionBase(
                TempoBox, TextBox.TextProperty);
            var button = sender as Button;
            if (button == null)
                return;
            if (button.Content.ToString() == "^")
            {
                MainUIViewModel.SetFineTempo(MainUIViewModel.TempoFine + 1);
                if (bindingExpression != null)
                    bindingExpression.UpdateTarget();
                return;
            }
            MainUIViewModel.SetFineTempo(MainUIViewModel.TempoFine - 1);

            if (bindingExpression != null)
                bindingExpression.UpdateTarget();
        }

        private void TempoChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            decimal test;
            if (!decimal.TryParse(textBox.Text, out test))
                return;
            if(MainUIViewModel == null)
                return;
            MainUIViewModel.FullTempo = test.ToString(CultureInfo.InvariantCulture);
        }

	}
}
