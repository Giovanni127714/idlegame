using Idlegame.Helpers;

namespace Idlegame.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string _statusMessage = "Idlegame skelet werkt.";

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }
    }
}
