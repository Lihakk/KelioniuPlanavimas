using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserAccount>> register(RegisterRequest request)
    {
        if (!checkRegistrationData(request))
        {
            return BadRequest("Registration data is not valid.");
        }

        var exists = await _context.UserAccounts.AnyAsync(item => item.Email == request.Email);
        if (exists)
        {
            return Conflict("Account already exists.");
        }

        var user = createAccount(request);
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(profile), new { id = user.Id }, user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserAccount>> login(LoginRequest request)
    {
        var user = await _context.UserAccounts.FirstOrDefaultAsync(item => item.Email == request.Email);
        if (user == null || !checkLoginData(user, request))
        {
            return Unauthorized();
        }

        return user;
    }

    [HttpPost("logout")]
    public ActionResult<object> logout()
    {
        return new { message = "User logged out" };
    }

    [HttpGet("profile/{id}")]
    public async Task<ActionResult<UserAccount>> profile(int id)
    {
        var user = await _context.UserAccounts.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPut("profile/{id}")]
    public async Task<IActionResult> editProfile(int id, ProfileUpdateRequest request)
    {
        var user = await _context.UserAccounts.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        updateProfileData(user, request);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Profile updated" });
    }

    [HttpGet("administrateAccounts")]
    public async Task<ActionResult<IEnumerable<UserAccount>>> administrateAccounts()
    {
        return await _context.UserAccounts.ToListAsync();
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> blockAccount(int id)
    {
        return await changeAccountStatus(id, blockAccount);
    }

    [HttpPost("{id}/unblock")]
    public async Task<IActionResult> unblockAccount(int id)
    {
        return await changeAccountStatus(id, unblockAccount);
    }

    private bool checkRegistrationData(RegisterRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.FirstName)
            && !string.IsNullOrWhiteSpace(request.LastName)
            && request.Email.Contains('@')
            && request.Password.Length >= 6;
    }

    private UserAccount createAccount(RegisterRequest request)
    {
        return new UserAccount
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = hashPassword(request.Password),
            IsAdmin = request.IsAdmin,
            AccountStatus = AccountStatus.Active
        };
    }

    private bool checkLoginData(UserAccount user, LoginRequest request)
    {
        return user.AccountStatus == AccountStatus.Active
            && user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)
            && user.PasswordHash == hashPassword(request.Password);
    }

    private static void updateProfileData(UserAccount user, ProfileUpdateRequest request)
    {
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email.Trim();
    }

    private static void blockAccount(UserAccount user)
    {
        user.blockAccount();
    }

    private static void unblockAccount(UserAccount user)
    {
        user.unblockAccount();
    }

    private async Task<IActionResult> changeAccountStatus(int id, Action<UserAccount> changeStatus)
    {
        var user = await _context.UserAccounts.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        changeStatus(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Account status changed", user.AccountStatus });
    }

    private static string hashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

public class ProfileUpdateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
