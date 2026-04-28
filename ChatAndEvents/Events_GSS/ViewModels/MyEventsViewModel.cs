using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Events_GSS.Data.Models;
using Events_GSS.Data.Services.eventServices;
using Events_GSS.Services.Interfaces;

namespace Events_GSS.ViewModels
{
    public class MyEventsViewModel : INotifyPropertyChanged
    {
        private readonly IEventService _eventService;
        private readonly IUserService _userService;

        public event Action<Event>? EventDetailsRequested;

        public ObservableCollection<Event> Events { get; } = new ObservableCollection<Event>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                if (_isEmpty != value)
                {
                    _isEmpty = value;
                    OnPropertyChanged();
                }
            }
        }

        public MyEventsViewModel(IEventService eventService, IUserService userService)
        {
            _eventService = eventService;
            _userService = userService;
        }

        public async Task LoadMyEventsAsync()
        {
            IsLoading = true;
            try
            {
                var currentUser = _userService.GetCurrentUser();
                var events = await _eventService.GetMyEventsAsync(currentUser.UserId);
                Events.Clear();
                foreach (var ev in events)
                {
                    Events.Add(ev);
                }
                IsEmpty = Events.Count == 0;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load my events: {ex}");
                IsEmpty = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void RequestEventDetails(Event selectedEvent) =>
            EventDetailsRequested?.Invoke(selectedEvent);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}