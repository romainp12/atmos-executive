using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;
using GestionLocationAvions.Database;
using GestionLocationAvions.Utils;

namespace GestionLocationAvions
{
    /// <summary>
    /// Interaction logic for ClientDashboard.xaml
    /// </summary>
    public partial class ClientDashboard : Window
    {
        public ClientDashboard()
        {
            InitializeComponent();

            // Affichage du nom de l'utilisateur connecté
            txtUserName.Text = "Bienvenue " + App.CurrentUserName;
            txtInitials.Text = App.Initials;


            // Chargement des avions
            ChargerAvions();
        }

        // Méthode pour charger les avions disponibles
        private void ChargerAvions()
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    // Requête pour récupérer les avions avec les informations associées
                    string query = @"
                    SELECT a.id_avion, a.immatriculation, a.annee, a.image_url, 
                           mo.nom_modele, mo.capacite, mo.vitesse_max, mo.autonomie, 
                           ma.nom_marque
                    FROM Avion a
                    INNER JOIN Modele mo ON a.id_modele = mo.id_modele
                    INNER JOIN Marque ma ON mo.id_marque = ma.id_marque
                    WHERE a.id_avion NOT IN (
                        SELECT r.id_avion 
                        FROM Reservation r 
                        WHERE r.statut IN ('En cours', 'À venir')
                        AND ((NOW() BETWEEN r.date_debut AND r.date_fin) 
                             OR (DATE_ADD(NOW(), INTERVAL 1 DAY) BETWEEN r.date_debut AND r.date_fin))
                    )";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Préparation des données pour l'affichage
                        List<AvionViewModel> avions = new List<AvionViewModel>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            avions.Add(new AvionViewModel
                            {
                                IdAvion = Convert.ToInt32(row["id_avion"]),
                                MarqueNom = row["nom_marque"].ToString(),
                                ModeleNom = row["nom_modele"].ToString(),
                                Immatriculation = row["immatriculation"].ToString(),
                                Annee = Convert.ToInt32(row["annee"]),
                                Capacite = Convert.ToInt32(row["capacite"]),
                                VitesseMax = row["vitesse_max"] != DBNull.Value ? Convert.ToInt32(row["vitesse_max"]) : 0,
                                Autonomie = row["autonomie"] != DBNull.Value ? Convert.ToInt32(row["autonomie"]) : 0,
                                ImageUrl = row["image_url"] != DBNull.Value ? row["image_url"].ToString() : null
                            });
                        }

                        // Affichage des données dans le DataGrid
                        planesItemsControl.ItemsSource = avions;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des avions : " + ex.Message, "Erreur");

            }
        }

        // Événement de clic sur le bouton de réservation
        private void btnReserver_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int idAvion = (int)btn.Tag;

            // Création de la fenêtre de réservation
            ReservationView reservationView = new ReservationView(idAvion);
            reservationView.Owner = this;
            bool? result = reservationView.ShowDialog();

            // Rechargement des avions après la réservation
            if (result == true)
            {
                ChargerAvions();
            }
        }

        // Événement de clic sur le bouton "Mes réservations"
        private void btnMesReservations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Création et affichage directe (sans ShowDialog)
                UserReservations userReservations = new UserReservations();
                userReservations.Owner = this;
                // Ajout d'un gestionnaire d'événements pour capturer les erreurs
                userReservations.Loaded += (s, args) => {
                    try
                    {
                        // La fenêtre est maintenant chargée
                        Console.WriteLine("Fenêtre des réservations chargée avec succès.");
                    }
                    catch (Exception ex)
                    {
                        MessageBoxService.ShowError("Erreur après chargement : " + ex.Message, "Erreur");

                    }
                };

                // Passage en Show au lieu de ShowDialog pour éviter le blocage
                userReservations.Show();
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors de l'ouverture des réservations : " + ex.Message, "Erreur");

            }
        }

        // Événement de clic sur le bouton de déconnexion
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

        public class AvionViewModel
        {
            public int IdAvion { get; set; }
            public string MarqueNom { get; set; }
            public string ModeleNom { get; set; }
            public string Immatriculation { get; set; }
            public int Annee { get; set; }
            public int Capacite { get; set; }
            public int VitesseMax { get; set; }
            public int Autonomie { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}