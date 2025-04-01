using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using GestionLocationAvions.Database;
using Microsoft.Win32;
using GestionLocationAvions.Utils;

namespace GestionLocationAvions
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            txtEmail.Focus();
        }

        // Événement de clic sur le bouton de connexion
        private void btnConnexion_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            // Validation des champs
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                txtErreur.Text = "Veuillez remplir tous les champs.";
                return;
            }

            try
            {
                // Vérification des identifiants
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT id_utilisateur, prenom, nom, est_admin, mot_de_passe FROM Utilisateurs WHERE email = @Email";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hashedPassword = reader["mot_de_passe"].ToString();

                                // Vérification du mot de passe
                                if (ConnectionDB.VerifyPassword(password, hashedPassword))
                                {
                                    // Stockage des informations de l'utilisateur
                                    App.CurrentUserId = Convert.ToInt32(reader["id_utilisateur"]);
                                    App.CurrentUserName = reader["prenom"].ToString();
                                    App.Initials = $"{reader["prenom"].ToString()[0]}{reader["nom"].ToString()[0]}".ToUpper();
                                    App.IsAdmin = Convert.ToBoolean(reader["est_admin"]);

                                    // Redirection vers le tableau de bord approprié
                                    if (App.IsAdmin)
                                    {
                                        AdminDashboard adminDashboard = new AdminDashboard();
                                        adminDashboard.Show();
                                    }
                                    else
                                    {
                                        ClientDashboard clientDashboard = new ClientDashboard();
                                        clientDashboard.Show();
                                    }

                                    this.Close();
                                }
                                else
                                {
                                    MessageBoxService.ShowError("Email ou mot de passe incorrect.", "Erreur de connexion");
                                    txtErreur.Text = "Email ou mot de passe incorrect.";
                                }
                            }
                            else
                            {
                                MessageBoxService.ShowError("Email ou mot de passe incorrect.", "Erreur de connexion");
                                txtErreur.Text = "Email ou mot de passe incorrect.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Une erreur est survenue : " + ex.Message, "Erreur");
            }
        }

        // Événement de clic sur le bouton d'inscription
        private void btnInscription_Click(object sender, RoutedEventArgs e)
        {
            Register registerWindow = new Register();
            registerWindow.Show();
            this.Close();
        }
    }
}
