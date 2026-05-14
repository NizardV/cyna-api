using Domain.Entities;

namespace Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                Password = "AQAAAAIAAYagAAAAECcuSZMjFdCazAPjxBVlAmCQZWyRqg6fggGoLMXikuXaZ2Fz6SD+kDP1aSDeaGUbvg==",
                Role = "Admin",
                Email = "admin@admin.fr"
            },
            new User
            {
                Username = "user",
                Password = "AQAAAAIAAYagAAAAEPfiLei9vzYZAbxxbd9VpIOZ27gwCrc/MZLfXVNXPtj8DEOWkL9QO4foKnXkuoiP1A==",
                Role = "User",
                Email = "user@user.fr"
            },
            new User
            {
                Username = "plop",
                Password = "AQAAAAIAAYagAAAAELwP0fHSzY9JOkS1jKuYKGsTt69+2wtm4EKtes/W173q6HyVmZOvdN2p2k4s3YMKTQ==",
                Role = "User",
                Email = "plop@plop.fr"
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        if (context.Games.Any())
        {
            return;
        }

        var games = new List<Game>
        {
            new Game
            {
                Title = "The Legend of Zelda: Breath of the Wild",
                Platform = "Nintendo Switch",
                Genre = "Action-Adventure",
                ReleaseDate = new DateTime(2017, 3, 3)
            },
            new Game
            {
                Title = "Elden Ring",
                Platform = "PC",
                Genre = "Action-RPG",
                ReleaseDate = new DateTime(2022, 2, 25)
            }
        };

        context.Games.AddRange(games);
        await context.SaveChangesAsync();
    }
}
