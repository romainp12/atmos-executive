using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;
using GestionLocationAvions.Database;
using GestionLocationAvions.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace GestionLocationAvions
{
    /// <summary>
    /// Interaction logic for AdminModeles.xaml
    /// </summary>
    public partial class AdminModeles : Window
    {
        private int? _idModeleEnEdition = null;
        private Dictionary<int, string> _marques = new Dictionary<int, string>();
        public AdminModeles()
        {
            InitializeComponent();

            // Chargement des marques pour le combobox
            ChargerMarques();

            // Chargement des modèles
            ChargerModeles();
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des marques : " + ex.Message, "Erreur");
            }
        }

        // Méthode pour charger les modèles
        private void ChargerModeles()
        {
            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                    SELECT mo.id_modele, mo.nom_modele, mo.capacite, mo.vitesse_max, mo.autonomie, 
                           ma.id_marque, ma.nom_marque
                    FROM Modele mo
                    INNER JOIN Marque ma ON mo.id_marque = ma.id_marque
                    ORDER BY ma.nom_marque, mo.nom_modele";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Préparation des données pour l'affichage
                        List<ModeleViewModel> modeles = new List<ModeleViewModel>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            modeles.Add(new ModeleViewModel
                            {
                                IdModele = Convert.ToInt32(row["id_modele"]),
                                IdMarque = Convert.ToInt32(row["id_marque"]),
                                NomMarque = row["nom_marque"].ToString(),
                                NomModele = row["nom_modele"].ToString(),
                                Capacite = Convert.ToInt32(row["capacite"]),
                                VitesseMax = row["vitesse_max"] != DBNull.Value ? row["vitesse_max"].ToString() + " km/h" : "-",
                                Autonomie = row["autonomie"] != DBNull.Value ? row["autonomie"].ToString() + " km" : "-"
                            });
                        }

                        // Affichage des données dans le DataGrid
                        icModeles.ItemsSource = modeles;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des modèles : " + ex.Message, "Erreur");

            }
        }

        // Événement de clic sur le bouton Ajouter/Modifier
        private void btnAjouter_Click(object sender, RoutedEventArgs e)
        {
            // Récupération des valeurs
            if (cbMarque.SelectedItem == null)
            {
                MessageBoxService.ShowError("Veuillez sélectionner une marque.", "Information");
                return;
            }

            int idMarque = (int)((ComboBoxItem)cbMarque.SelectedItem).Tag;
            string nomModele = txtNomModele.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(nomModele))
            {
                MessageBoxService.ShowError("Veuillez entrer un nom de modèle.", "Information");

                return;
            }

            if (!int.TryParse(txtCapacite.Text, out int capacite) || capacite <= 0)
            {
                MessageBoxService.ShowError("Veuillez entrer une capacité valide(nombre entier positif).", "Information");
                return;
            }

            // Vitesse max et autonomie peuvent être null
            int? vitesseMax = null;
            int? autonomie = null;

            if (!string.IsNullOrEmpty(txtVitesseMax.Text) && int.TryParse(txtVitesseMax.Text, out int vitesse))
            {
                vitesseMax = vitesse;
            }

            if (!string.IsNullOrEmpty(txtAutonomie.Text) && int.TryParse(txtAutonomie.Text, out int auto))
            {
                autonomie = auto;
            }

            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    // Vérification si le modèle existe déjà pour cette marque
                    string queryVerif = "SELECT COUNT(*) FROM Modele WHERE nom_modele = @NomModele AND id_marque = @IdMarque";
                    if (_idModeleEnEdition.HasValue)
                    {
                        queryVerif += " AND id_modele <> @IdModele";
                    }

                    using (MySqlCommand commandVerif = new MySqlCommand(queryVerif, connection))
                    {
                        commandVerif.Parameters.AddWithValue("@NomModele", nomModele);
                        commandVerif.Parameters.AddWithValue("@IdMarque", idMarque);
                        if (_idModeleEnEdition.HasValue)
                        {
                            commandVerif.Parameters.AddWithValue("@IdModele", _idModeleEnEdition.Value);
                        }

                        long count = (long)commandVerif.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBoxService.ShowError("Ce modèle existe déjà pour cette marque.", "Information");

                            return;
                        }
                    }

                    // Ajout ou modification du modèle
                    string query;
                    if (_idModeleEnEdition.HasValue)
                    {
                        // Modification
                        query = @"
                        UPDATE Modele 
                        SET nom_modele = @NomModele, 
                            id_marque = @IdMarque, 
                            capacite = @Capacite, 
                            vitesse_max = @VitesseMax, 
                            autonomie = @Autonomie 
                        WHERE id_modele = @IdModele";
                    }
                    else
                    {
                        // Ajout
                        query = @"
                        INSERT INTO Modele (nom_modele, id_marque, capacite, vitesse_max, autonomie) 
                        VALUES (@NomModele, @IdMarque, @Capacite, @VitesseMax, @Autonomie)";
                    }

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NomModele", nomModele);
                        command.Parameters.AddWithValue("@IdMarque", idMarque);
                        command.Parameters.AddWithValue("@Capacite", capacite);
                        command.Parameters.AddWithValue("@VitesseMax", vitesseMax.HasValue ? (object)vitesseMax.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@Autonomie", autonomie.HasValue ? (object)autonomie.Value : DBNull.Value);

                        if (_idModeleEnEdition.HasValue)
                        {
                            command.Parameters.AddWithValue("@IdModele", _idModeleEnEdition.Value);
                        }

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            string message = _idModeleEnEdition.HasValue ? "Modèle modifié avec succès." : "Modèle ajouté avec succès.";
                            MessageBoxService.ShowInfo(message, "Succès");

                            // Réinitialisation du formulaire
                            ResetForm();

                            // Rechargement des modèles
                            ChargerModeles();
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
            int idModele = (int)btn.Tag;

            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    string query = @"
                    SELECT nom_modele, id_marque, capacite, vitesse_max, autonomie
                    FROM Modele
                    WHERE id_modele = @IdModele";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdModele", idModele);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                _idModeleEnEdition = idModele;

                                // Remplissage du formulaire
                                txtNomModele.Text = reader["nom_modele"].ToString();
                                int idMarque = Convert.ToInt32(reader["id_marque"]);

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

                                txtCapacite.Text = reader["capacite"].ToString();
                                txtVitesseMax.Text = reader["vitesse_max"] != DBNull.Value ? reader["vitesse_max"].ToString() : "";
                                txtAutonomie.Text = reader["autonomie"] != DBNull.Value ? reader["autonomie"].ToString() : "";

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
            int idModele = (int)btn.Tag;

            // Demande de confirmation

            if (MessageBoxService.ShowConfirmation(
    "Êtes-vous sûr de vouloir supprimer ce modèle ? Cette action supprimera également tous les avions associés.",
    "Confirmation"))
            {
                try
                {
                    using (MySqlConnection connection = ConnectionDB.GetConnection())
                    {
                        connection.Open();

                        // Vérification des dépendances
                        string queryVerif = @"
                        SELECT COUNT(*)
                        FROM Reservation r
                        INNER JOIN Avion a ON r.id_avion = a.id_avion
                        WHERE a.id_modele = @IdModele AND r.statut IN ('En cours', 'À venir')";

                        using (MySqlCommand commandVerif = new MySqlCommand(queryVerif, connection))
                        {
                            commandVerif.Parameters.AddWithValue("@IdModele", idModele);

                            long count = (long)commandVerif.ExecuteScalar();

                            if (count > 0)
                            {
                                MessageBoxService.ShowError("Impossible de supprimer ce modèle car des réservations en cours ou à venir existent pour des avions de ce modèle.", "Erreur");
                                return;
                            }
                        }

                        // Suppression du modèle (CASCADE supprimera également les avions)
                        string queryDelete = "DELETE FROM Modele WHERE id_modele = @IdModele";

                        using (MySqlCommand commandDelete = new MySqlCommand(queryDelete, connection))
                        {
                            commandDelete.Parameters.AddWithValue("@IdModele", idModele);

                            int resultDelete = commandDelete.ExecuteNonQuery();

                            if (resultDelete > 0)
                            {
                                MessageBoxService.ShowInfo("Modèle supprimé avec succès.", "Succès");

                                // Rechargement des modèles
                                ChargerModeles();
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
            _idModeleEnEdition = null;
            txtNomModele.Text = string.Empty;
            txtCapacite.Text = string.Empty;
            txtVitesseMax.Text = string.Empty;
            txtAutonomie.Text = string.Empty;
            if (cbMarque.Items.Count > 0)
            {
                cbMarque.SelectedIndex = 0;
            }
            btnAjouter.Content = "Ajouter";
            btnAnnuler.Visibility = Visibility.Collapsed;
        }
    }

    // Classe pour stocker les données d'affichage des modèles
    public class ModeleViewModel
    {
        public int IdModele { get; set; }
        public int IdMarque { get; set; }
        public string NomMarque { get; set; }
        public string NomModele { get; set; }
        public int Capacite { get; set; }
        public string VitesseMax { get; set; }
        public string Autonomie { get; set; }
    }
}