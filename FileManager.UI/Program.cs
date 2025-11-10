using FileManager.Domain.Entities;
using FileManager.Domain.Interfaces.Queries;
using FileManager.Domain.Interfaces.Repositories;
using FileManager.Persistence.Extensions;
using FileManager.Services;
using FileManager.Services.Extensions;
using FileManager.Services.Utils;
using FileManager.UI.ViewUI;
using FileManager.UI.ViewUI.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

var services = new ServiceCollection();

var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Configurations\\appsettings.json", optional: false, reloadOnChange: true)
            .Build();

string connectionString = configuration.GetConnectionString("DbConnection")
                                ?? throw new InvalidOperationException("Connection string not found");

var currentPath = Directory.GetCurrentDirectory();
var fullPathUserDirectory = Path.Combine(currentPath, Menu.TitleUserDirectory);

if (!Directory.Exists(fullPathUserDirectory))
{
    Directory.CreateDirectory(fullPathUserDirectory);
}

services.AddScoped<DirectoryPath>();
services.AddScoped<IMenu>(provider => new Menu
{
    Path = fullPathUserDirectory,
    Output = Console.WriteLine,
    OutputHelpPanel = HelpPanel.Output,
    InputText = InputPanel.Input
});

services.AddPersistenceServices(connectionString);
services.AddApplicationServices();

using var serviceProvider = services.BuildServiceProvider();
var menu = serviceProvider.GetRequiredService<IMenu>();

var userQueries = serviceProvider.GetRequiredService<IUserQueries>();
var userRepository = serviceProvider.GetRequiredService<IUserRepository>();

var menuHandler = new MenuHandler(menu, serviceProvider);
int? id = null;

while (true)
{
    Console.WriteLine("Введите 1, если хотите зарегестрировать");
    Console.WriteLine("Введите 2, если хотите аутнетифицироваться");
    Console.Write("Команда: ");

    var choise = Console.ReadLine();

    if (choise == "1")
    {
        Console.Write("Логин: ");
        var login = Console.ReadLine();

        Console.Write("Пароль: ");
        var password = Console.ReadLine();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Некорректный логин или пароль");
            continue;
        }

        if (login.Length <= 5)
        {
            Console.WriteLine("Длина логина должна быть более 5 символов");
            continue;
        }

        if (password.Length <= 5)
        {
            Console.WriteLine("Длина пароля должна быть более 5 символов");
            continue;
        }

        var loginIsExist = await userQueries.IsExistAsync(login);

        if (loginIsExist)
        {
            Console.WriteLine($"Пользователь с логином: {login} уже существует в системе");
            continue;
        }

        var passwordHash = HashPassword(password);

        var user = new User
        {
            Username = login,
            PasswordHash = passwordHash
        };

        id = await userRepository.AddAsync(user);
        break;
    }
    else if (choise == "2")
    {
        Console.Write("Логин: ");
        var login = Console.ReadLine();

        Console.Write("Пароль: ");
        var password = Console.ReadLine();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Некорректный логин или пароль");
            continue;
        }

        id = await userQueries.GetIdByLoginPasswordAsync(login, password, VerifyPassword);

        if (id is null)
        {
            Console.WriteLine("Некорректный логин или пароль");
            continue;
        }
        break;
    }
    else
    {
        Console.WriteLine("Неверная команада");
    }
}

if (id is null)
{
    throw new Exception("Ошибка получения id пользователя");
}

menu.UserId = id.Value;
Console.WriteLine("Вы вошли");

while (true)
{
    await menuHandler.EnterCommand();
}

static string HashPassword(string password)
{
    var salt = GenerateSalt();

    using var pbkdf2 = new Rfc2898DeriveBytes(
        password: password,
        salt: Encoding.UTF8.GetBytes(salt),
        iterations: 100000,
        hashAlgorithm: HashAlgorithmName.SHA256);

    byte[] hash = pbkdf2.GetBytes(32); 
    return $"{salt}:{Convert.ToBase64String(hash)}";
}

static string GenerateSalt(int size = 16)
{
    byte[] salt = new byte[size];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(salt);
    }

    return Convert.ToBase64String(salt);
}

static bool VerifyPassword(string enteredPassword, string storedHash)
{
    try
    {
        var parts = storedHash.Split(':');

        if (parts.Length != 2)
        {
            return false;
        }

        string salt = parts.First();
        string originalHashBase64 = parts.Second();

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: enteredPassword,
            salt: Encoding.UTF8.GetBytes(salt),
            iterations: 100000,
            hashAlgorithm: HashAlgorithmName.SHA256);

        byte[] enteredHashBytes = pbkdf2.GetBytes(32);
        string enteredHashBase64 = Convert.ToBase64String(enteredHashBytes);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(originalHashBase64),
            enteredHashBytes);
    }
    catch
    {
        return false;
    }
}