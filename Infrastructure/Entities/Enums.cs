namespace Infrastructure.Entities;

public enum UserRole
{
    User,
    Admin,
    SuperAdmin
}

public enum BillingPeriod
{
    Monthly,
    Yearly,
    Lifetime
}

public enum SubscriptionStatus
{
    Active,
    Cancelled,
    Expired,
    Suspended,
    Pending
}

public enum OrderStatus
{
    Pending,
    Paid,
    Failed,
    Refunded,
    Cancelled
}

public enum ContactStatus
{
    New,
    InProgress,
    Resolved,
    Closed
}

public enum ChatbotSender
{
    User,
    Bot,
    Agent
}

public enum LocaleLang
{
    Fr,
    En
}

public enum BillingUnit
{
    User,
    Device
}

public enum ProductStatus
{
    Available,
    Unavailable,
    Preview
}