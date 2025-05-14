using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Valuator.Models;
using Valuator.Services;

namespace Valuator.Pages;

public class RegisterModel : PageModel
{
    private readonly IUserStorageService _userStorageService;

    public RegisterModel(IUserStorageService userStorageService)
    {
        _userStorageService = userStorageService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string username, string password)
    {
        Console.WriteLine($"username: {username}, Password: {password}");
        
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Username and password are required.");
            return Page();
        }

        var existingUser = await _userStorageService.FindByUserNameAsync(username);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "User already exists.");
            return Page();
        }

        User newUser = await _userStorageService.CreateAsync(username, password);

        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, newUser.Id.ToString()),
            new (ClaimTypes.Name, newUser.UserName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToPage("/Index");
    }
}