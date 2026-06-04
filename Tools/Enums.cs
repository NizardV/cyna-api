using System.ComponentModel;

namespace Infrastructure.Entities;

public enum UserRole
{
    [Description("Utilisateur")] User = 0,
    [Description("Administrateur")] Admin = 1,
    [Description("Super Administrateur")] SuperAdmin = 2
}

public enum BillingPeriod
{
    [Description("Mensuel")] Monthly = 0,
    [Description("Annuel")] Yearly = 1,
    [Description("À vie")] Lifetime = 2
}

public enum SubscriptionStatus
{
    [Description("Actif")] Active = 0,
    [Description("Annulé")] Cancelled = 1,
    [Description("Expiré")] Expired = 2,
    [Description("Suspendu")] Suspended = 3,
    [Description("En attente")] Pending = 4
}

public enum OrderStatus
{
    [Description("En attente")] Pending = 0,
    [Description("Payé")] Paid = 1,
    [Description("Échoué")] Failed = 2,
    [Description("Remboursé")] Refunded = 3,
    [Description("Annulé")] Cancelled = 4
}

public enum ContactStatus
{
    [Description("Nouveau")] New = 0,
    [Description("En cours")] InProgress = 1,
    [Description("Résolu")] Resolved = 2,
    [Description("Fermé")] Closed = 3
}

public enum ChatbotSender
{
    [Description("Utilisateur")] User = 0,
    [Description("Bot")] Bot = 1,
    [Description("Agent")] Agent = 2
}

public enum LocaleLang
{
    [Description("Français")] Fr = 0,
    [Description("Anglais")] En = 1
}

public enum BillingUnit
{
    [Description("Par utilisateur")] User = 0,
    [Description("Par appareil")] Device = 1
}

public enum ProductStatus
{
    [Description("Disponible")] Available = 0,
    [Description("Indisponible")] Unavailable = 1,
    [Description("Rupture de stock")] OutOfStock = 2,
    [Description("Aperçu")] Preview = 3
}

public enum CardBrand
{
    [Description("Visa")] Visa = 0,
    [Description("Mastercard")] Mastercard = 1
}