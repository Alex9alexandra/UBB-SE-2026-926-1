using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories.eventRepository;

namespace Events_GSS.ViewModels
{
    // 1. Inherit from INotifyPropertyChanged instead of BaseViewModel
    public class EventListingViewModel : INotifyPropertyChanged
    {
        private readonly IEventRepository _eventRepository;
        public event Action? CreateEventRequested;
        public event Action<Event>? EventDetailsRequested;

        // The UI binds to this list
        public ObservableCollection<Event> Events { get; } = new ObservableCollection<Event>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                // 2. We handle the notification manually here instead of using BaseViewModel's "Set()"
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public EventListingViewModel(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task LoadEventsAsync()
        {
            IsLoading = true;
            try
            {
                var events = await _eventRepository.GetAllPublicActiveAsync();
                Events.Clear();
                foreach (var ev in events)
                {
                    Events.Add(ev);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load events: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // 3. This is the background magic that BaseViewModel used to do for you!
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void RequestCreateEvent() => CreateEventRequested?.Invoke();
        public void RequestEventDetails(Event selectedEvent) => EventDetailsRequested?.Invoke(selectedEvent);
    }
}