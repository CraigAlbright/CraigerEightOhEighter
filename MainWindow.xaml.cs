using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;
using Autofac;
using Autofac.Core;
using CraigerEightOhEighter.Helpers;
using CraigerEightOhEighter.Models;
using CraigerEightOhEighter.ViewModels;
using CraigerEightOhEighter.Views;
using Microsoft.Win32;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

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
        public IContainer Container { get; set; }
		private bool _playing;
		private IEnumerable<Track> _tracks;
		public MainWindow()
		{
			InitializeComponent();

		    var diHelper = new DependencyInjectionHelper();
		    Container = diHelper.Container;
            MainUIViewModel = Container.Resolve<MainUiViewModel>();
		    MainUIViewModel.TempoMain = 77;
		    MainUIViewModel.TempoFine = 5;
		    MainUIViewModel.Container = Container;
		    DataContext = MainUIViewModel;
		}

		void OnLoad(object sender, RoutedEventArgs e)
		{
			var windowPtr = new WindowInteropHelper(this).Handle;
            _streamingPlayer = new StreamingPlayer(windowPtr, 22050, 16, 2, Container);
            _mMachine = new RythmMachineApp( _streamingPlayer, MainUIViewModel, Container);
		    _tracks = _mMachine.Mixer.Tracks;
			BuildGridOnUi(_tracks);
			//_handler = ClickHandler;
		}

	    private void ClearUiButtons()
	    {
	        int[] trackNumber = {0};
	        foreach (var activeTrack in _tracks.Select(track => GetActiveTrack(trackNumber[0])))
	        {
	            activeTrack.GridStack.Children.Clear();
	            trackNumber[0]++;
	        }
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
		    return
		        (from object control in controls where control.GetType() == typeof (DrumLane) select control as DrumLane)
		            .FirstOrDefault(drumLane => drumLane != null && drumLane.Name == "Track" + trackNum);
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

        private void SavePattern(object sender, RoutedEventArgs e)
        {
            var serializer = new DataContractJsonSerializer(typeof(Track));
            using (var fs = new FileStream(@"C:\Test.808er", FileMode.OpenOrCreate))
            {
                foreach (var track in _tracks)
                {
                    serializer.WriteObject(fs, track);
                    var encoding = new UTF8Encoding();
                    var splitter = " " + Environment.NewLine + " ";
                    var bytes = encoding.GetBytes(splitter);
                    fs.Write(bytes, 0, bytes.Length);
                }
                fs.Flush(true);
            }
            MessageBox.Show("File Saved Successfully");
        }

        private void LoadPattern(object sender, RoutedEventArgs e)
        {
            ClearUiButtons();
            var fd = new OpenFileDialog();
            var result = fd.ShowDialog();
            if (result == null || (!(bool) result || !fd.CheckFileExists || string.IsNullOrEmpty(fd.FileName))) return;
            _tracks = new List<Track>();
            var ms = new MemoryStream();
            var encoding = new UTF8Encoding();
            using (var sr = new StreamReader(@"C:\Test.808er"))
            {
                while (!sr.EndOfStream)
                {
                    var text = sr.ReadLine();
                    if (text == null || string.IsNullOrEmpty(text.Trim())) continue;
                    var bytes = encoding.GetBytes(text);
                    ms.Write(bytes, 0, bytes.Length);
                    using (
                        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(bytes,
                            XmlDictionaryReaderQuotas.Max))
                    {
                        var outputSerialiser = new DataContractJsonSerializer(typeof (Track));
                        var track = outputSerialiser.ReadObject(jsonReader);
                        if (track == null) continue;
                        var listTracks = _tracks as List<Track>;
                        listTracks.Add((Track) track);
                    }
                }
            }
            BuildGridOnUi(_tracks);
        }

        private void ClearPattern(object sender, RoutedEventArgs e)
        {
            for (var t=0; t< _tracks.ToArray().Count(); t++)
            {
                var track = _tracks.ToArray()[t];
                for (var i = 0; i < track.Pattern.Length; i++)
                {
                    track.Pattern[i] = false;
                }
                var trackGrid = GetActiveTrack(t);
                trackGrid.GridStack.Children.Clear();
                RebuildTrackUi(t);
            }
           
        }

	}
}
