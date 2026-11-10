# Snake Arcade VR — Meta Quest

Scène : `Assets/SnakeArcade/Scenes/SnakeArcadeVR.unity`.

La scène est autonome et placée en première position des scènes de build. La scène VR d'origine reste disponible.

## Jouer

1. Ouvrir `SnakeArcadeVR` et lancer Play avec le Quest connecté à un environnement OpenXR opérationnel (par exemple Quest Link).
2. Se placer devant la borne. L'expérience utilise la hauteur réelle du casque et reste fixe pour éviter les déplacements involontaires.
3. Saisir la boule rouge du joystick avec le grip d'un contrôleur, ou avec le pincement d'une main suivie.
4. Appuyer sur le bouton cyan Start avec le doigt, ou le sélectionner avec une interaction XR.
5. Incliner le joystick : vers la borne = haut, vers soi = bas, gauche/droite = gauche/droite à l'écran.

Manger les carrés corail fait grandir le serpent et augmente le score. La vitesse augmente progressivement. Toucher un mur ou le serpent termine la partie. Le bouton Start permet de rejouer. Le record est conservé sur l'appareil. Start est ignoré pendant une partie pour éviter les redémarrages accidentels.

## Vérifier sans casque

Dans la même scène, appuyer sur Play. Espace ou Entrée active Start ; les flèches ou WASD dirigent le serpent. Le point de vue se positionne automatiquement devant la borne lorsque Unity ne détecte aucun affichage XR actif.

## Quest autonome

Les réglages OpenXR Android du projet possèdent déjà les profils Quest Touch et le suivi des mains. Pour installer sur un Quest autonome, effectuer ensuite un build Android et déployer l'APK sur le casque. Cette étape matérielle ne fait pas partie des vérifications effectuées sur le poste : aucun APK n'a été installé et aucune session en casque n'a été validée.

## Composants

- `SnakeGameModel` : grille 20 × 20, règles, score, collisions, nourriture, file d'entrée à un seul virage par pas.
- `SnakeArcadeMachine` : écran dynamique, partie, sons, record et clavier.
- `SnakeArcadeJoystick` : sélection XR, inclinaison à quatre directions et retour au centre.
- `SnakeArcadeStartButton` : pression du doigt, sélection XR, animation et protection contre les pressions répétées.
- `Editor/SnakeArcadeSceneBuilder` : génération de la borne, du décor et du rig XR existant.
- `Editor/SnakeArcadeChecks` : vérifications des règles et des interactions en mode Jeu.

Menus Unity : `Tools > Snake Arcade`.
Les rapports de contrôle sont enregistrés dans `Tools/Integrations/snake-*-check.json`.
Vérification du 10 novembre 2026 : 18 contrôles réussis (10 règles, 5 interactions XR simulées, 3 contrôles du bouton physique). La scène finale a été chargée et testée en mode Jeu ; la dernière lecture de la console ne retourne aucune erreur. Le runtime OpenXR du poste étant indisponible pendant ces essais, aucune validation avec un casque réel n'est revendiquée.
Le rendu de la caméra de prévisualisation est `Tools/Integrations/SnakeArcade-Preview.png` ; ce rendu ne capture pas le bureau.

La borne est construite avec des géométries et des matériaux Unity, sans téléchargement de modèles ni dépendance à Blender en exécution.
