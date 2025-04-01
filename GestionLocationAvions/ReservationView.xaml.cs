using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;
using GestionLocationAvions.Database;
using System.Windows.Media.Imaging;
using GestionLocationAvions.Utils;

namespace GestionLocationAvions
{
    /// <summary>
    /// Interaction logic for ReservationView.xaml
    /// </summary>
    public partial class ReservationView : Window
    {
        private int _idAvion;
        private DateTime _dateDebut;
        private DateTime _dateFin;
        public ReservationView(int idAvion)
        {
            InitializeComponent();

            _idAvion = idAvion;

            // Chargement des informations de l'avion
            ChargerInfosAvion();

            // Initialisation des contrôles de date et heure
            InitialiserControlesDateTime();
        }

        // Nouvelle méthode pour afficher les erreurs
        private void AfficherErreur(string message)
        {
            txtErreur.Text = message;
            txtErreur.Visibility = string.IsNullOrEmpty(message)
                ? System.Windows.Visibility.Collapsed
                : System.Windows.Visibility.Visible;
        }

        // Chargement des informations de l'avion sélectionné
        private void ChargerInfosAvion()
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                    SELECT a.immatriculation, a.annee, a.image_url, 
                           mo.nom_modele, mo.capacite, 
                           ma.nom_marque
                    FROM Avion a
                    INNER JOIN Modele mo ON a.id_modele = mo.id_modele
                    INNER JOIN Marque ma ON mo.id_marque = ma.id_marque
                    WHERE a.id_avion = @IdAvion";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdAvion", _idAvion);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Affichage des informations
                                txtMarque.Text = reader["nom_marque"].ToString();
                                txtModele.Text = reader["nom_modele"].ToString();
                                txtImmatriculation.Text = reader["immatriculation"].ToString();
                                txtCapacite.Text = reader["capacite"].ToString() + " passagers";
                                txtAnnee.Text = reader["annee"].ToString();

                                // Chargement de l'image
                                if (reader["image_url"] != DBNull.Value && !string.IsNullOrEmpty(reader["image_url"].ToString()))
                                {
                                    try
                                    {
                                        imgAvion.Source = new BitmapImage(new Uri(reader["image_url"].ToString()));
                                    }
                                    catch (Exception)
                                    {
                                        // En cas d'erreur de chargement d'image, on ne fait rien
                                    }
                                }
                            }
                            else
                            {
                                MessageBoxService.ShowError("Avion non trouvé.", "Erreur");
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des informations : " + ex.Message, "Erreur");
                this.Close();
            }
        }

        // Initialisation des contrôles de date et heure
        private void InitialiserControlesDateTime()
        {
            // Définir la date d'aujourd'hui par défaut
            dpDateDebut.SelectedDate = DateTime.Today;
            dpDateFin.SelectedDate = DateTime.Today.AddDays(1);

            // Initialisation des heures (de 8h à 20h)
            for (int i = 8; i <= 20; i++)
            {
                cbHeureDebut.Items.Add(i.ToString("00"));
                cbHeureFin.Items.Add(i.ToString("00"));
            }

            // Initialisation des minutes (par pas de 15min)
            for (int i = 0; i < 60; i += 15)
            {
                cbMinuteDebut.Items.Add(i.ToString("00"));
                cbMinuteFin.Items.Add(i.ToString("00"));
            }

            // Sélectionner des horaires par défaut
            cbHeureDebut.SelectedIndex = 0; // 8h
            cbHeureFin.SelectedIndex = 0;   // 8h
            cbMinuteDebut.SelectedIndex = 0; // 00 minutes
            cbMinuteFin.SelectedIndex = 0;   // 00 minutes

            // Effacer tout message d'erreur ou de durée
            txtDuree.Text = "";
            AfficherErreur("");

            // Mettre à jour la durée initialement
            try
            {
                RecupererDateHeureSelectionnees();
                if (VerifierValiditeDuree())
                {
                    AfficherDuree();
                }
            }
            catch
            {
                // Ignorer les erreurs lors de l'initialisation
            }
        }

        // Récupération des dates et heures sélectionnées
        private bool RecupererDateHeureSelectionnees()
        {
            DateTime? dateDebut = dpDateDebut.SelectedDate;
            DateTime? dateFin = dpDateFin.SelectedDate;

            // Vérification que les dates sont sélectionnées
            if (!dateDebut.HasValue)
            {
                AfficherErreur("Veuillez sélectionner une date de début.");
                return false;
            }

            if (!dateFin.HasValue)
            {
                AfficherErreur("Veuillez sélectionner une date de fin.");
                return false;
            }

            // Vérification que les heures sont sélectionnées
            if (cbHeureDebut.SelectedItem == null)
            {
                AfficherErreur("Veuillez sélectionner une heure de début.");
                return false;
            }

            if (cbMinuteDebut.SelectedItem == null)
            {
                AfficherErreur("Veuillez sélectionner les minutes de début.");
                return false;
            }

            if (cbHeureFin.SelectedItem == null)
            {
                AfficherErreur("Veuillez sélectionner une heure de fin.");
                return false;
            }

            if (cbMinuteFin.SelectedItem == null)
            {
                AfficherErreur("Veuillez sélectionner les minutes de fin.");
                return false;
            }

            // Récupération des valeurs
            string heureDebut = cbHeureDebut.SelectedItem.ToString();
            string minuteDebut = cbMinuteDebut.SelectedItem.ToString();
            string heureFin = cbHeureFin.SelectedItem.ToString();
            string minuteFin = cbMinuteFin.SelectedItem.ToString();

            _dateDebut = dateDebut.Value.Date.AddHours(int.Parse(heureDebut)).AddMinutes(int.Parse(minuteDebut));
            _dateFin = dateFin.Value.Date.AddHours(int.Parse(heureFin)).AddMinutes(int.Parse(minuteFin));

            return true;
        }

        // Vérification de la validité de la durée
        private bool VerifierValiditeDuree()
        {
            // Vérification si les dates sont valides
            if (_dateDebut < DateTime.Now)
            {
                AfficherErreur("La date de début doit être future.");
                return false;
            }

            if (_dateFin <= _dateDebut)
            {
                AfficherErreur("La date de fin doit être postérieure à la date de début.");
                return false;
            }

            return true;
        }

        // Calcul et affichage de la durée
        private void AfficherDuree()
        {
            TimeSpan duree = _dateFin - _dateDebut;

            int jours = duree.Days;
            int heures = duree.Hours;
            int minutes = duree.Minutes;

            string texte = "Durée: ";
            if (jours > 0)
                texte += jours + " jour" + (jours > 1 ? "s" : "") + " ";
            if (heures > 0 || jours > 0)
                texte += heures + " heure" + (heures > 1 ? "s" : "") + " ";
            texte += minutes + " minute" + (minutes > 1 ? "s" : "");

            txtDuree.Text = texte;
            txtDuree.Foreground = System.Windows.Media.Brushes.Black;
        }

        // Vérification de la disponibilité de l'avion
        private bool VerifierDisponibilite()
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                    SELECT COUNT(id_reservation) 
                    FROM Reservation 
                    WHERE id_avion = @IdAvion 
                      AND statut IN ('En cours', 'À venir')
                      AND ((@DateDebut BETWEEN date_debut AND date_fin)
                           OR (@DateFin BETWEEN date_debut AND date_fin)
                           OR (date_debut BETWEEN @DateDebut AND @DateFin))";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdAvion", _idAvion);
                        command.Parameters.AddWithValue("@DateDebut", _dateDebut);
                        command.Parameters.AddWithValue("@DateFin", _dateFin);

                        long count = (long)command.ExecuteScalar();
                        return count == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors de la vérification de disponibilité : " + ex.Message, "Erreur");
                return false;
            }
        }

        // Création de la réservation
        private bool CreerReservation()
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                    INSERT INTO Reservation (id_utilisateur, id_avion, date_debut, date_fin, statut) 
                    VALUES (@IdUtilisateur, @IdAvion, @DateDebut, @DateFin, 'À venir')";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdUtilisateur", App.CurrentUserId);
                        command.Parameters.AddWithValue("@IdAvion", _idAvion);
                        command.Parameters.AddWithValue("@DateDebut", _dateDebut);
                        command.Parameters.AddWithValue("@DateFin", _dateFin);

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors de la création de la réservation : " + ex.Message, "Erreur");
                return false;
            }
        }

        // Événements pour les boutons
        private void btnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnConfirmer_Click(object sender, RoutedEventArgs e)
        {
            // Effacer les messages précédents
            AfficherErreur("");

            // Récupérer les dates et heures sélectionnées
            if (!RecupererDateHeureSelectionnees())
                return;

            // Vérifier si les dates sont valides
            if (!VerifierValiditeDuree())
                return;

            // Afficher la durée
            AfficherDuree();

            // Vérification de la disponibilité
            if (!VerifierDisponibilite())
            {
                AfficherErreur("Cet avion n'est pas disponible pour la période sélectionnée.");
                return;
            }

            // Création de la réservation
            if (CreerReservation())
            {
                // Utilisation du nouveau MessageBox pour afficher la confirmation
                MessageBoxService.ShowInfo("Votre réservation a été confirmée.", "Réservation réussie");
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                AfficherErreur("Une erreur est survenue lors de la réservation.");
            }
        }

        // Méthode pour mettre à jour la durée
        private void MettreAJourDuree()
        {
            AfficherErreur("");

            try
            {
                if (RecupererDateHeureSelectionnees() && VerifierValiditeDuree())
                {
                    AfficherDuree();
                }
            }
            catch
            {
                // En cas d'erreur, ne rien faire (les contrôles ne sont peut-être pas encore tous initialisés)
            }
        }

        // Événements de changement de date et d'heure
        private void dpDateDebut_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            MettreAJourDuree();
        }

        private void dpDateFin_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            MettreAJourDuree();
        }

        private void cbHeureDebut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MettreAJourDuree();
        }

        private void cbHeureFin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MettreAJourDuree();
        }

        private void cbMinuteDebut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MettreAJourDuree();
        }

        private void cbMinuteFin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MettreAJourDuree();
        }
    }
}