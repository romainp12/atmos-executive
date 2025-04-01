using Google.Protobuf;
using System;
using System.Windows;
using System.Windows.Media;

namespace GestionLocationAvions.Utils
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public bool IsConfirmed { get; private set; }
        public CustomMessageBox(string message, string title, CustomMessageBoxType type)
        {
            InitializeComponent();

            // Définir le titre et le message
            this.Title = title;
            txtMessage.Text = message;

            // Configurer l'apparence en fonction du type
            ConfigureAppearance(type);

            // Centrer la fenêtre
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void ConfigureAppearance(CustomMessageBoxType type)
        {
            switch (type)
            {
                case CustomMessageBoxType.Error:
                    // Configuration pour les erreurs
                    iconPath.Data = Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z");
                    iconPath.Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // #E74C3C
                    headerBorder.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // #E74C3C
                    btnOK.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // #E74C3C
                    btnCancel.Visibility = Visibility.Collapsed;
                    break;

                case CustomMessageBoxType.Info:
                    // Configuration pour les infos
                    iconPath.Data = Geometry.Parse("M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
                    iconPath.Fill = new SolidColorBrush(Color.FromRgb(41, 128, 185)); // #2980B9
                    headerBorder.Background = new SolidColorBrush(Color.FromRgb(41, 128, 185)); // #2980B9
                    btnOK.Background = new SolidColorBrush(Color.FromRgb(41, 128, 185)); // #2980B9
                    btnCancel.Visibility = Visibility.Collapsed;
                    break;

                case CustomMessageBoxType.Confirmation:
                    // Configuration pour les confirmations
                    iconPath.Data = Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L18,9.5L16.59,8.09L11,13.67L7.91,10.59L6.5,12L11,16.5Z");
                    iconPath.Fill = new SolidColorBrush(Color.FromRgb(39, 174, 96)); // #27AE60
                    headerBorder.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)); // #27AE60
                    btnOK.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)); // #27AE60
                    btnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}
