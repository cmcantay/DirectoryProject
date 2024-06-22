using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class UserService
{
    private readonly string _connectionString;
    private readonly ILogger<UserService> _logger;

    public UserService(string connectionString, ILogger<UserService> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<int?> ValidateUserAsync(string username, string password)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT UserId FROM tbl_Users WHERE Username = @Username AND Password = @Password", connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    return (int?)result;
                }
                else
                {
                    _logger.LogWarning("Sorgu null veya DBNull döndürdü.");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı doğrulanırken bir hata oluştu.");
            throw;
        }
    }

    public async Task<List<DirectoryEntry>> GetDirectoryEntriesAsync(int userId)
    {
        var entries = new List<DirectoryEntry>();

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT DirectoryId, UserId, FirstName, LastName, PhoneNumber FROM tbl_directory WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(new DirectoryEntry
                        {
                            DirectoryId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            FirstName = reader.GetString(2),
                            LastName = reader.GetString(3),
                            PhoneNumber = reader.GetString(4)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rehber kayıtları alınırken bir hata oluştu.");
            throw;
        }

        return entries;
    }

    public async Task AddDirectoryEntryAsync(DirectoryEntry entry)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("INSERT INTO tbl_directory (UserId, FirstName, LastName, PhoneNumber) VALUES (@UserId, @FirstName, @LastName, @PhoneNumber)", connection);
                command.Parameters.AddWithValue("@UserId", entry.UserId);
                command.Parameters.AddWithValue("@FirstName", entry.FirstName);
                command.Parameters.AddWithValue("@LastName", entry.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", entry.PhoneNumber);

                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yeni rehber kaydı eklenirken bir hata oluştu.");
            throw;
        }
    }

    public async Task UpdateDirectoryEntryAsync(DirectoryEntry entry)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("UPDATE tbl_directory SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber WHERE DirectoryId = @DirectoryId", connection);
                command.Parameters.AddWithValue("@DirectoryId", entry.DirectoryId);
                command.Parameters.AddWithValue("@FirstName", entry.FirstName);
                command.Parameters.AddWithValue("@LastName", entry.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", entry.PhoneNumber);

                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rehber kaydı güncellenirken bir hata oluştu.");
            throw;
        }
    }

    public async Task DeleteDirectoryEntryAsync(int directoryId)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("DELETE FROM tbl_directory WHERE DirectoryId = @DirectoryId", connection);
                command.Parameters.AddWithValue("@DirectoryId", directoryId);

                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rehber kaydı silinirken bir hata oluştu.");
            throw;
        }
    }

    public async Task<bool> RegisterUserAsync(string username, string password)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("INSERT INTO tbl_Users (Username, Password) VALUES (@Username, @Password)", connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                await command.ExecuteNonQueryAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yeni kullanıcı kaydı oluşturulurken bir hata oluştu.");
            return false;
        }
    }
}

public class DirectoryEntry
{
    public int DirectoryId { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
}
