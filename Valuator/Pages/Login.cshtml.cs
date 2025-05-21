using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Valuator.Services;
using Valuator.Models;

namespace Valuator.Pages;

public class LoginModel : PageModel
{
    private readonly IUserStorageService _userStorageService;

    public LoginModel(IUserStorageService userStorageService)
    {
        _userStorageService = userStorageService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string username, string password)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Username and password are required.");
            return Page();
        }

        User? user = await _userStorageService.FindByUserNameAsync(username);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var passwordHasher = new PasswordHasher<User>();
        PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid password.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);
        
        return RedirectToPage("/Index");
    }
}