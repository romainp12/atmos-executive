using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GestionLocationAvions.Utils
{
    public static class MessageBoxService
    {
        /// <summary>
        /// Affiche un message d'erreur avec un style personnalisé
        /// </summary>
        /// <param name="message">Le message d'erreur à afficher</param>
        /// <param name="title">Le titre de la fenêtre (optionnel)</param>
        public static void ShowError(string message, string title = "Erreur")
        {
            CustomMessageBox messageBox = new CustomMessageBox(
                message,
                title,
                CustomMessageBoxType.Error);

            messageBox.ShowDialog();
        }

        /// <summary>
        /// Affiche un message d'information avec un style personnalisé
        /// </summary>
        /// <param name="message">Le message d'information à afficher</param>
        /// <param name="title">Le titre de la fenêtre (optionnel)</param>
        public static void ShowInfo(string message, string title = "Information")
        {
            CustomMessageBox messageBox = new CustomMessageBox(
                message,
                title,
                CustomMessageBoxType.Info);

            messageBox.ShowDialog();
        }

        /// <summary>
        /// Affiche un message de confirmation avec un style personnalisé
        /// </summary>
        /// <param name="message">Le message de confirmation à afficher</param>
        /// <param name="title">Le titre de la fenêtre (optionnel)</param>
        /// <returns>True si l'utilisateur a confirmé, sinon False</returns>
        public static bool ShowConfirmation(string message, string title = "Confirmation")
        {
            CustomMessageBox messageBox = new CustomMessageBox(
                message,
                title,
                CustomMessageBoxType.Confirmation);

            messageBox.ShowDialog();

            return messageBox.IsConfirmed;
        }
    }

    public enum CustomMessageBoxType
    {
        Error,
        Info,
        Confirmation
    }
}