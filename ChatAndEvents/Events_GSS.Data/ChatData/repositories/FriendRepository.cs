using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.interfaces.Repositories;
using Microsoft.Data.SqlClient;

namespace ChatAndEvents.Data.ChatData.repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly DatabaseManager databaseManager;

        public FriendRepository(DatabaseManager databaseManager)
        {
            this.databaseManager = databaseManager ?? throw new ArgumentNullException(nameof(databaseManager));
        }

        public async Task<Friend?> GetFriendshipAsync(Guid firstUserId, Guid secondUserId)
        {
            const string queryString = @"
            SELECT Id, UserId1, UserId2, Status, IsMatch, CreatedAt
            FROM Friends
            WHERE (UserId1 = @FirstUserId AND UserId2 = @SecondUserId)
               OR (UserId1 = @SecondUserId AND UserId2 = @FirstUserId);";

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@FirstUserId", firstUserId);
            sqlCommand.Parameters.AddWithValue("@SecondUserId", secondUserId);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();
            if (!await dataReader.ReadAsync())
            {
                return null;
            }

            return MapFriendFromDatabase(dataReader);
        }

        public async Task<List<Friend>> GetAllFriendshipsForUserAsync(Guid targetUserId)
        {
            const string queryString = @"
            SELECT Id, UserId1, UserId2, Status, IsMatch, CreatedAt
            FROM Friends
            WHERE UserId1 = @TargetUserId OR UserId2 = @TargetUserId;";

            var friendList = new List<Friend>();

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@TargetUserId", targetUserId);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();
            while (await dataReader.ReadAsync())
            {
                friendList.Add(MapFriendFromDatabase(dataReader));
            }

            return friendList;
        }

        public async Task<List<Friend>> GetPendingRequestsForUserAsync(Guid targetUserId)
        {
            const string queryString = @"
            SELECT Id, UserId1, UserId2, Status, IsMatch, CreatedAt
            FROM Friends
            WHERE UserId2 = @TargetUserId AND Status = @PendingStatus;";

            var friendList = new List<Friend>();

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@TargetUserId", targetUserId);
            sqlCommand.Parameters.AddWithValue("@PendingStatus", (byte)FriendStatus.Pending);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();
            while (await dataReader.ReadAsync())
            {
                friendList.Add(MapFriendFromDatabase(dataReader));
            }
            return friendList;
        }

        public async Task<List<Friend>> GetAcceptedFriendsAsync(Guid targetUserId)
        {
            const string queryString = @"
            SELECT Id, UserId1, UserId2, Status, IsMatch, CreatedAt
            FROM Friends
            WHERE (UserId1 = @TargetUserId OR UserId2 = @TargetUserId) AND Status = @AcceptedStatus;";

            var friendList = new List<Friend>();

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@TargetUserId", targetUserId);
            sqlCommand.Parameters.AddWithValue("@AcceptedStatus", (byte)FriendStatus.Accepted);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();
            while (await dataReader.ReadAsync())
            {
                friendList.Add(MapFriendFromDatabase(dataReader));
            }
            return friendList;
        }

        public async Task<List<Guid>> GetMutualFriendIdentifiersAsync(Guid firstUserId, Guid secondUserId)
        {
            const string queryString = @"
            SELECT UserId1, UserId2
            FROM Friends
            WHERE (UserId1 = @FirstUserId AND UserId2 IN (
                    SELECT CASE WHEN UserId1 = @SecondUserId THEN UserId2 ELSE UserId1 END
                    FROM Friends
                    WHERE (UserId1 = @SecondUserId OR UserId2 = @SecondUserId) AND Status = @AcceptedStatus
                ))
               OR (UserId2 = @FirstUserId AND UserId1 IN (
                    SELECT CASE WHEN UserId1 = @SecondUserId THEN UserId2 ELSE UserId1 END
                    FROM Friends
                    WHERE (UserId1 = @SecondUserId OR UserId2 = @SecondUserId) AND Status = @AcceptedStatus
                )) AND Status = @AcceptedStatus;";

            var mutualFriendIds = new List<Guid>();

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@FirstUserId", firstUserId);
            sqlCommand.Parameters.AddWithValue("@SecondUserId", secondUserId);
            sqlCommand.Parameters.AddWithValue("@AcceptedStatus", (byte)FriendStatus.Accepted);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();
            while (await dataReader.ReadAsync())
            {
                var friendUserId1 = (Guid)dataReader["UserId1"];
                var friendUserId2 = (Guid)dataReader["UserId2"];
                var mutualFriendId = friendUserId1 == firstUserId ? friendUserId2 : friendUserId1;
                mutualFriendIds.Add(mutualFriendId);
            }

            return mutualFriendIds;
        }

        public async Task<bool> CheckIfFriendsAsync(Guid firstUserId, Guid secondUserId)
        {
            const string queryString = @"
            SELECT COUNT(*) 
            FROM Friends
            WHERE ((UserId1 = @FirstUserId AND UserId2 = @SecondUserId) OR (UserId1 = @SecondUserId AND UserId2 = @FirstUserId)) 
              AND Status = @AcceptedStatus;";

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@FirstUserId", firstUserId);
            sqlCommand.Parameters.AddWithValue("@SecondUserId", secondUserId);
            sqlCommand.Parameters.AddWithValue("@AcceptedStatus", (byte)FriendStatus.Accepted);
            var friendList = await sqlCommand.ExecuteScalarAsync();
            var count = Convert.ToInt32(friendList);

            return count > 0;
        }

        public async Task CreateFriendshipAsync(Friend newFriendship)
        {
            const string queryString = @"
            INSERT INTO Friends (Id, UserId1, UserId2, Status, IsMatch, CreatedAt)
            VALUES (@TargetUserId, @UserId1, @UserId2, @Status, @IsMatch, @CreatedAt);";

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@TargetUserId", newFriendship.Id);
            sqlCommand.Parameters.AddWithValue("@UserId1", newFriendship.UserId1);
            sqlCommand.Parameters.AddWithValue("@UserId2", newFriendship.UserId2);
            sqlCommand.Parameters.AddWithValue("@Status", (byte)newFriendship.Status);
            sqlCommand.Parameters.AddWithValue("@IsMatch", newFriendship.IsMatch);
            sqlCommand.Parameters.AddWithValue("@CreatedAt", newFriendship.CreatedAt);
            await sqlCommand.ExecuteNonQueryAsync();
        }

        public async Task UpdateFriendshipStatusAsync(Guid firstUserId, Guid secondUserId, FriendStatus newStatus)
        {
            const string queryString = @"
            UPDATE Friends
            SET Status = @Status
            WHERE (UserId1 = @FirstUserId AND UserId2 = @SecondUserId) OR (UserId1 = @SecondUserId AND UserId2 = @FirstUserId);";

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@FirstUserId", firstUserId);
            sqlCommand.Parameters.AddWithValue("@SecondUserId", secondUserId);
            sqlCommand.Parameters.AddWithValue("@Status", (byte)newStatus);
            await sqlCommand.ExecuteNonQueryAsync();
        }

        public async Task SetMatchStatusAsync(Guid firstUserId, Guid secondUserId, bool isMatchStatus)
        {
            const string queryString = @"
            UPDATE Friends
            SET IsMatch = @IsMatch
            WHERE (UserId1 = @FirstUserId AND UserId2 = @SecondUserId) OR (UserId1 = @SecondUserId AND UserId2 = @FirstUserId);";

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@FirstUserId", firstUserId);
            sqlCommand.Parameters.AddWithValue("@SecondUserId", secondUserId);
            sqlCommand.Parameters.AddWithValue("@IsMatch", isMatchStatus);
            await sqlCommand.ExecuteNonQueryAsync();
        }

        public async Task DeleteFriendshipAsync(Guid firstUserId, Guid secondUserId)
        {
            const string queryString = @"
            DELETE FROM Friends
            WHERE (UserId1 = @FirstUserId AND UserId2 = @SecondUserId) OR (UserId1 = @SecondUserId AND UserId2 = @FirstUserId);";

            await using var databaseConnection = new SqlConnection(databaseManager.ConnectionString);
            await databaseConnection.OpenAsync();

            await using var sqlCommand = new SqlCommand(queryString, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@FirstUserId", firstUserId);
            sqlCommand.Parameters.AddWithValue("@SecondUserId", secondUserId);
            await sqlCommand.ExecuteNonQueryAsync();
        }

        private static Friend MapFriendFromDatabase(SqlDataReader dataReader)
        {
            // Using standard ADO.NET indexing.
            // Note that Status is cast to a byte first, because TINYINT = byte
            return new Friend
            {
                Id = (Guid)dataReader["Id"],
                UserId1 = (Guid)dataReader["UserId1"],
                UserId2 = (Guid)dataReader["UserId2"],
                Status = (FriendStatus)(byte)dataReader["Status"],
                IsMatch = (bool)dataReader["IsMatch"],
                CreatedAt = (DateTime)dataReader["CreatedAt"]
            };
        }
    }
}
