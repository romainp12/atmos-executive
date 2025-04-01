-- phpMyAdmin SQL Dump
-- version 3.3.9.2
-- http://www.phpmyadmin.net
--
-- Host: 127.0.0.1
-- Generation Time: Apr 01, 2025 at 02:59 PM
-- Server version: 5.5.10
-- PHP Version: 5.3.6

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `gestionlocationavions`
--

-- --------------------------------------------------------

--
-- Table structure for table `avion`
--

CREATE TABLE IF NOT EXISTS `avion` (
  `id_avion` int(11) NOT NULL AUTO_INCREMENT,
  `id_modele` int(11) NOT NULL,
  `annee` int(11) NOT NULL,
  `immatriculation` varchar(20) NOT NULL,
  `image_url` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_avion`),
  UNIQUE KEY `immatriculation` (`immatriculation`),
  KEY `id_modele` (`id_modele`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=18 ;

--
-- Dumping data for table `avion`
--

INSERT INTO `avion` (`id_avion`, `id_modele`, `annee`, `immatriculation`, `image_url`) VALUES
(11, 11, 2023, 'F-LUXE', 'https://www.aeroflap.com.br/wp-content/uploads/2023/09/Gulfstream-G700-free-use-scaled.jpg'),
(12, 12, 2022, 'F-GLOB', 'https://aeroaffaires.fr/wp-content/uploads/2019/06/capture-dcran-2019-06-17--15-30-52.png'),
(13, 13, 2023, 'F-FALC', 'https://i.f1g.fr/media/eidos/1200x630_crop/2021/05/06/XVM658f59fa-ae75-11eb-96fa-78334c940b20.jpg'),
(14, 14, 2021, 'F-FLCN', 'https://www.europair.com/storage/app/uploads/public/5f0/326/635/5f0326635fc1c816725357.jpg'),
(15, 15, 2022, 'F-CITE', 'https://africair.com/wp-content/uploads/2020/01/EBACE_Longitude.jpg'),
(16, 16, 2021, 'F-EMGC', 'https://aeroaffaires.fr/wp-content/uploads/2021/07/15_legacy_500_0118-scaled.jpeg'),
(17, 17, 2022, 'F-PILT', 'https://www.flyingmag.com/uploads/2025/02/FLY10_24_FEAT1-scaled.jpg?auto=webp');

-- --------------------------------------------------------

--
-- Table structure for table `marque`
--

CREATE TABLE IF NOT EXISTS `marque` (
  `id_marque` int(11) NOT NULL AUTO_INCREMENT,
  `nom_marque` varchar(50) NOT NULL,
  PRIMARY KEY (`id_marque`),
  UNIQUE KEY `nom_marque` (`nom_marque`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=8 ;

--
-- Dumping data for table `marque`
--

INSERT INTO `marque` (`id_marque`, `nom_marque`) VALUES
(2, 'Bombardier'),
(1, 'Cessna'),
(3, 'Dassault'),
(4, 'Embraer'),
(7, 'Gulfstream'),
(5, 'Pilatus');

-- --------------------------------------------------------

--
-- Table structure for table `modele`
--

CREATE TABLE IF NOT EXISTS `modele` (
  `id_modele` int(11) NOT NULL AUTO_INCREMENT,
  `nom_modele` varchar(50) NOT NULL,
  `id_marque` int(11) NOT NULL,
  `capacite` int(11) NOT NULL,
  `vitesse_max` int(11) DEFAULT NULL,
  `autonomie` int(11) DEFAULT NULL,
  PRIMARY KEY (`id_modele`),
  KEY `id_marque` (`id_marque`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=19 ;

--
-- Dumping data for table `modele`
--

INSERT INTO `modele` (`id_modele`, `nom_modele`, `id_marque`, `capacite`, `vitesse_max`, `autonomie`) VALUES
(11, 'G700', 7, 19, 982, 13890),
(12, 'Global 7500', 2, 19, 982, 14260),
(13, 'Falcon 10X', 3, 16, 1100, 13900),
(14, 'Falcon 8X', 3, 16, 956, 11945),
(15, 'Citation Longitude', 1, 12, 895, 6482),
(16, 'Legacy 500', 4, 12, 863, 5788),
(17, 'PC-24', 5, 10, 815, 3704);

-- --------------------------------------------------------

--
-- Table structure for table `reservation`
--

CREATE TABLE IF NOT EXISTS `reservation` (
  `id_reservation` int(11) NOT NULL AUTO_INCREMENT,
  `id_utilisateur` int(11) NOT NULL,
  `id_avion` int(11) NOT NULL,
  `date_debut` datetime NOT NULL,
  `date_fin` datetime NOT NULL,
  `statut` varchar(20) DEFAULT 'À venir',
  `date_creation` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_reservation`),
  KEY `id_utilisateur` (`id_utilisateur`),
  KEY `idx_reservation_dates` (`id_avion`,`date_debut`,`date_fin`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=28 ;

--
-- Dumping data for table `reservation`
--

INSERT INTO `reservation` (`id_reservation`, `id_utilisateur`, `id_avion`, `date_debut`, `date_fin`, `statut`, `date_creation`) VALUES
(16, 6, 12, '2025-03-27 12:00:00', '2025-03-28 08:00:00', 'Annulée', '2025-03-25 19:32:36'),
(17, 6, 12, '2025-03-28 18:00:00', '2025-03-29 10:00:00', 'Passée', '2025-03-25 19:47:08'),
(19, 6, 12, '2025-04-24 08:00:00', '2025-04-25 08:00:00', 'Annulée', '2025-03-30 16:58:55'),
(21, 6, 12, '2025-04-25 08:00:00', '2025-04-26 08:00:00', 'Annulée', '2025-03-30 22:49:15'),
(22, 6, 14, '2025-05-03 08:00:00', '2025-05-05 08:00:00', 'À venir', '2025-03-30 23:08:35'),
(23, 6, 12, '2025-05-02 13:00:00', '2025-05-03 08:00:00', 'Annulée', '2025-03-31 11:50:59'),
(24, 6, 15, '2025-04-01 08:00:00', '2025-04-02 12:00:00', 'En cours', '2025-04-01 00:53:37'),
(27, 6, 14, '2025-04-07 08:00:00', '2025-04-08 08:00:00', 'À venir', '2025-04-01 13:48:13');

-- --------------------------------------------------------

--
-- Table structure for table `utilisateurs`
--

CREATE TABLE IF NOT EXISTS `utilisateurs` (
  `id_utilisateur` int(11) NOT NULL AUTO_INCREMENT,
  `nom` varchar(50) NOT NULL,
  `prenom` varchar(50) NOT NULL,
  `email` varchar(100) NOT NULL,
  `mot_de_passe` varchar(255) NOT NULL,
  `telephone` varchar(15) DEFAULT NULL,
  `est_admin` tinyint(1) DEFAULT '0',
  `date_creation` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_utilisateur`),
  UNIQUE KEY `email` (`email`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=12 ;

--
-- Dumping data for table `utilisateurs`
--

INSERT INTO `utilisateurs` (`id_utilisateur`, `nom`, `prenom`, `email`, `mot_de_passe`, `telephone`, `est_admin`, `date_creation`) VALUES
(5, 'Pereira', 'Romain', 'admin@gmail.com', '$2a$11$o.4ZxHT8Q1IEC2LEVDE0ueeW7YV5GoNssjOmmZrM2qhudgmpKIRVm', '0678656565', 1, '2025-03-25 01:08:57'),
(6, 'Chaili', 'Yanis', 'user@gmail.com', '$2a$11$cHVB94QhXRpoU080IB1kBu3Hj8GfVzQQ.zrmcIQq6hvDXKvavUqd2', '0678761290', 0, '2025-03-25 01:10:09');

--
-- Constraints for dumped tables
--

--
-- Constraints for table `avion`
--
ALTER TABLE `avion`
  ADD CONSTRAINT `avion_ibfk_1` FOREIGN KEY (`id_modele`) REFERENCES `modele` (`id_modele`) ON DELETE CASCADE;

--
-- Constraints for table `modele`
--
ALTER TABLE `modele`
  ADD CONSTRAINT `modele_ibfk_1` FOREIGN KEY (`id_marque`) REFERENCES `marque` (`id_marque`) ON DELETE CASCADE;

--
-- Constraints for table `reservation`
--
ALTER TABLE `reservation`
  ADD CONSTRAINT `reservation_ibfk_1` FOREIGN KEY (`id_utilisateur`) REFERENCES `utilisateurs` (`id_utilisateur`) ON DELETE CASCADE,
  ADD CONSTRAINT `reservation_ibfk_2` FOREIGN KEY (`id_avion`) REFERENCES `avion` (`id_avion`) ON DELETE CASCADE;