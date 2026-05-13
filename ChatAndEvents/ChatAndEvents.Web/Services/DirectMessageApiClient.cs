using System;
using System.Net.Http.Json;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.services;

namespace ChatAndEvents.Web.Services;

public class DirectMessageApiClient : IDirectMessageService
{
    private readonly HttpClient _httpClient;

    public DirectMessageApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Conversation> GetOrCreateAsync(Guid userId1, Guid userId2)
    {
        var response = await _httpClient.PostAsync(
            $"api/DirectMessage/conversation?userId1={userId1}&userId2={userId2}",
            null);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Conversation>()
            ?? throw new InvalidOperationException("The API did not return a conversation.");
    }

    public async Task<User?> GetOtherUserAsync(Guid conversationId, Guid viewerUserId)
    {
        return await _httpClient.GetFromJsonAsync<User>(
            $"api/DirectMessage/{conversationId}/other-user?viewerUserId={viewerUserId}");
    }

    public async Task<bool> IsBlockedAsync(Guid conversationId, Guid viewerUserId)
    {
        var response = await _httpClient.GetAsync(
            $"api/DirectMessage/{conversationId}/blocked?viewerUserId={viewerUserId}");

        return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
    }

    public Task<Participant?> GetOtherParticipantAsync(Guid conversationId, Guid currentUserId)
    {
        throw new NotSupportedException("The API does not expose this direct-message operation.");
    }

    public Task<(Message Pinned, Message Notice)> PinMessageAsync(
        Guid conversationId,
        Guid requesterId,
        Guid messageId,
        DateTime expiresAt)
    {
        throw new NotSupportedException("The API does not expose this direct-message operation.");
    }

    public Task<Message> UnpinMessageAsync(Guid conversationId, Guid requesterId)
    {
        throw new NotSupportedException("The API does not expose this direct-message operation.");
    }

    public Task ClearExpiredPinAsync(Guid conversationId, Guid pinnedMessageId)
    {
        throw new NotSupportedException("The API does not expose this direct-message operation.");
    }
}
