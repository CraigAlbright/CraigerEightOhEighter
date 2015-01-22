using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using CraigerEightOhEighter.Annotations;

namespace CraigerEightOhEighter.ViewModels
{
    public class MainUiViewModel : INotifyPropertyChanged, IMainUiViewModel
    {
        public MainUiViewModel(int tempoInit)
        {
            TempoMain = tempoInit;
        }

        public MainUiViewModel()
        {
            TempoMain = 80;
        }
        public int CurrentTick { get; set; }
        public int TempoMain { get; set; }
        public int TempoFine { get; set; }
        public string FullTempo
        {
            get
            {
                var number = TempoMain +
                             CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator +
                             TempoFine;
                var doubleNumber = Double.Parse(number);
                return doubleNumber.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                var val = value.Split('.');
                if (val.Length > 0)
                {
                    int main;
                    int.TryParse(val[0], out main);
                    TempoMain = main;
                    if (val.Length > 1)
                    {
                        int fine;
                        int.TryParse(val[1], out fine);
                        TempoFine = fine;
                    }
                }
                OnPropertyChanged();
            }
        }

        public void SetMainTempo(int main)
        {
            TempoMain = main < 0 ? 0 : main;
            OnPropertyChanged();
        }

        public void SetFineTempo(int fine)
        {
            if (fine < 0)
            {
                TempoFine = 9;
                TempoMain--;
                OnPropertyChanged();
                return;
            }
            if (fine > 9)
            {
                TempoFine = 0;
                TempoMain++;
                OnPropertyChanged();
                return;
            }
            TempoFine = fine;
            OnPropertyChanged();
           
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
