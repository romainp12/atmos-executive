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
    /// Interaction logic for UserReservations.xaml
    /// </summary>
    public partial class UserReservations : Window

    {

        private bool dataInitialized = false;
        public UserReservations()
        {
            try
            {
                InitializeComponent();

                cbFiltreStatut.SelectedIndex = 0;

                this.Loaded += (s, e) => {
                    try
                    {
                        // Chargement initial des réservations
                        ChargerReservations();
                    }
                    catch (Exception ex)
                    {
                        AfficherErreur("Erreur lors du chargement initial des réservations", ex);
                    }
                };
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur lors de l'initialisation", ex);
            }
        }

        // Méthode pour afficher une erreur
        private void AfficherErreur(string message, Exception ex)
        {
            MessageBox.Show($"{message}: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Méthode pour charger les réservations de l'utilisateur
        private void ChargerReservations(string filtreStatut = null)
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    // Construction de la requête selon le filtre
                    string whereClause = "WHERE r.id_utilisateur = @IdUtilisateur";
                    if (!string.IsNullOrEmpty(filtreStatut) && filtreStatut != "Toutes les réservations")
                    {
                        whereClause += " AND r.statut = @Statut";
                    }

                    // Requête pour récupérer les réservations avec les informations associées
                    string query = $@"
                    SELECT r.id_reservation, r.date_debut, r.date_fin, r.statut,
                           a.immatriculation, mo.nom_modele, ma.nom_marque
                    FROM Reservation r
                    INNER JOIN Avion a ON r.id_avion = a.id_avion
                    INNER JOIN Modele mo ON a.id_modele = mo.id_modele
                    INNER JOIN Marque ma ON mo.id_marque = ma.id_marque
                    {whereClause}
                    ORDER BY r.date_debut DESC";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdUtilisateur", App.CurrentUserId);
                        if (!string.IsNullOrEmpty(filtreStatut) && filtreStatut != "Toutes les réservations")
                        {
                            command.Parameters.AddWithValue("@Statut", filtreStatut);
                        }

                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Mise à jour du statut des réservations
                        MettreAJourStatutReservations(connection, dataTable);

                        // Préparation des données pour l'affichage
                        List<ReservationViewModel> reservations = new List<ReservationViewModel>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            string statut = row["statut"].ToString();
                            DateTime dateDebut = Convert.ToDateTime(row["date_debut"]);
                            DateTime dateFin = Convert.ToDateTime(row["date_fin"]);

                            reservations.Add(new ReservationViewModel
                            {
                                IdReservation = Convert.ToInt32(row["id_reservation"]),
                                AvionDescription = $"{row["nom_marque"]} {row["nom_modele"]} ({row["immatriculation"]})",
                                DateDebut = dateDebut,
                                DateFin = dateFin,
                                Statut = statut,
                                EstAnnulable = statut != "Passée" && statut != "En cours" && statut != "Annulée",
                                // Ajouter la durée de réservation
                                DureeReservation = CalculerDureeReservation(dateDebut, dateFin)
                            });
                        }

                        // Affichage des données dans le DataGrid
                        icReservations.ItemsSource = reservations;
                    }
                }
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur lors du chargement des réservations", ex);
            }
        }

        // Méthode pour calculer la durée de réservation
        private string CalculerDureeReservation(DateTime debut, DateTime fin)
        {
            TimeSpan duree = fin - debut;

            if (duree.TotalDays >= 1)
            {
                int jours = (int)duree.TotalDays;
                int heures = duree.Hours;
                return $"{jours} jour{(jours > 1 ? "s" : "")} {heures} heure{(heures > 1 ? "s" : "")}";
            }
            else
            {
                int heures = (int)duree.TotalHours;
                int minutes = duree.Minutes;
                return $"{heures} heure{(heures > 1 ? "s" : "")} {minutes} minute{(minutes > 1 ? "s" : "")}";
            }
        }

        // Méthode pour mettre à jour le statut des réservations
        private void MettreAJourStatutReservations(MySqlConnection connection, DataTable reservations)
        {
            try
            {
                DateTime maintenant = DateTime.Now;

                foreach (DataRow row in reservations.Rows)
                {
                    int idReservation = Convert.ToInt32(row["id_reservation"]);
                    DateTime dateDebut = Convert.ToDateTime(row["date_debut"]);
                    DateTime dateFin = Convert.ToDateTime(row["date_fin"]);
                    string statutActuel = row["statut"].ToString();

                    // Ne pas mettre à jour les réservations annulées
                    if (statutActuel == "Annulée")
                        continue;

                    string nouveauStatut = statutActuel;

                    // Détermination du nouveau statut
                    if (maintenant < dateDebut)
                    {
                        nouveauStatut = "À venir";
                    }
                    else if (maintenant >= dateDebut && maintenant <= dateFin)
                    {
                        nouveauStatut = "En cours";
                    }
                    else if (maintenant > dateFin)
                    {
                        nouveauStatut = "Passée";
                    }

                    // Mise à jour du statut en base de données si nécessaire
                    if (nouveauStatut != statutActuel)
                    {
                        row["statut"] = nouveauStatut;

                        string updateQuery = "UPDATE Reservation SET statut = @Statut WHERE id_reservation = @IdReservation";
                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Statut", nouveauStatut);
                            updateCommand.Parameters.AddWithValue("@IdReservation", idReservation);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur lors de la mise à jour des statuts", ex);
            }
        }

        // Événement de changement de filtre
        private void cbFiltreStatut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignorer l'événement s'il est déclenché lors du chargement initial
            if (cbFiltreStatut.IsLoaded && IsLoaded && icReservations.IsLoaded && e.Source == cbFiltreStatut)
            {
                try
                {
                    ComboBoxItem selectedItem = cbFiltreStatut.SelectedItem as ComboBoxItem;
                    if (selectedItem != null)
                    {
                        string filtreStatut = selectedItem.Content.ToString();
                        ChargerReservations(filtreStatut);
                    }
                }
                catch (Exception ex)
                {
                    AfficherErreur("Erreur lors du changement de filtre", ex);
                }
            }
        }

        // Événement de clic sur le bouton Actualiser
        private void btnActualiser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBoxItem selectedItem = cbFiltreStatut.SelectedItem as ComboBoxItem;
                string filtreStatut = selectedItem?.Content.ToString() ?? "Toutes les réservations";
                ChargerReservations(filtreStatut);
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur lors de l'actualisation", ex);
            }
        }

        // Événement de clic sur le bouton Annuler
        private void btnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int idReservation = (int)btn.Tag;

                // Utilisation du MessageBox personnalisé pour la confirmation d'annulation
                bool confirme = MessageBoxService.ShowConfirmation(
                    "Êtes-vous sûr de vouloir annuler cette réservation ?",
                    "Confirmation d'annulation");

                if (confirme)
                {
                    using (MySqlConnection connection = ConnectionDB.GetConnection())
                    {
                        connection.Open();

                        string query = "UPDATE Reservation SET statut = 'Annulée' WHERE id_reservation = @IdReservation";

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@IdReservation", idReservation);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // Utilisation du MessageBox personnalisé pour confirmer l'annulation
                                MessageBoxService.ShowInfo(
                                    "La réservation a été annulée avec succès.",
                                    "Annulation confirmée");

                                // Rechargement des réservations
                                ComboBoxItem selectedItem = cbFiltreStatut.SelectedItem as ComboBoxItem;
                                string filtreStatut = selectedItem?.Content.ToString() ?? "Toutes les réservations";
                                ChargerReservations(filtreStatut);
                            }
                            else
                            {
                                // Utilisation du MessageBox personnalisé pour l'erreur
                                MessageBoxService.ShowError(
                                    "Impossible d'annuler la réservation. Veuillez réessayer.",
                                    "Erreur d'annulation");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur lors de l'annulation de la réservation", ex);
            }
        }

        // Événement de clic sur le bouton Fermer
        private void btnFermer_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Classe pour stocker les données d'affichage des réservations
    public class ReservationViewModel
    {
        public int IdReservation { get; set; }
        public string AvionDescription { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string Statut { get; set; }
        public bool EstAnnulable { get; set; }
        public string DureeReservation { get; set; }
    }
}