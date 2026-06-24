namespace Application.Services;

using System.Security.Cryptography;

using Application.Interfaces;

using Domain.Dto.User;
using Domain.Entities;
using Domain.Entities.AuthCodes;

using Infrastructure.Interfaces;

using NLog;

using Tools;

/// <summary>
/// Implémentation du service d'authentification.
/// </summary>
public class AuthService : IAuthService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _jwtTokenGenerator;
    private readonly EmailHelper     _emailHelper;

    public AuthService(
        IUserRepository userRepository,
        ITokenGenerator jwtTokenGenerator,
        EmailHelper     emailHelper)
    {
        _userRepository    = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailHelper       = emailHelper;
    }

    // ── Login (standard) ──────────────────────────────────────────────────────

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || !request.Password.VerifyHashProvided(user.PasswordHash))
            return Fail("Identifiants invalides.");

        if (user.IsDisabled)
            return Fail("Ce compte a été désactivé. Contactez l'administrateur.");

        // Only block once 2FA is CONFIRMED active — never just because a secret
        // was generated. Otherwise an admin who started (but never finished)
        // 2FA setup would be locked out of both login routes simultaneously.
        if (user.Role >= UserRole.Admin && user.TwoFactorEnabled)
            return Fail("Les comptes administrateur avec 2FA activé doivent utiliser la connexion administrateur.");

        return await IssueTokensAsync(user);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.GetByEmailAsync(request.Email) is not null)
            return Fail("Email déjà utilisé.");

        var newUser = new User
        {
            Email           = request.Email,
            PasswordHash    = request.Password.GetHash(),
            FirstName       = request.FirstName,
            LastName        = request.LastName,
            Role            = UserRole.User,
            IsEmailVerified = false,
            CreatedAt       = DateTime.UtcNow
        };

        await _userRepository.AddAsync(newUser);
        await SendEmailVerificationOtpInternalAsync(newUser);

        return new AuthResultDto { Success = true };
    }

    // ── Refresh token ─────────────────────────────────────────────────────────

    public async Task<AuthResultDto?> ResetTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (user is null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return null;

        if (user.IsDisabled)
            return null;

        return await IssueTokensAsync(user);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
        if (user is null) return false;

        user.RefreshToken           = null;
        user.RefreshTokenExpiryTime = null;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    // ── Password reset (OTP) ──────────────────────────────────────────────────

    public async Task SendPasswordResetOtpAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null) return; // anti-enumeration

        var code = OtpGenerator.Generate(6);

        await _userRepository.AddPasswordResetCodeAsync(new PasswordResetCode
        {
            UserId    = user.Id,
            Code      = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed    = false
        });

        await _emailHelper.SendAsync(
            to:       user.Email,
            subject:  "Réinitialisation de votre mot de passe",
            htmlBody: BuildOtpEmailHtml(
                user.FirstName, code, "Réinitialisation de mot de passe",
                "Utilisez le code ci-dessous pour réinitialiser votre mot de passe. Il expire dans 15 minutes."),
            attachments: null);

        _logger.Info("OTP de réinitialisation envoyé à {Email}", user.Email);
    }

    public async Task<bool> ResetPasswordWithOtpAsync(ResetPasswordWithOtpDto dto)
    {
        var entry = await _userRepository.GetValidPasswordResetCodeAsync(dto.Email, dto.Code);
        if (entry is null) return false;

        await _userRepository.MarkPasswordResetCodeUsedAsync(entry.Id);
        await _userRepository.UpdatePasswordAsync(entry.UserId, dto.NewPassword.GetHash());

        _logger.Info("Mot de passe réinitialisé via OTP pour {Email}", dto.Email);
        return true;
    }

    // ── Email confirmation (OTP) ──────────────────────────────────────────────

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user is null) return false;

        var entry = await _userRepository.GetValidEmailVerificationCodeAsync(user.Id, dto.Code);
        if (entry is null) return false;

        await _userRepository.MarkEmailVerificationCodeUsedAsync(entry.Id);

        user.IsEmailVerified = true;
        await _userRepository.UpdateAsync(user);

        _logger.Info("Email vérifié pour l'utilisateur ID {UserId}", user.Id);
        return true;
    }

    // ── Admin 2FA setup ───────────────────────────────────────────────────────

    public async Task<TwoFactorSetupDto> SetupTwoFactorAsync(int adminUserId)
    {
        var user = await _userRepository.GetByIdAsync(adminUserId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {adminUserId}).");

        if (user.Role < UserRole.Admin)
            throw new InvalidOperationException("Le 2FA n'est disponible que pour les comptes administrateur.");

        var secret = TotpHelper.GenerateSecret();
        var otpUrl = TotpHelper.BuildOtpAuthUrl(user.Email, secret);

        // Store the secret as PENDING only — TwoFactorEnabled stays false until
        // ConfirmTwoFactorAsync succeeds. This is what makes it safe to call
        // /auth/2fa/setup again (e.g. the admin re-scans because they fumbled
        // the QR code) without ever risking a lockout: standard login only
        // checks TwoFactorEnabled, never the mere presence of a secret.
        user.TwoFactorSecret = secret;
        await _userRepository.UpdateAsync(user);

        return new TwoFactorSetupDto { Secret = secret, OtpAuthUrl = otpUrl };
    }

    public async Task<bool> ConfirmTwoFactorAsync(int adminUserId, string totpCode)
    {
        var user = await _userRepository.GetByIdAsync(adminUserId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {adminUserId}).");

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new InvalidOperationException("Le 2FA n'a pas encore été configuré pour cet utilisateur.");

        var valid = TotpHelper.Verify(user.TwoFactorSecret, totpCode);
        if (!valid)
        {
            _logger.Warn("Confirmation 2FA refusée (code invalide) pour l'utilisateur ID {UserId}", adminUserId);
            return false;
        }

        // This is the ONLY place TwoFactorEnabled flips to true.
        user.TwoFactorEnabled = true;
        await _userRepository.UpdateAsync(user);

        _logger.Info("2FA activé pour l'utilisateur ID {UserId}", adminUserId);
        return true;
    }

    // ── Admin login (bootstrap-aware) ─────────────────────────────────────────

    public async Task<AuthResultDto> AdminLoginWithTwoFactorAsync(AdminTwoFactorLoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user is null || !dto.Password.VerifyHashProvided(user.PasswordHash))
            return Fail("Identifiants invalides.");

        if (user.IsDisabled)
            return Fail("Ce compte a été désactivé.");

        if (user.Role < UserRole.Admin)
            return Fail("Accès réservé aux administrateurs.");

        // ── Bootstrap path: 2FA not yet confirmed for this account ───────────
        // Let the admin in on credentials alone so the frontend can immediately
        // route them to /account/security/2fa. They're never stuck.
        if (!user.TwoFactorEnabled)
        {
            _logger.Info("Connexion admin bootstrap (2FA non configuré) pour {Email}", dto.Email);
            var bootstrapResult = await IssueTokensAsync(user);
            bootstrapResult.RequiresTwoFactorSetup = true;
            return bootstrapResult;
        }

        // ── Normal path: 2FA is active, a valid TOTP code is mandatory ───────
        if (string.IsNullOrWhiteSpace(dto.TotpCode))
        {
            return new AuthResultDto
            {
                Success      = false,
                TotpRequired = true,
                ErrorMessage = "Veuillez saisir votre code TOTP."
            };
        }

        if (!TotpHelper.Verify(user.TwoFactorSecret!, dto.TotpCode))
        {
            return new AuthResultDto
            {
                Success      = false,
                TotpRequired = true,
                ErrorMessage = "Code TOTP invalide ou expiré."
            };
        }

        _logger.Info("Connexion admin 2FA réussie pour {Email}", dto.Email);
        return await IssueTokensAsync(user);
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    public async Task SendEmailVerificationOtpInternalAsync(User user)
    {
        var code = OtpGenerator.Generate(6);

        await _userRepository.AddEmailVerificationCodeAsync(new EmailVerificationCode
        {
            UserId    = user.Id,
            Code      = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            IsUsed    = false
        });

        await _emailHelper.SendAsync(
            to:       user.Email,
            subject:  "Vérification de votre adresse email",
            htmlBody: BuildOtpEmailHtml(
                user.FirstName, code, "Vérification d'email",
                "Utilisez le code ci-dessous pour confirmer votre adresse email. Il expire dans 30 minutes."),
            attachments: null);

        _logger.Info("OTP de vérification email envoyé à {Email}", user.Email);
    }

    private async Task<AuthResultDto> IssueTokensAsync(User user)
    {
        var token        = _jwtTokenGenerator.GenerateToken(user);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        user.RefreshToken           = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        await _userRepository.UpdateAsync(user);

        return new AuthResultDto { Success = true, Token = token, RefreshToken = refreshToken };
    }

    private static AuthResultDto Fail(string message) =>
        new() { Success = false, ErrorMessage = message };

    private static string BuildOtpEmailHtml(string firstName, string code, string title, string body) => $"""
        <!DOCTYPE html>
        <html lang="fr">
        <body style="font-family:sans-serif;max-width:480px;margin:auto;padding:32px">
          <h2>{title}</h2>
          <p>Bonjour {firstName},</p>
          <p>{body}</p>
          <div style="font-size:36px;font-weight:bold;letter-spacing:8px;text-align:center;
                      padding:24px 0;background:#f4f4f4;border-radius:8px;margin:24px 0">
            {code}
          </div>
          <p style="color:#888;font-size:12px">
            Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.
          </p>
        </body>
        </html>
        """;
}