namespace Lab01.App.ViewModels;

public sealed class NetworkTaskRowViewModel : ViewModelBase
{
    private int _id;
    private string _predecessorsText = "-";
    private int _duration;
    private int _people;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    public string PredecessorsText
    {
        get => _predecessorsText;
        set { _predecessorsText = value; OnPropertyChanged(); }
    }

    public int Duration
    {
        get => _duration;
        set { _duration = value; OnPropertyChanged(); }
    }

    public int People
    {
        get => _people;
        set { _people = value; OnPropertyChanged(); }
    }
}
