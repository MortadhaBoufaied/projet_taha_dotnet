using AiClientManager.Web.Auth;
using AiClientManager.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AiClientManager.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;

    public AccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    {
        _signIn = signIn;
        _users = users;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginVm { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _signIn.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction("Dashboard", "Home");
        }

        ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
        return View(vm);
    }

    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new ApplicationUser(vm.Email, vm.Email)
        {
            Email = vm.Email,
            UserName = vm.Email,
            EmailConfirmed = true
        };

        var result = await _users.CreateAsync(user, vm.Password);
        if (result.Succeeded)
        {
            await _users.AddToRoleAsync(user, "User");
            await _signIn.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Dashboard", "Home");
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError(string.Empty, err.Description);

        return View(vm);
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
