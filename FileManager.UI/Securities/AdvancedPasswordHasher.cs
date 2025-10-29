using System.Security.Cryptography;

namespace FileManager.UI.Securities;

public class AdvancedPasswordHasher(
    int saltSize = 16,
    int hashSize = 32,
    int iterations = 100000,
    HashAlgorithmName? algorithm = null)
{
    private readonly int _saltSize = saltSize;
    private readonly int _hashSize = hashSize;
    private readonly int _iterations = iterations;
    private readonly HashAlgorithmName _algorithm = algorithm ?? HashAlgorithmName.SHA256;

    public PasswordHashResult HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Пароль не может быть пустым");

        // Генерируем соль
        byte[] salt = GenerateSalt();

        // Хэшируем пароль
        byte[] hash = ComputeHash(password, salt);

        // Создаем результат
        return new PasswordHashResult
        {
            Hash = Convert.ToBase64String(hash),
            Salt = Convert.ToBase64String(salt),
            Iterations = _iterations,
            Algorithm = _algorithm.Name
        };
    }

    public bool VerifyPassword(string password, PasswordHashResult storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || storedHash == null)
            return false;

        try
        {
            byte[] salt = Convert.FromBase64String(storedHash.Salt);
            byte[] originalHash = Convert.FromBase64String(storedHash.Hash);

            // Вычисляем хэш для проверяемого пароля
            byte[] testHash = ComputeHash(password, salt, storedHash.Iterations);

            // Безопасное сравнение
            return CryptographicOperations.FixedTimeEquals(originalHash, testHash);
        }
        catch
        {
            return false;
        }
    }

    private byte[] GenerateSalt()
    {
        byte[] salt = new byte[_saltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private byte[] ComputeHash(string password, byte[] salt, int? customIterations = null)
    {
        int iterations = customIterations ?? _iterations;

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations,
            _algorithm
        );
        return pbkdf2.GetBytes(_hashSize);
    }
}

public class PasswordHashResult
{
    public string Hash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public int Iterations { get; set; }
    public string Algorithm { get; set; } = string.Empty;

    // Для хранения в базе данных
    public string ToDatabaseString()
    {
        return $"{Iterations}.{Algorithm}.{Salt}.{Hash}";
    }

    public static PasswordHashResult FromDatabaseString(string data)
    {
        var parts = data.Split('.');
        if (parts.Length != 4)
            throw new FormatException("Неверный формат хэша");

        return new PasswordHashResult
        {
            Iterations = int.Parse(parts[0]),
            Algorithm = parts[1],
            Salt = parts[2],
            Hash = parts[3]
        };
    }
}