using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using CraigerEightOhEighter.Properties;
using CraigerEightOhEighter.ViewModels;

namespace CraigerEightOhEighter.Models
{
	public class RythmMachineApp : IDisposable
	{
		private const int TrackLength = 16;
        public IContainer Container { get; set; }
	    public MainUiViewModel MainUiViewModel { get; set; }
        public Dictionary<string, bool[]> CurrentPattern { get; set; } 
		public RythmMachineApp(IAudioPlayer player, MainUiViewModel viewModel)
		{
		    MainUiViewModel = viewModel;
			const int measuresPerBeat = 2;
		    CurrentPattern = new Dictionary<string, bool[]>();
			Mixer = new Mixer(player, measuresPerBeat){Container = viewModel.Container};
			Mixer.Add(new Track("808 Kick", new Patch("bass", Resources.bass), TrackLength));
			Mixer.Add(new Track("808 Snare", new Patch("snare", Resources.snare), TrackLength));
            Mixer.Add(new Track("808 CH", new Patch("ch", Resources.closed), TrackLength));
            Mixer.Add(new Track("808 OH", new Patch("oh", Resources.open), TrackLength));
            Mixer.Add(new Track("808 RS", new Patch("rs", Resources.rim), TrackLength));
            Mixer.Add(new Track("808 CY", new Patch("cy", Resources.crash), TrackLength));
            // Init with any preset
            Mixer["808 Kick"].Init(new byte[] { 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0 });
            Mixer["808 Snare"].Init(new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 });
            Mixer["808 CH"].Init(new byte[] { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 0 });
            Mixer["808 OH"].Init(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 });
		    for (int i = 0; i < Mixer.Count; i++)
		    {
                CurrentPattern.Add(Mixer[i].Name, Mixer[i].Pattern);
		    }
		    if (MainUiViewModel != null)
		    {
                MainUiViewModel.PropertyChanged -= MainUiViewModelPropertyChanged;
		        MainUiViewModel.PropertyChanged += MainUiViewModelPropertyChanged;
		    }
		    // Init with any preset
			Mixer.Bpm = decimal.Parse(viewModel.FullTempo);
			//BuildUi(control);

			_mTimer = new Timer {Interval = 250};
		    _mTimer.Tick += m_Timer_Tick;
			_mTimer.Enabled = true;
		}

        void MainUiViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("Tempo"))
            {
                Mixer.Bpm = MainUiViewModel.TempoMain;
            }
        }

		~RythmMachineApp()
		{
			Dispose();
		}
		public void Dispose()
		{
			if (_mTimer != null)
				_mTimer.Dispose();
			if (Mixer != null)
				Mixer.Dispose();
			GC.SuppressFinalize(this);
		}

		private readonly Timer _mTimer;

		public readonly Mixer Mixer;

		private int CurrentTick
		{
			get { return Mixer.CurrentTick % TrackLength; }
		}


		private void m_Timer_Tick(object sender, EventArgs e)
		{
			if (MainUiViewModel != null)
                MainUiViewModel.CurrentTick = CurrentTick;
		}

		
	}
}
