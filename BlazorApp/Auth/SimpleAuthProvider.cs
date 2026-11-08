using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using ApiContracts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorApp.Auth;

public class SimpleAuthProvider(HttpClient httpClient, IJSRuntime jsRuntime) : AuthenticationStateProvider
{
    private const string StorageKey = "currentUser";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task LoginAsync(string userName, string password)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "auth/login",
            new LoginRequest(userName, password));

        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(string.IsNullOrWhiteSpace(content) ? "Login failed." : content);
        }

        UserDto userDto = JsonSerializer.Deserialize<UserDto>(content, JsonOptions)!;

        // Cache in the browser so a page refresh doesn't log the user out.
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey,
            JsonSerializer.Serialize(userDto, JsonOptions));

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(ToPrincipal(userDto))));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? userAsJson;
        try
        {
            userAsJson = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
        }
        catch (Exception)
        {
            // JS interop isn't available yet (e.g. during prerendering) -> treat as logged out.
            return new AuthenticationState(new ClaimsPrincipal());
        }

        if (string.IsNullOrEmpty(userAsJson))
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }

        UserDto? userDto = JsonSerializer.Deserialize<UserDto>(userAsJson, JsonOptions);
        return userDto is null
            ? new AuthenticationState(new ClaimsPrincipal())
            : new AuthenticationState(ToPrincipal(userDto));
    }

    public async Task LogoutAsync()
    {
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
    }

    private static ClaimsPrincipal ToPrincipal(UserDto userDto)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, userDto.UserName),
            new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString())
        ];
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "apiauth"));
    }
}
