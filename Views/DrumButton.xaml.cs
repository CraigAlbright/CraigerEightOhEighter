using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CraigerEightOhEighter.Annotations;

namespace CraigerEightOhEighter.Views
{
    /// <summary>
    /// Interaction logic for DrumButton.xaml
    /// </summary>
    public partial class DrumButton : INotifyPropertyChanged, IDrumButton
    {
        public DrumButton()
        {
            InitializeComponent();
        }
        public bool Clicked { get; set; }
        public void DrumButtonClicked(object sender, MouseEventArgs e)
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
