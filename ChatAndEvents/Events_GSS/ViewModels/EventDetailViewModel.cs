using Events_GSS.Data.Models;
using Events_GSS.Services.Interfaces;
using System.ComponentModel;

public class EventDetailViewModel
{
    private readonly IAttendedEventService _attendedService;
    public Event SelectedEvent { get; }
    
    public event Action? BackRequested;

    // Now it takes the event AND the service
    public EventDetailViewModel(Event selectedEvent, IAttendedEventService attendedService)
    {
        SelectedEvent = selectedEvent;
        _attendedService = attendedService;
    }

    public void RequestBack() => BackRequested?.Invoke();
    
    // You could move the "Join/Leave" logic here later!
}