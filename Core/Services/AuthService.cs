using SolarWise.Core.Models;

namespace SolarWise.Core.Services;

public class AuthService
{
    public async Task<bool> RegisterAsync(User user)
    {
        // Logic registrasi user ke database
        await Task.CompletedTask;
        return true;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        // Logic autentikasi user
        await Task.CompletedTask;
        return null;
    }

    public List<Recommendation> ViewRecommendations(int userId)
    {
        // Fetch riwayat rekomendasi milik user
        return new List<Recommendation>();
    }

    // Aksi Khusus Admin
    public async Task<bool> AddSolarPanelAsync(SolarPanel panel)
    {
        await Task.CompletedTask;
        return true;
    }

    public List<User> ViewUsers()
    {
        return new List<User>();
    }

    public bool EditUser(User user)
    {
        return true;
    }

    public bool EditCalculations()
    {
        return true;
    }
}