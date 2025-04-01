using System;
using System.Windows;

namespace GestionLocationAvions
{
    public partial class App : Application
    {
        // Propriétés pour stocker les informations de l'utilisateur connecté
        public static int CurrentUserId { get; set; }
        public static string CurrentUserName { get; set; }
        public static string Initials { get; set; }
        public static bool IsAdmin { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Affichage de la fenêtre de connexion au démarrage
            Login loginWindow = new Login();
            loginWindow.Show();
        }
    }
}