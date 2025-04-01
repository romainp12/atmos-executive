using System;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using GestionLocationAvions.Database;
using GestionLocationAvions.Utils;

namespace GestionLocationAvions
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
            txtNom.Focus();
        }

        // Événement de clic sur le bouton d'inscription
        private void btnInscription_Click(object sender, RoutedEventArgs e)
        {
            // Récupération des valeurs
            string nom = txtNom.Text.Trim();
            string prenom = txtPrenom.Text.Trim();
            string email = txtEmail.Text.Trim();
            string telephone = txtTelephone.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;
            bool estAdmin = chkEstAdmin.IsChecked ?? false;

            // Validation des champs
            if (string.IsNullOrEmpty(nom) || string.IsNullOrEmpty(prenom) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                txtErreur.Text = "Veuillez remplir tous les champs obligatoires.";
                return;
            }

            // Validation de l'email
            if (!IsValidEmail(email))
            {
                txtErreur.Text = "Veuillez entrer une adresse email valide.";
                return;
            }

            // Validation du mot de passe
            if (password.Length < 6)
            {
                txtErreur.Text = "Le mot de passe doit contenir au moins 6 caractères.";
                return;
            }

            // Vérification de la concordance des mots de passe
            if (password != confirmPassword)
            {
                txtErreur.Text = "Les mots de passe ne correspondent pas.";
                return;
            }

            try
            {
                // Vérification si l'email existe déjà
                if (EmailExists(email))
                {
                    txtErreur.Text = "Cette adresse email est déjà utilisée.";
                    return;
                }

                // Hashage du mot de passe
                string hashedPassword = ConnectionDB.HashPassword(password);

                // Insertion de l'utilisateur
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();
                    string query = "INSERT INTO Utilisateurs (nom, prenom, email, mot_de_passe, telephone, est_admin) " +
                                   "VALUES (@Nom, @Prenom, @Email, @MotDePasse, @Telephone, @EstAdmin)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nom", nom);
                        command.Parameters.AddWithValue("@Prenom", prenom);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@MotDePasse", hashedPassword);
                        command.Parameters.AddWithValue("@Telephone", telephone);
                        command.Parameters.AddWithValue("@EstAdmin", estAdmin ? 1 : 0);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBoxService.ShowInfo("Inscription réussie ! Vous pouvez maintenant vous connecter.", "Inscription terminée");
                            // Redirection vers la page de connexion
                            Login loginWindow = new Login();
                            loginWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            txtErreur.Text = "Erreur lors de l'inscription. Veuillez réessayer.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Une erreur est survenue : " + ex.Message, "Erreur");
            }
        }

        // Vérifier si l'email existe déjà
        private bool EmailExists(string email)
        {
            using (MySqlConnection connection = ConnectionDB.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(id_utilisateur) FROM Utilisateurs WHERE email = @Email";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Validation du format de l'email
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        // Événement de clic sur le bouton de retour à la connexion
        private void btnRetourConnexion_Click(object sender, RoutedEventArgs e)
        {
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }
    }
}
