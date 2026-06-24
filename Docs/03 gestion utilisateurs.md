# Gestion des Utilisateurs — Cyna API

## 🎯 Objectif du document

Détailler les routes et la logique métier liées au **profil utilisateur** (`UserController`) et à **l'administration des comptes** (`AdminUserController`), ainsi que les règles de cohérence appliquées (vérification email, désactivation, rôles).

---

## 👤 1. Profil utilisateur (`UserController` — `/user`, `[Authorize]`)

Toutes les routes nécessitent un JWT valide. L'identifiant de l'utilisateur connecté est extrait du token via `ClaimsHelper.GetUserId(User)` (voir `01-Authentification-JWT-2FA.md`).

| Route | Méthode | Description |
|---|---|---|
| `/user/profile` | GET | Retourne `UserProfileDto` (id, email, nom, rôle, `IsEmailVerified`, `IsDisabled`, `TwoFactorEnabled`, date de création). |
| `/user/profile` | PUT | Met à jour prénom/nom/email. Si l'email change → `IsEmailVerified = false` + nouvel OTP envoyé (voir `02-Email-OTP.md`). |
| `/user/password` | PUT | Change le mot de passe (vérifie l'ancien mot de passe). |
| `/user/orders` | GET | Historique des commandes de l'utilisateur (voir `05-Panier-Commandes.md`). |
| `/user/subscriptions` | GET | Abonnements actifs de l'utilisateur (voir `05-Panier-Commandes.md`). |

### `UpdateProfileAsync` — logique de détection du changement d'email

```csharp
var emailChanged = !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase);
user.FirstName = dto.FirstName;
user.LastName  = dto.LastName;
user.Email     = dto.Email;

if (emailChanged)
{
    user.IsEmailVerified = false;
    // ...
}
await _userRepository.UpdateAsync(user);
if (emailChanged)
    await _authService.SendEmailVerificationOtpInternalAsync(user);
```

⚠️ La comparaison est **insensible à la casse** — changer `Jean@Mail.com` en `jean@mail.com` n'est **pas** considéré comme un changement d'email (pas de re-vérification déclenchée), ce qui est cohérent avec le fait que les adresses email ne sont normalement pas sensibles à la casse.

### `UpdatePasswordAsync` — vérification de l'ancien mot de passe

```csharp
if (!dto.CurrentPassword.VerifyHashProvided(user.PasswordHash))
    throw new UnauthorizedAccessException("Le mot de passe actuel est incorrect.");
```

Notez que cette exception est interceptée côté contrôleur et renvoyée en **`400 BadRequest`** (pas `401`) — choix délibéré car l'utilisateur est déjà authentifié, l'erreur porte sur la validation du formulaire, pas sur l'accès à la ressource.

---

## 🛠️ 2. Administration des comptes (`AdminUserController` — `/admin/users`, policy `AdminOnly`)

Toutes les routes sont protégées par la policy `AdminOnly` (rôles `Administrateur` ou `Super Administrateur` — voir `01-Authentification-JWT-2FA.md`).

| Route | Méthode | Description | Codes retour |
|---|---|---|---|
| `/admin/users` | GET | Liste tous les utilisateurs **sauf l'admin connecté** (`GetAllUsersExceptAsync`). | 200, 401, 403 |
| `/admin/users/{id}/disable` | PATCH | Désactive un compte (`IsDisabled = true`). Un compte désactivé ne peut plus se connecter ni rafraîchir son token. | 200, 404 |
| `/admin/users/{id}/enable` | PATCH | Réactive un compte. | 200, 404 |
| `/admin/users/{id}/role` | PATCH | Change le rôle d'un utilisateur (`ChangeRoleDto.Role`, enum `UserRole`). | 200, 400, 404 |

### Pourquoi exclure l'admin connecté de la liste ?

`GetAllUsersExceptAsync(currentAdminId)` évite qu'un admin puisse accidentellement se désactiver ou changer son propre rôle depuis l'interface de gestion des utilisateurs — protection contre l'auto-verrouillage.

### Impact d'une désactivation de compte

La désactivation (`IsDisabled = true`) est vérifiée à **trois endroits distincts** dans `AuthService`, garantissant qu'un compte désactivé est immédiatement bloqué sur tous les flux :

1. `LoginAsync` → refuse la connexion standard.
2. `ResetTokenAsync` (refresh) → refuse même si le refresh token est encore valide.
3. `AdminLoginWithTwoFactorAsync` → refuse la connexion admin avant même de vérifier le rôle/2FA.

> Le compte désactivé garde néanmoins son JWT existant valable jusqu'à expiration naturelle (15 min) si déjà émis — la désactivation empêche le **renouvellement**, pas la révocation immédiate du token en cours. À considérer si une révocation instantanée est requise (ex. blocklist de tokens, durée de vie de l'access token plus courte).

### Changement de rôle (`SetUserRoleAsync`)

```csharp
await _userRepository.SetRoleAsync(targetUserId, role);
```

Effectué via une mise à jour ciblée (`ExecuteUpdateAsync`) côté `UserRepository`, sans recharger l'entité complète — opération efficace côté base. Le message de confirmation utilise `dto.Role.GetEnumDescription()` (ex. *"Rôle de l'utilisateur 12 changé en « Administrateur »."*).

---

## 🧩 3. DTOs de mapping

### `UserProfileDto` (profil propre à l'utilisateur)

Inclut `TwoFactorEnabled` — **reflète l'état confirmé** du 2FA (pas la simple présence d'un secret en attente), voir `01-Authentification-JWT-2FA.md`.

### `AdminUserDto` (vue liste pour les admins)

Inclut `HasTwoFactor` — même règle : reflète `TwoFactorEnabled`, **pas** `TwoFactorSecret != null`. Le commentaire dans `UserService.ToAdminDto` est explicite : *« HasTwoFactor reflects CONFIRMED 2FA only, not a pending/unconfirmed secret »*.

---

## ⚠️ 4. Points d'attention

* Aucune route ne permet à un admin de **réinitialiser le 2FA** d'un autre admin (ex. perte du téléphone authenticator) — actuellement seul l'utilisateur lui-même peut relancer `/auth/2fa/setup` (et seulement s'il n'a pas encore confirmé, ou en repassant par la connexion bootstrap). Pourrait nécessiter une route d'urgence réservée à `Super Administrateur`.
* Le contrôle d'auto-modification n'existe que pour la **liste** (`GetAllUsersExceptAsync`) — rien n'empêche techniquement un admin d'appeler `PATCH /admin/users/{son-propre-id}/disable` ou `/role` directement par ID s'il le connaît. À sécuriser si nécessaire (vérification `id != currentAdminId` côté service).

---

## 🔗 Documents liés

* `01-Authentification-JWT-2FA.md`
* `02-Email-OTP.md`
* `05-Panier-Commandes.md`