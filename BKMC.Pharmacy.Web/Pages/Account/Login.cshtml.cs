using BKMC.Pharmacy.Web.Models.Auth;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly AuthApiService _authApiService;

    public LoginModel(AuthApiService authApiService)
    {
        _authApiService = authApiService;
    }

    [BindProperty]
    public LoginRequestDto LoginRequest { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _authApiService.LoginAsync(LoginRequest);

        if (response == null)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        if (!string.Equals(
                response.Role,
                "Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "You are not authorized to access the Admin Portal.";
            return Page();
        }

        HttpContext.Session.SetString(
            "JWToken",
            response.Token);

        HttpContext.Session.SetString(
            "Username",
            response.Username);

        HttpContext.Session.SetString(
            "Role",
            response.Role);

        HttpContext.Session.SetString(
            "TokenExpiresAt",
            response.ExpiresAt.ToString("O"));

        return RedirectToPage("/Index");
    }
}