using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using GestionLocationAvions.Database;
using GestionLocationAvions.Utils;

namespace GestionLocationAvions
{
    /// <summary>
    /// Interaction logic for AdminDashboard.xaml
    /// </summary>
    public partial class AdminDashboard : Window
    {
        public AdminDashboard()
        {
            InitializeComponent();

            // Affichage du nom de l'utilisateur connecté
            txtUserName.Text = App.CurrentUserName;
            txtInitials.Text = App.Initials;

            // Chargement des statistiques
            ChargerStatistiques();
        }

        // Méthode pour charger les statistiques
        private void ChargerStatistiques()
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    // Nombre d'avions
                    string queryAvions = "SELECT COUNT(id_avion) FROM Avion";
                    using (MySqlCommand command = new MySqlCommand(queryAvions, connection))
                    {
                        long nombreAvions = (long)command.ExecuteScalar();
                        txtNombreAvions.Text = nombreAvions.ToString();
                    }

                    // Nombre d'utilisateurs (clients)
                    string queryUtilisateurs = "SELECT COUNT(id_utilisateur) FROM Utilisateurs WHERE est_admin = 0";
                    using (MySqlCommand command = new MySqlCommand(queryUtilisateurs, connection))
                    {
                        long nombreUtilisateurs = (long)command.ExecuteScalar();
                        txtNombreUtilisateurs.Text = nombreUtilisateurs.ToString();
                    }

                    // Réservations en cours
                    string queryReservationsEnCours = @"
                    SELECT COUNT(id_reservation) FROM Reservation 
                    WHERE statut = 'En cours'";
                    using (MySqlCommand command = new MySqlCommand(queryReservationsEnCours, connection))
                    {
                        long reservationsEnCours = (long)command.ExecuteScalar();
                        txtReservationsEnCours.Text = reservationsEnCours.ToString();
                    }

                    // Réservations à venir
                    string queryReservationsAVenir = @"
                    SELECT COUNT(id_reservation) FROM Reservation 
                    WHERE statut = 'À venir'";
                    using (MySqlCommand command = new MySqlCommand(queryReservationsAVenir, connection))
                    {
                        long reservationsAVenir = (long)command.ExecuteScalar();
                        txtReservationsAVenir.Text = reservationsAVenir.ToString();
                    }

                    // Réservations passées
                    string queryReservationsPassees = @"
                    SELECT COUNT(id_reservation) FROM Reservation 
                    WHERE statut = 'Passée'";
                    using (MySqlCommand command = new MySqlCommand(queryReservationsPassees, connection))
                    {
                        long reservationsPassees = (long)command.ExecuteScalar();
                        txtReservationsPassees.Text = reservationsPassees.ToString();
                    }

                    // Réservations annulées
                    string queryReservationsAnnulees = @"
                    SELECT COUNT(id_reservation) FROM Reservation 
                    WHERE statut = 'Annulée'";
                    using (MySqlCommand command = new MySqlCommand(queryReservationsAnnulees, connection))
                    {
                        long reservationsAnnulees = (long)command.ExecuteScalar();
                        txtReservationsAnnulees.Text = reservationsAnnulees.ToString();
                    }

                    // Mettre à jour le statut des réservations
                    MettreAJourStatutReservations(connection);
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des statistiques : " + ex.Message, "Erreur");

            }
        }

        // Méthode pour mettre à jour le statut des réservations
        private void MettreAJourStatutReservations(MySqlConnection connection)
        {
            try
            {
                // Mettre à jour les réservations 'À venir' vers 'En cours'
                string queryAVenirVersEnCours = @"
                UPDATE Reservation
                SET statut = 'En cours'
                WHERE statut = 'À venir'
                AND NOW() BETWEEN date_debut AND date_fin";
                using (MySqlCommand command = new MySqlCommand(queryAVenirVersEnCours, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Mettre à jour les réservations 'En cours' vers 'Passée'
                string queryEnCoursVersPassee = @"
                UPDATE Reservation
                SET statut = 'Passée'
                WHERE statut = 'En cours'
                AND NOW() > date_fin";
                using (MySqlCommand command = new MySqlCommand(queryEnCoursVersPassee, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors de la mise à jour des statuts : " + ex.Message, "Erreur");
            }
        }

        // Événement de clic sur le bouton Déconnexion
        private void btnDeconnexion_Click(object sender, RoutedEventArgs e)
        {
            // Réinitialisation des informations de session
            App.CurrentUserId = 0;
            App.CurrentUserName = null;
            App.IsAdmin = false;

            // Redirection vers la page de connexion
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }

        // Événement de clic sur le bouton de gestion des marques
        private void btnGestionMarques_Click(object sender, RoutedEventArgs e)
        {
            AdminMarques adminMarques = new AdminMarques();
            adminMarques.Owner = this;
            adminMarques.ShowDialog();

            // Rafraîchir les statistiques
            ChargerStatistiques();
        }

        // Événement de clic sur le bouton de gestion des modèles
        private void btnGestionModeles_Click(object sender, RoutedEventArgs e)
        {
            AdminModeles adminModeles = new AdminModeles();
            adminModeles.Owner = this;
            adminModeles.ShowDialog();

            // Rafraîchir les statistiques
            ChargerStatistiques();
        }

        // Événement de clic sur le bouton de gestion des avions
        private void btnGestionAvions_Click(object sender, RoutedEventArgs e)
        {
            AdminAvions adminAvions = new AdminAvions();
            adminAvions.Owner = this;
            adminAvions.ShowDialog();

            // Rafraîchir les statistiques
            ChargerStatistiques();
        }

        // Événement de clic sur le bouton de gestion des réservations
        private void btnGestionReservations_Click(object sender, RoutedEventArgs e)
        {
            AdminReservations adminReservations = new AdminReservations();
            adminReservations.Owner = this;
            adminReservations.ShowDialog();

            // Rafraîchir les statistiques
            ChargerStatistiques();
        }
    }
}
