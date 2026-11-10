# Hand Tracking MiniJeu — Meta Quest 3

Branche : `Hand-Tracking-MiniJeu`.
Scène : `Assets/HandTrackingMiniJeu/Scenes/HandTrackingMiniJeu.unity`.

## Ce que contient la scène

- Un rig XR avec suivi des mains OpenXR/XR Hands et prise en charge des contrôleurs.
- Un écran affichant, pour chaque main, l'état de suivi, la position XYZ du poignet dans le monde (en mètres), la proximité du pouce et de l'index et une vue du dessus du squelette.
- Une table avec six objets physiques à saisir par pincement, déplacer et relâcher.
- Trois zones de tri : déposer les objets dans la zone de même couleur pour atteindre 6/6.
- Un bouton physique Replacer ; la touche R remplit la même fonction dans l'éditeur.
- Le retour automatique sur la table des objets tombés au sol. Un objet encore saisi n'est jamais déplacé par la remise à zéro.
- La borne d'arcade Snake, à droite de la table, avec son joystick et son bouton Start.
- Trois cibles derrière le joueur, à environ quatre mètres, avec tir par geste de la main, score au-dessus de chaque cible et compteur global.

Le pourcentage affiché mesure la proximité pouce/index ; ce n'est pas une probabilité de reconnaissance du geste. Les coordonnées ne restent pas affichées après la perte du suivi. Sans casque ou sans mains suivies, l'écran annonce l'absence de suivi : aucune main fictive n'est affichée en cours de jeu.

## Tir aux doigts et borne Snake

Pour tirer, tourner sur soi afin de regarder les cibles derrière la position de départ. Tendre l'index et le majeur, replier l'annulaire et l'auriculaire, puis relever le pouce en l'éloignant de l'index. Maintenir brièvement cette pose pour armer, puis rabattre le pouce vers la main : un seul tir part. Relever de nouveau le pouce pour armer le tir suivant. Les deux mains sont prises en charge. La direction du tir suit les deux doigts tendus.

Le centre rapporte 10 points, l'anneau intermédiaire 5 et le bord 2. Les points gagnés apparaissent au-dessus de la cible touchée. Une perte de suivi ou une pose incorrecte désarme la main sans tirer. Le bouton Replacer de la table remet également les scores des cibles à zéro.

La borne Snake se trouve à droite : la rejoindre dans l'espace physique, saisir son joystick, puis appuyer sur Start. L'écran des mains et la table restent dans la même scène.

Sans casque dans l'éditeur, Tab fait défiler les vues Table, Snake et Cibles. Pour Snake : Espace puis flèches/WASD. Pour vérifier les cibles : F tire depuis le centre de la vue. Ce raccourci de tir est réservé à l'éditeur et n'est pas inclus dans le jeu Quest.

Les données de mains sont affichées localement. Aucun enregistrement ni envoi des positions n'est implémenté.

## Utilisation sur Quest 3

1. Activer le suivi des mains dans les réglages du casque et poser les contrôleurs pour passer aux mains.
2. Présenter les deux mains devant les caméras, dans une pièce bien éclairée.
3. Approcher un objet puis pincer pouce/index pour le saisir. Déplacer la main et relâcher le pincement pour déposer l'objet.
4. Ranger les six objets dans les zones correspondantes. Le compteur ne compte un objet que lorsqu'il est relâché et stabilisé.

Le Quest doit être configuré en mode développeur et autoriser le débogage USB pour un déploiement ADB. Ces réglages et confirmations doivent être effectués par son propriétaire sur le casque. Un essai sur casque réel reste nécessaire pour valider le confort et la qualité de suivi.

## Configuration du projet

Meta XR All-in-One SDK 207.0.1, XR Hands 1.9.0, XR Interaction Toolkit 3.6.1, OpenXR 1.18.0 et Unity OpenXR Meta 2.6.1 sont présents. Les fonctionnalités Hand Tracking et Meta Hand Tracking Aim sont activées. Le réglage Meta autorise les contrôleurs et les mains.

Android : IL2CPP, ARM64, API minimale 34. Le nom d'application Android est `com.baptisteauvert.handtrackingminijeu`. La scène de ce mini-jeu est placée en première position des scènes de lancement de cette branche.

Android Build Support, SDK, NDK et OpenJDK ont été installés avec Unity Hub pour Unity 6000.6.8f1. Java et ADB ont été vérifiés. Pour jouer via Quest Link dans l'éditeur, un environnement Meta Quest Link/OpenXR fonctionnel reste nécessaire sur le PC.

## Vérifications et reproduction

Menus Unity : `Tools > Hand Tracking MiniJeu`.

- Create Quest 3 Scene : recréer la scène. Les scènes ouvertes non enregistrées sont copiées dans un dossier Backups local avant son ouverture.
- Check Scene : vérifier le câblage des six objets, les colliders, l'écran et les réglages Meta.
- Check Live Interactions : en mode Jeu, vérifier un échantillon synthétique d'affichage, la perte du suivi, la saisie XR simulée, le suivi de cible, le relâchement, le score, la remise à zéro et le retour d'un objet tombé.
- Render Preview : générer un rendu de caméra Unity, sans capture du bureau.
- Build Quest 3 APK : passer à la cible Android et générer `Builds/Quest3/HandTrackingMiniJeu.apk` avec cette seule scène. Le fichier reste local et est exclu de Git.
- Add Arcade and Finger Targets : ajouter les deux zones à une scène HandTrackingMiniJeu existante. La création complète les inclut également.
- Check Finger Gun Rules et Check Target Hits : vérifier l'armement, le déclenchement, la perte de suivi, les rayons, les zones de points et l'affichage au-dessus des cibles.

Les rapports de contrôle et rendus locaux sont placés sous `Tools/Integrations` et exclus de Git. Les vérifications simulées ne remplacent pas un essai avec le Quest 3.

Contrôle du 10 novembre 2026 : 39 vérifications réussies sur la scène complète (câblage, saisie XR simulée, déplacement et relâchement, suivi affiché et perte de suivi, tri, remise à zéro, armement du pouce, absence de tirs répétés, Snake, rayons et points sur les cibles).

APK Android généré avec succès le 10 novembre 2026, sans erreur : `Builds/Quest3/HandTrackingMiniJeu.apk` (environ 76 Mio). Le paquet ARM64, les déclarations de suivi des mains et la signature APK v2 ont été vérifiés. Aucun casque n'était connecté pour un essai réel.
