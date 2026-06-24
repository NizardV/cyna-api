namespace Domain.Dto.User;

/// <summary>
/// Résultat d'une opération d'authentification.
/// </summary>
public class AuthResultDto
{
    /// <summary>Indique si l'opération a réussi (tokens émis).</summary>
    public bool Success { get; set; }

    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// True uniquement en sortie de <c>/auth/admin/login</c> : l'admin vient
    /// de se connecter pour la 1ère fois et n'a PAS encore activé le 2FA.
    /// Le frontend doit alors le rediriger immédiatement vers la page de
    /// configuration 2FA — il est déjà authentifié (tokens émis) mais son
    /// compte reste vulnérable jusqu'à confirmation.
    /// </summary>
    public bool RequiresTwoFactorSetup { get; set; }

    /// <summary>
    /// True quand les identifiants (email + mot de passe) sont corrects mais
    /// qu'un code TOTP valide est nécessaire pour terminer la connexion.
    /// Permet au frontend de distinguer "mauvais mot de passe" (repasser en
    /// étape 1) de "il manque le code" (passer/rester en étape 2), sans avoir
    /// à parser le message d'erreur.
    /// </summary>
    public bool TotpRequired { get; set; }
}