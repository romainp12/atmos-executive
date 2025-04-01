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
    /// Interaction logic for AdminMarques.xaml
    /// </summary>
    public partial class AdminMarques : Window
    {
        private int? _idMarqueEnEdition = null;
        public AdminMarques()
        {
            InitializeComponent();

            // Chargement des marques
            ChargerMarques();
        }

        // Méthode pour charger les marques
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
                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Préparation des données pour l'affichage
                        List<MarqueViewModel> marques = new List<MarqueViewModel>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            marques.Add(new MarqueViewModel
                            {
                                IdMarque = Convert.ToInt32(row["id_marque"]),
                                NomMarque = row["nom_marque"].ToString()
                            });
                        }

                        // Affichage des données dans le DataGrid
                        icMarques.ItemsSource = marques;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowError("Erreur lors du chargement des marques : " + ex.Message, "Erreur");

            }
        }

        // Événement de clic sur le bouton Ajouter/Modifier
        private void btnAjouter_Click(object sender, RoutedEventArgs e)
        {
            string nomMarque = txtNomMarque.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(nomMarque))
            {
                MessageBoxService.ShowError("Veuillez entrer un nom de marque.", "Information");

                return;
            }

            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    // Vérification si la marque existe déjà
                    string queryVerif = "SELECT COUNT(*) FROM Marque WHERE nom_marque = @NomMarque";
                    if (_idMarqueEnEdition.HasValue)
                    {
                        queryVerif += " AND id_marque <> @IdMarque";
                    }

                    using (MySqlCommand commandVerif = new MySqlCommand(queryVerif, connection))
                    {
                        commandVerif.Parameters.AddWithValue("@NomMarque", nomMarque);
                        if (_idMarqueEnEdition.HasValue)
                        {
                            commandVerif.Parameters.AddWithValue("@IdMarque", _idMarqueEnEdition.Value);
                        }

                        long count = (long)commandVerif.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBoxService.ShowError("Cette marque existe déjà.", "Information");
                            return;
                        }
                    }

                    // Ajout ou modification de la marque
                    string query;
                    if (_idMarqueEnEdition.HasValue)
                    {
                        // Modification
                        query = "UPDATE Marque SET nom_marque = @NomMarque WHERE id_marque = @IdMarque";
                    }
                    else
                    {
                        // Ajout
                        query = "INSERT INTO Marque (nom_marque) VALUES (@NomMarque)";
                    }

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NomMarque", nomMarque);
                        if (_idMarqueEnEdition.HasValue)
                        {
                            command.Parameters.AddWithValue("@IdMarque", _idMarqueEnEdition.Value);
                        }

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            string message = _idMarqueEnEdition.HasValue ? "Marque modifiée avec succès." : "Marque ajoutée avec succès.";
                            MessageBoxService.ShowInfo(message, "Succès");


                            // Réinitialisation du formulaire
                            ResetForm();

                            // Rechargement des marques
                            ChargerMarques();
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
            int idMarque = (int)btn.Tag;

            try
            {
                using (MySqlConnection connection = ConnectionDB.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT nom_marque FROM Marque WHERE id_marque = @IdMarque";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdMarque", idMarque);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                _idMarqueEnEdition = idMarque;
                                txtNomMarque.Text = reader["nom_marque"].ToString();
                                btnAjouter.Content = "Modifier";
                                btnAnnuler.Visibility = Visibility.Visible;
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

        // Événement de clic sur le bouton Supprimer
        private void btnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int idMarque = (int)btn.Tag;

            // Demande de confirmation
            if (MessageBoxService.ShowConfirmation(
    "Êtes-vous sûr de vouloir supprimer cette marque ? Cette action supprimera également tous les modèles et avions associés.",
    "Confirmation"))
            {
                try
                {
                    using (MySqlConnection connection = ConnectionDB.GetConnection())
                    {
                        connection.Open();

                        // Vérification des dépendances
                        string queryVerif = "SELECT COUNT(*) FROM Reservation r " +
                                           "INNER JOIN Avion a ON r.id_avion = a.id_avion " +
                                           "INNER JOIN Modele mo ON a.id_modele = mo.id_modele " +
                                           "WHERE mo.id_marque = @IdMarque AND r.statut IN ('En cours', 'À venir')";

                        using (MySqlCommand commandVerif = new MySqlCommand(queryVerif, connection))
                        {
                            commandVerif.Parameters.AddWithValue("@IdMarque", idMarque);

                            long count = (long)commandVerif.ExecuteScalar();

                            if (count > 0)
                            {
                                MessageBoxService.ShowError("Impossible de supprimer cette marque car des réservations en cours ou à venir existent pour des avions de cette marque.", "Erreur");
                                return;
                            }
                        }

                        // Suppression de la marque (CASCADE supprimera également les modèles et avions)
                        string queryDelete = "DELETE FROM Marque WHERE id_marque = @IdMarque";

                        using (MySqlCommand commandDelete = new MySqlCommand(queryDelete, connection))
                        {
                            commandDelete.Parameters.AddWithValue("@IdMarque", idMarque);

                            int resultDelete = commandDelete.ExecuteNonQuery();

                            if (resultDelete > 0)
                            {
                                MessageBoxService.ShowInfo("Marque supprimée avec succès.", "Succès");

                                // Rechargement des marques
                                ChargerMarques();
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
            _idMarqueEnEdition = null;
            txtNomMarque.Text = string.Empty;
            btnAjouter.Content = "Ajouter";
            btnAnnuler.Visibility = Visibility.Collapsed;
        }
    }

    // Classe pour stocker les données d'affichage des marques
    public class MarqueViewModel
    {
        public int IdMarque { get; set; }
        public string NomMarque { get; set; }
    }
}