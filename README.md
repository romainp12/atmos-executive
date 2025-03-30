# ✈️ Atmos Executive

Cette application bureau développée en C# et WPF permet de gérer un service de location d'avions privés. Elle offre une interface moderne et intuitive pour les clients souhaitant réserver des avions, ainsi qu'un panel d'administration pour gérer l'ensemble du parc aérien.

## 📋 Table des matières

- [Présentation du projet](#-présentation-du-projet)
- [Fonctionnalités](#-fonctionnalités)
- [Architecture de l'application](#-architecture-de-lapplication)
- [Installation](#-installation)
- [Profils utilisateurs](#-profils-utilisateurs)
- [Technologies utilisées](#️-technologies-utilisées)

## 🎯 Présentation du projet

Ce projet a été développé dans le cadre de l'épreuve E6 du BTS SIO option SLAM. Il s'agit d'une application de gestion de location d'avions privés pour une entreprise fictive, Atmos Executive.
L'application permet la gestion complète du cycle de réservation, depuis la consultation des avions disponibles jusqu'à la gestion administrative du parc d'avions, en offrant une interface riche et intuitive aux utilisateurs.

L'application est divisée en deux interfaces principales :
- Une interface client pour la recherche et la réservation d'avions
- Une interface d'administration pour la gestion des avions, des modèles, des marques et des réservations

## ✨ Fonctionnalités

### Utilisateurs (Clients)
- Création de compte et authentification
- Consultation du catalogue d'avions disponibles
- Réservation d'un avion pour une période donnée
- Gestion des réservations personnelles (consultation, annulation)
- Interface intuitive avec visualisation des caractéristiques des avions

### Administrateurs
- Gestion complète du parc d'avions (ajout, modification, suppression)
- Gestion des modèles et des marques d'avions
- Supervision des réservations de tous les clients
- Tableaux de bord avec statistiques (nombre d'avions, réservations en cours, etc.)
- Validation et annulation des réservations

## 🏗️ Architecture de l'application

L'application est structurée avec une séparation claire entre les données, la logique métier et l'interface utilisateur.

```
GestionLocationAvions/
│
├── Database/                      # Couche d'accès aux données
│   └── ConnectionDB.cs            # Gestion de la connexion à la base de données et opérations de sécurité
│
├── Utils/                         # Utilitaires et services communs
│   ├── MessageBoxService.cs       # Service de messagerie personnalisée
│   ├── CustomMessageBox.xaml      # Interface des boîtes de dialogue personnalisées
│   └── CustomMessageBox.xaml.cs   # Logique des boîtes de dialogue personnalisées
│
├── Modèles d'interface admin      # Interfaces de gestion administrateur
│   ├── AdminDashboard.xaml        # Tableau de bord principal admin
│   ├── AdminAvions.xaml           # Gestion des avions
│   ├── AdminMarques.xaml          # Gestion des marques d'avions
│   ├── AdminModeles.xaml          # Gestion des modèles d'avions
│   └── AdminReservations.xaml     # Gestion des réservations
│ 
├── Modèles d'interface client     # Interfaces utilisateur client
│   ├── ClientDashboard.xaml       # Tableau de bord client
│   ├── UserReservations.xaml      # Gestion des réservations utilisateur
│   └── ReservationView.xaml       # Interface de création de réservation
│
├── Authentification               # Gestion des utilisateurs et authentification
│   ├── Login.xaml                 # Interface de connexion
│   └── Register.xaml              # Interface d'inscription
│
└── App.xaml                       # Point d'entrée de l'application
```

## 💻 Installation

### Prérequis
- Windows 10 ou supérieur
- .NET Framework 4.7.2 ou supérieur / .NET 8.0
- MySQL Server 5.7 ou supérieur

### Étapes d'installation

1. **Clonez ou téléchargez le projet**
   ```bash
   git clone https://github.com/romainp12/atmos-executive.git
   ```

2. **Configurez la base de données**
   - Créez une base de données MySQL nommée `gestionlocationavions`
   - Importez le fichier SQL fourni pour créer les tables et les données initiales
   - Modifiez le fichier `ConnectionDB.cs` pour configurer la connexion à votre base de données :
   ```csharp
   private static string connectionString = "Server=localhost;Database=gestionlocationavions;Uid=votre_utilisateur;Pwd=votre_mot_de_passe;";
   ```

3. **Compilez et exécutez l'application**
   - Ouvrez la solution dans Visual Studio
   - Restaurez les packages NuGet si nécessaire
   - Compilez et lancez l'application

## 👥 Profils utilisateurs

L'application gère deux types d'utilisateurs :

### Client
- Peut consulter les avions disponibles
- Peut réserver un avion pour une période donnée
- Peut gérer ses propres réservations (consulter, annuler)
- Accès limité à ses propres données

### Administrateur
- Accès à toutes les fonctionnalités client
- Gestion complète du parc d'avions (CRUD)
- Gestion des marques et modèles
- Supervision de toutes les réservations
- Accès aux statistiques et tableaux de bord

## 🔐 Comptes de démonstration

Voici les identifiants pour tester l'application :

| Type de compte | Email | Mot de passe |
|----------------|-------|--------------|
| Administrateur | admin@gmail.com | admin |
| Client | user@gmail.com | user |

## 🛠️ Technologies utilisées

- **Frontend** : C# WPF (Windows Presentation Foundation) avec XAML
- **Backend** : C# .NET
- **Base de données** : MySQL
- **ORM** : ADO.NET avec des requêtes SQL directes
- **Sécurité** : BCrypt pour le hachage des mots de passe

## 🔄 Fonctionnalités de sécurité

- **Protection des mots de passe** : Utilisation de BCrypt pour le hachage des mots de passe
- **Validation des entrées** : Vérification systématique des saisies utilisateur
- **Contrôle d'accès** : Séparation claire des interfaces admin et client
- **Protection contre les suppressions accidentelles** : Vérification des dépendances avant suppression

## 📦 Dépendances

- MySql.Data (8.0.32 ou supérieur)
- BCrypt.Net-Next (4.0.3 ou supérieur)
- BouncyCastle.Crypto (1.9.0 ou supérieur)

## 📝 Notes de développement

- L'application utilise une interface moderne avec des effets visuels avancés (ombres, animations, etc.)
- Le design respecte les principes du Material Design pour une expérience utilisateur optimale
- Les messages d'erreur et de confirmation sont personnalisés pour une meilleure expérience utilisateur
- Le système prévient automatiquement la suppression d'avions, de modèles ou de marques impliqués dans des réservations actives
