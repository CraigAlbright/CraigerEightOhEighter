namespace CraigerEightOhEighter.Views
{
    public interface IDrumButton
    {
        string ButtonName { get; set; }
        int SequenceNumber { get; set; }
        int Track { get; set; }
    }
}
