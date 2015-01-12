using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CraigerEightOhEighter.Annotations;

namespace CraigerEightOhEighter.Views
{
    /// <summary>
    /// Interaction logic for DrumOn.xaml
    /// </summary>
    public partial class DrumOn : INotifyPropertyChanged, IDrumButton
    {
        public DrumOn()
        {
            InitializeComponent();
        }
        public bool Clicked { get; set; }
        public void DrumOnClicked(object sender, MouseEventArgs e)
        {
            Clicked = true;
            if (ButClick != null)
            {
                ButClick(this, new RoutedEventArgs(e.RoutedEvent));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event RoutedEventHandler ButClick;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ButtonName { get; set; }
        public int SequenceNumber { get; set; }
        public int Track { get; set; }
    }
}
