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
    /// Interaction logic for AdminReservations.xaml
    /// </summary>
    public partial class AdminReservations : Window
    {

        private string _filtreStatut = null;
        private int? _filtreClientId = null;
        private int? _filtreAvionId = null;
        public AdminReservations()
        {
            InitializeComponent();


            // Chargement des clients pour le filtre
            ChargerClients();

            // Chargement des avions pour le filtre
            ChargerAvions();

            // Charger les réservations une fois que le contrôle est chargé
            this.Loaded += (s, e) => {
                try
                {
                    // Chargement initial des réservations
                    ChargerReservations();
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowError("Erreur lors du chargement initial des réservations : " + ex.Message, "Erreur");
                }
            };
        }

        // Méthode pour charger les clients dans le combobox de filtre
        private void ChargerClients()
        {
            try
            {
                cbFiltreClient.Items.Clear();

                // Ajout d'un élément "Tous les clients"
                cbFiltreClient.Items.Add(new ComboBoxItem
                {
                    Content = "Tous les clients",
                    Tag = null
                });

                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();
                    string sql = @"
                        SELECT 
                            id_utilisateur, 
                            CONCAT(prenom, ' ', nom, ' (', email, ')') AS nom_complet
                        FROM 
                            Utilisateurs
                        ORDER BY 
                            nom, prenom";

                    using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int idUtilisateur = Convert.ToInt32(reader["id_utilisateur"]);
                                string nomComplet = reader["nom_complet"].ToString();

                                cbFiltreClient.Items.Add(new ComboBoxItem
                                {
                                    Content = nomComplet,
                                    Tag = idUtilisateur
                                });
                            }
                        }
                    }
                }

                if (cbFiltreClient.Items.Count > 0)
                {
                    cbFiltreClient.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des clients : " + ex.Message, "Erreur");
            }
        }

        private void ChargerAvions()
        {
            try
            {
                cbFiltreAvion.Items.Clear();

                // Ajout d'un élément "Tous les avions"
                cbFiltreAvion.Items.Add(new ComboBoxItem
                {
                    Content = "Tous les avions",
                    Tag = null
                });

                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();
                    string sql = @"
                        SELECT 
                            a.id_avion, 
                            CONCAT(ma.nom_marque, ' ', mo.nom_modele, ' (', a.immatriculation, ')') AS description_avion
                        FROM 
                            Avion a
                            JOIN Modele mo ON a.id_modele = mo.id_modele
                            JOIN Marque ma ON mo.id_marque = ma.id_marque
                        ORDER BY 
                            ma.nom_marque, mo.nom_modele, a.immatriculation";

                    using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int idAvion = Convert.ToInt32(reader["id_avion"]);
                                string descriptionAvion = reader["description_avion"].ToString();

                                cbFiltreAvion.Items.Add(new ComboBoxItem
                                {
                                    Content = descriptionAvion,
                                    Tag = idAvion
                                });
                            }
                        }
                    }
                }

                if (cbFiltreAvion.Items.Count > 0)
                {
                    cbFiltreAvion.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des avions : " + ex.Message, "Erreur");
            }
        }

        private void ChargerReservations()
        {
            try
            {
                List<AdminReservationViewModel> reservations = new List<AdminReservationViewModel>();

                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    // Mise à jour des statuts des réservations
                    MettreAJourStatuts(connection);

                    // Construction de la requête SQL de base
                    string sql = @"
                        SELECT 
                            r.id_reservation, 
                            r.date_debut, 
                            r.date_fin, 
                            r.statut, 
                            r.date_creation,
                            u.id_utilisateur, 
                            CONCAT(u.prenom, ' ', u.nom, ' (', u.email, ')') AS nom_client,
                            a.id_avion, 
                            CONCAT(ma.nom_marque, ' ', mo.nom_modele, ' (', a.immatriculation, ')') AS description_avion
                        FROM 
                            Reservation r
                            JOIN Utilisateurs u ON r.id_utilisateur = u.id_utilisateur
                            JOIN Avion a ON r.id_avion = a.id_avion
                            JOIN Modele mo ON a.id_modele = mo.id_modele
                            JOIN Marque ma ON mo.id_marque = ma.id_marque
                        WHERE 1=1";

                    // Construction des paramètres pour les filtres
                    List<MySqlParameter> parameters = new List<MySqlParameter>();

                    // Filtrage par statut
                    if (!string.IsNullOrEmpty(_filtreStatut) && _filtreStatut != "Toutes les réservations")
                    {
                        sql += " AND r.statut = @Statut";
                        parameters.Add(new MySqlParameter("@Statut", _filtreStatut));
                    }

                    // Filtrage par client
                    if (_filtreClientId.HasValue)
                    {
                        sql += " AND r.id_utilisateur = @IdUtilisateur";
                        parameters.Add(new MySqlParameter("@IdUtilisateur", _filtreClientId.Value));
                    }

                    // Filtrage par avion
                    if (_filtreAvionId.HasValue)
                    {
                        sql += " AND r.id_avion = @IdAvion";
                        parameters.Add(new MySqlParameter("@IdAvion", _filtreAvionId.Value));
                    }

                    sql += " ORDER BY r.date_debut DESC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                    {
                        // Ajout des paramètres
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string statut = reader["statut"].ToString();
                                DateTime dateDebut = Convert.ToDateTime(reader["date_debut"]);
                                DateTime dateFin = Convert.ToDateTime(reader["date_fin"]);

                                reservations.Add(new AdminReservationViewModel
                                {
                                    IdReservation = Convert.ToInt32(reader["id_reservation"]),
                                    IdClient = Convert.ToInt32(reader["id_utilisateur"]),
                                    NomClient = reader["nom_client"].ToString(),
                                    IdAvion = Convert.ToInt32(reader["id_avion"]),
                                    AvionDescription = reader["description_avion"].ToString(),
                                    DateDebut = dateDebut,
                                    DateFin = dateFin,
                                    Statut = statut,
                                    DateCreation = Convert.ToDateTime(reader["date_creation"]),
                                    EstAnnulable = statut != "Passée" && statut != "Annulée" && statut != "En cours"
                                });
                            }
                        }
                    }
                }

                // Affichage des données dans le DataGrid
                if (icReservations != null)
                {
                    icReservations.ItemsSource = reservations;

                    // Afficher un message si aucune réservation n'est trouvée
                    if (reservations.Count == 0)
                    {
                        string message = "Aucune réservation ne correspond aux critères de filtrage.";
                        if (_filtreStatut == null && _filtreClientId == null && _filtreAvionId == null)
                        {
                            message = "Aucune réservation n'existe dans le système.";
                        }

                        MessageBoxService.ShowInfo(message, "Information");
                    }
                }
                else
                {
                    MessageBoxService.ShowError("Erreur : Le contrôle DataGrid n'est pas initialisé ", "Erreur");
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des réservations : " + ex.Message, "Erreur");
            }
        }

        private void MettreAJourStatuts(MySqlConnection connection)
        {
            try
            {
                // Réservations à venir -> En cours
                string sql1 = @"
                    UPDATE Reservation 
                    SET statut = 'En cours' 
                    WHERE statut = 'À venir' 
                    AND NOW() BETWEEN date_debut AND date_fin";

                using (MySqlCommand cmd = new MySqlCommand(sql1, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Réservations en cours -> Passées
                string sql2 = @"
                    UPDATE Reservation 
                    SET statut = 'Passée' 
                    WHERE statut = 'En cours' 
                    AND NOW() > date_fin";

                using (MySqlCommand cmd = new MySqlCommand(sql2, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la mise à jour des statuts : " + ex.Message);
                // On ne relance pas l'exception pour ne pas interrompre le chargement
            }
        }

        private void cbFiltreStatut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignorer l'événement s'il est déclenché lors du chargement initial
            if (cbFiltreStatut.IsLoaded && IsLoaded && icReservations.IsLoaded && e.Source == cbFiltreStatut)
            {
                try
                {
                    if (cbFiltreStatut.SelectedItem != null)
                    {
                        string statut = ((ComboBoxItem)cbFiltreStatut.SelectedItem).Content.ToString();
                        _filtreStatut = statut == "Toutes les réservations" ? null : statut;
                        ChargerReservations();
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowError("Erreur lors du changement de filtre : " + ex.Message, "Erreur");
                }
            }
        }

        private void cbFiltreClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignorer l'événement s'il est déclenché lors du chargement initial
            if (cbFiltreClient.IsLoaded && IsLoaded && icReservations.IsLoaded && e.Source == cbFiltreClient)
            {
                try
                {
                    if (cbFiltreClient.SelectedItem != null)
                    {
                        _filtreClientId = ((ComboBoxItem)cbFiltreClient.SelectedItem).Tag as int?;
                        ChargerReservations();
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowError("Erreur lors du changement de filtre client : " + ex.Message, "Erreur");
                }
            }
        }

        private void cbFiltreAvion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignorer l'événement s'il est déclenché lors du chargement initial
            if (cbFiltreAvion.IsLoaded && IsLoaded && icReservations.IsLoaded && e.Source == cbFiltreAvion)
            {
                try
                {
                    if (cbFiltreAvion.SelectedItem != null)
                    {
                        _filtreAvionId = ((ComboBoxItem)cbFiltreAvion.SelectedItem).Tag as int?;
                        ChargerReservations();
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowError("Erreur lors du changement de filtre avion : " + ex.Message, "Erreur");
                }
            }
        }

        private void btnReinitialiserFiltres_Click(object sender, RoutedEventArgs e)
        {
            cbFiltreStatut.SelectedIndex = 0;
            cbFiltreClient.SelectedIndex = 0;
            cbFiltreAvion.SelectedIndex = 0;

            _filtreStatut = null;
            _filtreClientId = null;
            _filtreAvionId = null;

            ChargerReservations();
        }

        private void btnActualiser_Click(object sender, RoutedEventArgs e)
        {
            ChargerReservations();
        }

        private void btnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier que le bouton a un tag
            if (sender is Button btn && btn.Tag != null)
            {
                int idReservation = Convert.ToInt32(btn.Tag);

                if (MessageBoxService.ShowConfirmation(
    "Êtes-vous sûr de vouloir annuler cette réservation ?",
    "Confirmation"))
                {
                    try
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
                                    MessageBoxService.ShowInfo("La réservation a été annulée avec succès.", "Succès");
                                    // Rechargement des réservations
                                    ChargerReservations();
                                }
                                else
                                {
                                    MessageBoxService.ShowInfo("Aucune réservation n'a été annulée", "Information");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBoxService.ShowError("Erreur lors de l'annulation de la réservation : " + ex.Message, "Erreur");
                    }
                }
            }
        }
    }

    public class AdminReservationViewModel
    {
        public int IdReservation { get; set; }
        public int IdClient { get; set; }
        public string NomClient { get; set; }
        public int IdAvion { get; set; }
        public string AvionDescription { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public bool EstAnnulable { get; set; }
    }
}