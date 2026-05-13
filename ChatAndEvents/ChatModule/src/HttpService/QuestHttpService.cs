using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Events_GSS.Data.Models;
using Events_GSS.Data.Services.Interfaces;

namespace Events_GSS.Data.Services;

public class QuestHttpService : IQuestService
{
    private readonly HttpClient _httpClient;

    public QuestHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> AddQuestAsync(Event toEvent, Quest quest)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/Quests/{toEvent.EventId}",
            quest);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task<List<Quest>> GetQuestsAsync(Event fromEvent)
    {
        var quests = await _httpClient.GetFromJsonAsync<List<Quest>>(
            $"api/Quests/{fromEvent.EventId}");

        return quests ?? new List<Quest>();
    }

    public async Task DeleteQuestAsync(Quest quest)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "api/Quests")
        {
            Content = JsonContent.Create(quest)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Quest>> GetPresetQuestsAsync()
    {
        var quests = await _httpClient.GetFromJsonAsync<List<Quest>>("api/Quests/preset");
        return quests ?? new List<Quest>();
    }
}
