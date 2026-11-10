# Hand Tracking MiniJeu — Meta Quest 3

Branche : `Hand-Tracking-MiniJeu`.
Scène : `Assets/HandTrackingMiniJeu/Scenes/HandTrackingMiniJeu.unity`.

## Ce que contient la scène

- Un rig XR avec suivi des mains OpenXR/XR Hands et prise en charge des contrôleurs.
- Un écran affichant, pour chaque main, l'état de suivi, la position XYZ du poignet dans le monde (en mètres), la proximité du pouce et de l'index et un squelette automatiquement centré dans le plan de la paume.
- Une table avec six objets physiques à saisir par pincement, déplacer et relâcher.
- Trois zones de tri : déposer les objets dans la zone de même couleur pour atteindre 6/6.
- Un bouton physique Replacer ; la touche R remplit la même fonction dans l'éditeur.
- Le retour automatique sur la table des objets tombés au sol. Un objet encore saisi n'est jamais déplacé par la remise à zéro.
- La borne d'arcade Snake, à droite de la table, avec son joystick et son bouton Start.
- Trois cibles derrière le joueur, à environ quatre mètres, avec tir par geste de la main, score au-dessus de chaque cible et compteur global.

Le pourcentage affiché mesure la proximité pouce/index ; ce n'est pas une probabilité de reconnaissance du geste. Les coordonnées ne restent pas affichées après la perte du suivi. Sans casque ou sans mains suivies, l'écran annonce l'absence de suivi : aucune main fictive n'est affichée en cours de jeu.

## Tir aux doigts et borne Snake

Pour tirer, tourner sur soi afin de regarder les cibles derrière la position de départ. Tendre l'index et le majeur : un canon futuriste orange apparaît sur la main. Les deux autres doigts ne bloquent pas la reconnaissance. Relever le pouce en l'éloignant des doigts tendus : le canon devient vert lorsqu'il est armé. Rabattre ensuite le pouce vers la main : un seul tir part, avec flash et recul du canon. Relever de nouveau le pouce pour armer le tir suivant. Les deux mains sont prises en charge. Le rayon de visée et le viseur sur la cible indiquent exactement la direction du prochain tir.

Le seuil de tir s'adapte à la position du pouce levé ; il n'est plus nécessaire de fermer complètement le pouce. La mesure ignore la distance le long de l'index. Les mouvements de visée sont lissés, et une perte de suivi masque immédiatement le canon et désarme la main.

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

### Test habituel : Quest Link et Play dans Unity

Pour les itérations du mini-jeu, utiliser Quest Link et le bouton Play, sans générer d'APK. Sur le PC, Meta Horizon Link doit être le moteur OpenXR actif et ses fonctions développeur doivent autoriser le suivi des mains. Dans le casque, lancer Quest Link vers ce PC avant de cliquer sur Play. Le branchement USB seul n'active pas la session PC VR. Les réglages OpenXR et XR Hands sont également activés pour la plateforme PC de ce projet.

Attention à la différence avec Écran virtuel / Remote Desktop : voir le bureau de Windows dans le casque ne suffit pas à établir une session PC VR. Le client Quest Link PC VR doit être ouvert. Le projet est maintenant placé sur la plateforme PC pour ce mode de test.

Si des fonctions développeur de Link sont activées pour la première fois, Meta recommande de redémarrer l'éditeur. Référence : [Link pour le développement](https://developers.meta.com/vr/documentation/unity/unity-link/).

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
- Check Cannon Feedback : vérifier le déclenchement avec un abaissement partiel du pouce, le réarmement, la mesure du pouce et le cadrage des mains après déplacement et rotation.

Les rapports de contrôle et rendus locaux sont placés sous `Tools/Integrations` et exclus de Git. Les vérifications simulées ne remplacent pas un essai avec le Quest 3.

Contrôle du 10 novembre 2026 : 39 vérifications réussies sur la scène complète (câblage, saisie XR simulée, déplacement et relâchement, suivi affiché et perte de suivi, tri, remise à zéro, armement du pouce, absence de tirs répétés, Snake, rayons et points sur les cibles).

APK Android généré avec succès le 10 novembre 2026, sans erreur : `Builds/Quest3/HandTrackingMiniJeu.apk` (environ 76 Mio). Le paquet ARM64, les déclarations de suivi des mains et la signature APK v2 ont été vérifiés. Aucun casque n'était connecté pour un essai réel.

Essai ultérieur sur Quest 3 : installation et lancement réussis, scène visible et saisie d'objets confirmées par l'utilisateur. Après ce premier essai, le tir a été assoupli et doté d'un canon avec retour visuel ; l'écran des mains utilise désormais un cadrage automatique. Les nouveaux contrôles couvrent également le tir avec un pouce partiellement abaissé et le maintien des articulations dans le cadre après rotation et déplacement.

Le test de la version avec canon se poursuit désormais avec Quest Link et Play dans l'éditeur, à la demande de l'utilisateur. L'APK installé précédemment ne contient pas encore ces corrections. Une recompilation propre des scripts a résolu l'erreur de liaison gRPC du traitement ILPP ; l'entrée en mode Play a ensuite été vérifiée.

Diagnostic PC VR du 10 novembre 2026 : après lancement du client Quest Link et démarrage des sous-systèmes XR, Unity confirme le moteur Oculus, l'affichage XR actif et le sous-système de suivi des mains en cours d'exécution. L'application Écran virtuel seule n'avait pas exposé le casque à OpenXR. Le rendu réel reste à valider : la session Link n'avait ensuite transmis aucune pose ni passe de rendu et affichait une image noire. Les fonctions développeur ont été activées par l'utilisateur ; Sources inconnues et le moteur OpenXR Meta étaient déjà activés. La batterie du casque était descendue à 3 % pendant ce diagnostic. Les nouvelles interactions avec canon ne sont donc pas encore confirmées sur casque.
