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
    /// Interaction logic for AdminAvions.xaml
    /// </summary>
    public partial class AdminAvions : Window
    {
        private int? _idAvionEnEdition = null;
        private Dictionary<int, string> _marques = new Dictionary<int, string>();
        private Dictionary<int, ModeleData> _modeles = new Dictionary<int, ModeleData>();

        // Classe pour stocker les données des modèles
        private class ModeleData
        {
            public int IdModele { get; set; }
            public string NomModele { get; set; }
            public int IdMarque { get; set; }
            public int Capacite { get; set; }
        }

        public AdminAvions()
        {
            InitializeComponent();

            // Chargement des marques pour le combobox
            ChargerMarques();

            // Chargement des avions
            ChargerAvions();
        }

        // Méthode pour charger les marques dans le combobox
        private void ChargerMarques()
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT id_marque, nom_marque FROM Marque ORDER BY nom_marque";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            cbMarque.Items.Clear();
                            _marques.Clear();

                            while (reader.Read())
                            {
                                int idMarque = Convert.ToInt32(reader["id_marque"]);
                                string nomMarque = reader["nom_marque"].ToString();

                                _marques.Add(idMarque, nomMarque);
                                cbMarque.Items.Add(new ComboBoxItem
                                {
                                    Content = nomMarque,
                                    Tag = idMarque
                                });
                            }

                            if (cbMarque.Items.Count > 0)
                            {
                                cbMarque.SelectedIndex = 0;
                            }
                            else
                            {
                                // Si aucune marque n'est trouvée, désactiver le formulaire
                                MessageBoxService.ShowError("Aucune marque n'est disponible. Veuillez d'abord créer des marques", "Information");
                            }
                        }
                    }

                    // Chargement des modèles pour toutes les marques
                    ChargerTousModeles(connection);
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des marques : " + ex.Message, "Erreur");
            }
        }

        // Méthode pour charger tous les modèles (pour toutes les marques)
        private void ChargerTousModeles(MySqlConnection connection)
        {
            string query = "SELECT id_modele, nom_modele, id_marque, capacite FROM Modele ORDER BY nom_modele";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    _modeles.Clear();

                    while (reader.Read())
                    {
                        int idModele = Convert.ToInt32(reader["id_modele"]);
                        string nomModele = reader["nom_modele"].ToString();
                        int idMarque = Convert.ToInt32(reader["id_marque"]);
                        int capacite = Convert.ToInt32(reader["capacite"]);

                        _modeles.Add(idModele, new ModeleData
                        {
                            IdModele = idModele,
                            NomModele = nomModele,
                            IdMarque = idMarque,
                            Capacite = capacite
                        });
                    }
                }
            }

            // Mise à jour du combobox des modèles selon la marque sélectionnée
            MettreAJourComboboxModeles();
        }

        // Méthode pour mettre à jour le combobox des modèles selon la marque sélectionnée
        private void MettreAJourComboboxModeles()
        {
            cbModele.Items.Clear();

            if (cbMarque.SelectedItem != null)
            {
                int idMarqueSelectionnee = (int)((ComboBoxItem)cbMarque.SelectedItem).Tag;
                bool modelesTrouves = false;

                foreach (var modele in _modeles.Values)
                {
                    if (modele.IdMarque == idMarqueSelectionnee)
                    {
                        cbModele.Items.Add(new ComboBoxItem
                        {
                            Content = modele.NomModele,
                            Tag = modele.IdModele
                        });
                        modelesTrouves = true;
                    }
                }

                if (modelesTrouves)
                {
                    cbModele.SelectedIndex = 0;

                    // Activer les champs si des modèles sont trouvés
                    txtImmatriculation.IsEnabled = true;
                    txtAnnee.IsEnabled = true;
                    txtImageUrl.IsEnabled = true;
                    btnAjouter.IsEnabled = true;
                }
                else
                {
                    // Ajouter un élément indiquant qu'il n'y a pas de modèle disponible
                    cbModele.Items.Add(new ComboBoxItem
                    {
                        Content = "-- Pas de modèle disponible --",
                        Tag = -1
                    });
                    cbModele.SelectedIndex = 0;

                    // Désactiver les champs pour éviter l'ajout d'un avion sans modèle valide
                    txtImmatriculation.IsEnabled = false;
                    txtAnnee.IsEnabled = false;
                    txtImageUrl.IsEnabled = false;
                    btnAjouter.IsEnabled = false;
                }
            }
        }

        // Méthode pour charger les avions
        private void ChargerAvions()
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    // Requête modifiée pour éviter l'erreur de champ inconnu et inclure image_url
                    string query = @"
            SELECT a.id_avion, a.immatriculation, a.annee, a.id_modele, a.image_url,
                   mo.nom_modele, mo.capacite, ma.id_marque, ma.nom_marque,
                   CASE 
                       WHEN EXISTS (
                           SELECT 1
                           FROM Reservation r
                           WHERE r.id_avion = a.id_avion AND r.statut = 'En cours'
                       ) THEN 'Réservé'
                       ELSE 'Disponible'
                   END AS statut
            FROM Avion a
            INNER JOIN Modele mo ON a.id_modele = mo.id_modele
            INNER JOIN Marque ma ON mo.id_marque = ma.id_marque
            ORDER BY ma.nom_marque, mo.nom_modele, a.immatriculation";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Préparation des données pour l'affichage
                        List<AdminAvionViewModel> avions = new List<AdminAvionViewModel>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            avions.Add(new AdminAvionViewModel
                            {
                                IdAvion = Convert.ToInt32(row["id_avion"]),
                                IdModele = Convert.ToInt32(row["id_modele"]),
                                IdMarque = Convert.ToInt32(row["id_marque"]),
                                NomMarque = row["nom_marque"].ToString(),
                                NomModele = row["nom_modele"].ToString(),
                                Immatriculation = row["immatriculation"].ToString(),
                                Annee = Convert.ToInt32(row["annee"]),
                                Capacite = Convert.ToInt32(row["capacite"]),
                                Statut = row["statut"].ToString(),
                                ImageUrl = row["image_url"] != DBNull.Value ? row["image_url"].ToString() : null
                            });
                        }

                        // Affichage des données dans le DataGrid
                        icAvions.ItemsSource = avions;


                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des avions : " + ex.Message, "Erreur");
            }
        }

        // Événement de changement de sélection de marque
        private void cbMarque_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MettreAJourComboboxModeles();
        }

        // Événement de clic sur le bouton Ajouter/Modifier
        private void btnAjouter_Click(object sender, RoutedEventArgs e)
        {
            // Récupération des valeurs
            if (cbModele.SelectedItem == null)
            {
                MessageBoxService.ShowError("Veuillez sélectionner un modèle", "Information");
                return;
            }

            int idModele = (int)((ComboBoxItem)cbModele.SelectedItem).Tag;
            string immatriculation = txtImmatriculation.Text.Trim().ToUpper();
            string imageUrl = txtImageUrl.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(immatriculation))
            {
                MessageBoxService.ShowError("Veuillez entrez une immatriculation", "Information");
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(immatriculation, @"^[A-Z]-[A-Z0-9]{4}$"))
            {
                MessageBoxService.ShowError("L'immatriculation doit être au format X-XXXX (ex: F-GXYZ)", "Information");
                return;
            }

            if (!int.TryParse(txtAnnee.Text, out int annee) || annee < 1950 || annee > DateTime.Now.Year)
            {
                MessageBoxService.ShowError("Veuillez entrer une année valide (entre 1950 et l'année actuelle).", "Information");
                return;
            }

            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    // Vérification si l'immatriculation existe déjà
                    string queryVerif = "SELECT COUNT(*) FROM Avion WHERE immatriculation = @Immatriculation";
                    if (_idAvionEnEdition.HasValue)
                    {
                        queryVerif += " AND id_avion <> @IdAvion";
                    }

                    using (MySqlCommand commandVerif = new MySqlCommand(queryVerif, connection))
                    {
                        commandVerif.Parameters.AddWithValue("@Immatriculation", immatriculation);
                        if (_idAvionEnEdition.HasValue)
                        {
                            commandVerif.Parameters.AddWithValue("@IdAvion", _idAvionEnEdition.Value);
                        }

                        long count = (long)commandVerif.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBoxService.ShowError("Cette immatriculation existe déjà.", "Information");
                            return;
                        }
                    }

                    // Ajout ou modification de l'avion
                    string query;
                    if (_idAvionEnEdition.HasValue)
                    {
                        // Modification
                        query = @"
                        UPDATE Avion 
                        SET id_modele = @IdModele, 
                            immatriculation = @Immatriculation,
                            annee = @Annee,
                            image_url = @ImageUrl
                        WHERE id_avion = @IdAvion";
                    }
                    else
                    {
                        // Ajout
                        query = @"
                        INSERT INTO Avion (id_modele, immatriculation, annee, image_url) 
                        VALUES (@IdModele, @Immatriculation, @Annee, @ImageUrl)";
                    }

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdModele", idModele);
                        command.Parameters.AddWithValue("@Immatriculation", immatriculation);
                        command.Parameters.AddWithValue("@Annee", annee);
                        command.Parameters.AddWithValue("@ImageUrl", string.IsNullOrEmpty(imageUrl) ? (object)DBNull.Value : imageUrl);

                        if (_idAvionEnEdition.HasValue)
                        {
                            command.Parameters.AddWithValue("@IdAvion", _idAvionEnEdition.Value);
                        }

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            string message = _idAvionEnEdition.HasValue ? "Avion modifié avec succès." : "Avion ajouté avec succès.";
                            MessageBoxService.ShowInfo(message, "Succès");

                            // Réinitialisation du formulaire
                            ResetForm();

                            // Rechargement des avions
                            ChargerAvions();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur : " + ex.Message, "Erreur");
            }
        }

        // Événement de clic sur le bouton Modifier
        private void btnModifier_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int idAvion = (int)btn.Tag;

            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    string query = @"
            SELECT a.id_modele, a.immatriculation, a.annee, a.image_url, mo.id_marque
            FROM Avion a
            INNER JOIN Modele mo ON a.id_modele = mo.id_modele
            WHERE a.id_avion = @IdAvion";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdAvion", idAvion);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                _idAvionEnEdition = idAvion;
                                int idMarque = Convert.ToInt32(reader["id_marque"]);
                                int idModele = Convert.ToInt32(reader["id_modele"]);

                                // Sélection de la marque dans le ComboBox
                                for (int i = 0; i < cbMarque.Items.Count; i++)
                                {
                                    ComboBoxItem item = (ComboBoxItem)cbMarque.Items[i];
                                    if ((int)item.Tag == idMarque)
                                    {
                                        cbMarque.SelectedIndex = i;
                                        break;
                                    }
                                }

                                // Mise à jour des modèles et sélection du modèle dans le ComboBox
                                for (int i = 0; i < cbModele.Items.Count; i++)
                                {
                                    ComboBoxItem item = (ComboBoxItem)cbModele.Items[i];
                                    if ((int)item.Tag == idModele)
                                    {
                                        cbModele.SelectedIndex = i;
                                        break;
                                    }
                                }

                                // Remplissage du formulaire
                                txtImmatriculation.Text = reader["immatriculation"].ToString();
                                txtAnnee.Text = reader["annee"].ToString();
                                txtImageUrl.Text = reader["image_url"] != DBNull.Value ? reader["image_url"].ToString() : "";

                                // S'assurer que les champs sont activés
                                txtImmatriculation.IsEnabled = true;
                                txtAnnee.IsEnabled = true;
                                txtImageUrl.IsEnabled = true;
                                btnAjouter.IsEnabled = true;

                                btnAjouter.Content = "Modifier";
                                btnAnnuler.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur : " + ex.Message, "Erreur");
            }
        }

        // Événement de clic sur le bouton Supprimer
        private void btnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int idAvion = (int)btn.Tag;

            // Demande de confirmation
            if (MessageBoxService.ShowConfirmation("Êtes-vous sûr de vouloir supprimer cet avion ?", "Confirmation"))
            {
                try
                {
                    using (MySqlConnection connection = ConnectionDB.GetConnection())
                    {
                        connection.Open();

                        // Vérification des dépendances
                        string queryVerif = "SELECT COUNT(*) FROM Reservation WHERE id_avion = @IdAvion AND statut IN ('En cours', 'À venir')";

                        using (MySqlCommand commandVerif = new MySqlCommand(queryVerif, connection))
                        {
                            commandVerif.Parameters.AddWithValue("@IdAvion", idAvion);

                            long count = (long)commandVerif.ExecuteScalar();

                            if (count > 0)
                            {
                                MessageBoxService.ShowError("Impossible de supprimer cet avion car des réservations en cours ou à venir existent pour cet avion.", "Erreur");
                                return;
                            }
                        }

                        // Suppression de l'avion
                        string queryDelete = "DELETE FROM Avion WHERE id_avion = @IdAvion";

                        using (MySqlCommand commandDelete = new MySqlCommand(queryDelete, connection))
                        {
                            commandDelete.Parameters.AddWithValue("@IdAvion", idAvion);

                            int resultDelete = commandDelete.ExecuteNonQuery();

                            if (resultDelete > 0)
                            {
                                MessageBoxService.ShowInfo("Avion supprimé avec succès.", "Succès");

                                // Rechargement des avions
                                ChargerAvions();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowError("Erreur lors de la suppression : " + ex.Message, "Erreur");
                }
            }
        }

        // Événement de clic sur le bouton Annuler
        private void btnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        // Méthode pour réinitialiser le formulaire
        private void ResetForm()
        {
            _idAvionEnEdition = null;
            if (cbMarque.Items.Count > 0)
            {
                cbMarque.SelectedIndex = 0;
            }
            txtImmatriculation.Text = string.Empty;
            txtAnnee.Text = string.Empty;
            txtImageUrl.Text = string.Empty;
            btnAjouter.Content = "Ajouter";
            btnAnnuler.Visibility = Visibility.Collapsed;
        }
    }

    // Classe pour stocker les données d'affichage des avions
    public class AdminAvionViewModel
    {
        public int IdAvion { get; set; }
        public int IdModele { get; set; }
        public int IdMarque { get; set; }
        public string NomMarque { get; set; }
        public string NomModele { get; set; }
        public string Immatriculation { get; set; }
        public int Annee { get; set; }
        public int Capacite { get; set; }
        public string Statut { get; set; }
        public string ImageUrl { get; set; }
    }
}