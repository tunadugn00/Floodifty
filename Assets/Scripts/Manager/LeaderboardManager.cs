using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    const string LEADERBOARD_ID = "endless_highscore";

    public bool IsInitialized { get; private set; } = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        await InitializeAsync();
    }

    async Task InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // Set tên mặc định nếu chưa có
            string savedName = PlayerPrefs.GetString("PlayerDisplayName", "");
            if (string.IsNullOrEmpty(savedName))
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                savedName = "Player_" + playerId.Substring(playerId.Length - 4).ToUpper();
                PlayerPrefs.SetString("PlayerDisplayName", savedName);
                PlayerPrefs.Save();
            }

            await AuthenticationService.Instance.UpdatePlayerNameAsync(savedName);

            IsInitialized = true;
            Debug.Log($"[Leaderboard] Ready. Name: {savedName}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Leaderboard] Init failed: {e.Message}");
        }
    }

    // Gọi khi người chơi thắng Endless
    public async Task SubmitScoreAsync(int score)
    {
        if (!IsInitialized) return;
        try
        {
            var entry = await LeaderboardsService.Instance
                .AddPlayerScoreAsync(LEADERBOARD_ID, score);
            Debug.Log($"[Leaderboard] Score {score} submitted → Rank #{entry.Rank + 1}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Leaderboard] Submit failed: {e.Message}");
        }
    }

    // Lấy top N người chơi
    public async Task<List<LeaderboardEntry>> GetTopScoresAsync(int count = 10)
    {
        if (!IsInitialized) return null;
        try
        {
            var options = new GetScoresOptions { Limit = count };
            var result = await LeaderboardsService.Instance
                .GetScoresAsync(LEADERBOARD_ID, options);
            return result.Results;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Leaderboard] Fetch failed: {e.Message}");
            return null;
        }
    }

    // Lấy rank của người chơi hiện tại
    public async Task<LeaderboardEntry> GetPlayerScoreAsync()
    {
        if (!IsInitialized) return null;
        try
        {
            return await LeaderboardsService.Instance
                .GetPlayerScoreAsync(LEADERBOARD_ID);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Leaderboard] GetPlayerScore failed: {e.Message}");
            return null;
        }
    }
    public async Task UpdatePlayerNameAsync(string newName)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            PlayerPrefs.SetString("PlayerDisplayName", newName);
            PlayerPrefs.Save();
            Debug.Log($"[Leaderboard] Name updated: {newName}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Leaderboard] Update name failed: {e.Message}");
        }
    }
}