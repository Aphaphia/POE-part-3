using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<(bool IsSuccess, string? ErrorMessage)> PostAsync<T>(string endpoint, T data);
    Task<(bool IsSuccess, string? ErrorMessage)> PostFileAsync(string endpoint, IFormFile file);
    Task<(bool IsSuccess, AuthResponseDto? Data, string? ErrorMessage)> LoginAsync(LoginDto data);
    Task<(bool IsSuccess, EventDto? Data, string? ErrorMessage)> CreateEventAsync(EventDto data);
    Task<List<CategoryDto>?> GetCategoriesAsync(int eventId);
    Task<List<EnrollmentDto>?> GetMyEnrollmentsAsync();
    Task<List<ResultDto>?> GetMyResultsAsync();
    Task<bool> EnrolAsync(int eventId, int categoryId);
    Task<UserProfileDto?> GetProfileAsync(int userId);
    Task<bool> UpdateProfileAsync(int userId, UserProfileDto profile);
    Task<bool> UploadProfilePictureAsync(IFormFile file);
    Task<List<EnrollmentDto>?> GetEventEnrollmentsAsync(int eventId);
    Task<(bool IsSuccess, string? ErrorMessage)> CaptureResultAsync(int enrollmentId, TimeSpan finishTime, int position);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("RaceDayApi");
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddJwtToken()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AddJwtToken(); var response = await _httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode) return default;
        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> PostAsync<T>(string endpoint, T data)
    {
        AddJwtToken();
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        return response.IsSuccessStatusCode ? (true, null) : (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> PostFileAsync(string endpoint, IFormFile file)
    {
        AddJwtToken(); using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(); var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);
        var response = await _httpClient.PostAsync(endpoint, content);
        return response.IsSuccessStatusCode ? (true, null) : (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool IsSuccess, AuthResponseDto? Data, string? ErrorMessage)> LoginAsync(LoginDto data)
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/auth/login", content);
        if (response.IsSuccessStatusCode) return (true, JsonConvert.DeserializeObject<AuthResponseDto>(await response.Content.ReadAsStringAsync()), null);
        return (false, null, "Login failed");
    }

    public async Task<(bool IsSuccess, EventDto? Data, string? ErrorMessage)> CreateEventAsync(EventDto data)
    {
        AddJwtToken(); var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/events", content);
        if (response.IsSuccessStatusCode) return (true, JsonConvert.DeserializeObject<EventDto>(await response.Content.ReadAsStringAsync()), null);
        return (false, null, "Failed");
    }

    public async Task<List<CategoryDto>?> GetCategoriesAsync(int eventId)
    {
        AddJwtToken(); var response = await _httpClient.GetAsync($"/api/events/{eventId}/categories");
        return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<List<CategoryDto>>(await response.Content.ReadAsStringAsync()) : null;
    }

    public async Task<List<EnrollmentDto>?> GetMyEnrollmentsAsync()
    {
        AddJwtToken(); var response = await _httpClient.GetAsync("/api/enrollments/my");
        return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<List<EnrollmentDto>>(await response.Content.ReadAsStringAsync()) : null;
    }

    public async Task<List<ResultDto>?> GetMyResultsAsync()
    {
        AddJwtToken(); var response = await _httpClient.GetAsync("/api/results/my");
        return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<List<ResultDto>>(await response.Content.ReadAsStringAsync()) : null;
    }

    public async Task<bool> EnrolAsync(int eventId, int categoryId)
    {
        AddJwtToken(); var content = new StringContent(JsonConvert.SerializeObject(new { EventID = eventId, CategoryID = categoryId }), Encoding.UTF8, "application/json");
        return (await _httpClient.PostAsync("/api/enrollments", content)).IsSuccessStatusCode;
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        AddJwtToken(); var response = await _httpClient.GetAsync($"/api/users/{userId}/profile");
        return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<UserProfileDto>(await response.Content.ReadAsStringAsync()) : null;
    }

    public async Task<bool> UpdateProfileAsync(int userId, UserProfileDto profile)
    {
        AddJwtToken(); var content = new StringContent(JsonConvert.SerializeObject(profile), Encoding.UTF8, "application/json");
        return (await _httpClient.PutAsync($"/api/users/{userId}/profile", content)).IsSuccessStatusCode;
    }

    public async Task<bool> UploadProfilePictureAsync(IFormFile file)
    {
        AddJwtToken(); using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(); var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);
        return (await _httpClient.PostAsync("/api/users/profile/picture", content)).IsSuccessStatusCode;
    }

    public async Task<List<EnrollmentDto>?> GetEventEnrollmentsAsync(int eventId)
    {
        AddJwtToken(); var response = await _httpClient.GetAsync($"/api/events/{eventId}/enrollments");
        return response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<List<EnrollmentDto>>(await response.Content.ReadAsStringAsync()) : null;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CaptureResultAsync(int enrollmentId, TimeSpan finishTime, int position)
    {
        AddJwtToken(); var content = new StringContent(JsonConvert.SerializeObject(new { EnrollmentID = enrollmentId, FinishTime = finishTime, Position = position }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/results", content);
        return response.IsSuccessStatusCode ? (true, null) : (false, "Failed");
    }
}