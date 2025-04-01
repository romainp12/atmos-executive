using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

using Org.BouncyCastle.Crypto.Generators;

namespace GestionLocationAvions.Database
{
    public class ConnectionDB
    {
        // Chaîne de connexion à la base de données MySQL
        private static string connectionString =
            "Server=localhost;Database=gestionlocationavions;Uid=your_username;Pwd=your_password;";

        // Méthode pour obtenir une connexion à la base de données
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // Méthode pour hacher un mot de passe avec bcrypt
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        // Méthode pour vérifier un mot de passe avec bcrypt
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
                // En cas d'erreur dans le format de hachage, retourne faux
                return false;
            }
        }
    }
}
