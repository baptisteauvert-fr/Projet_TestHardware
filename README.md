# Projet TestHardware — Snake Arcade VR

Projet Unity **6000.6.8f1**, destiné à Meta Quest avec OpenXR.

Meta XR All-in-One SDK **207.0.1** est installé via Unity Package Manager. Le manifeste et le verrouillage des packages permettent de restaurer le SDK et ses dépendances après un clonage.

Les groupes Meta XR sont activés pour OpenXR (PC et Android). À la première importation dans Unity 6.6, laisser l'API Updater mettre à jour les anciennes références d'API du module audio si Unity le demande. Android Build Support, SDK, NDK et OpenJDK sont installés sur le poste pour Unity 6000.6.8f1. Ces outils doivent aussi être installés via Unity Hub sur un autre poste pour générer un APK Quest.

## Branche Hand-Tracking-MiniJeu

Cette branche ajoute la scène `Assets/HandTrackingMiniJeu/Scenes/HandTrackingMiniJeu.unity` : écran de positions des mains Quest 3, table de six objets à saisir, mini-jeu de tri par couleur, borne Snake à droite et trois cibles au loin derrière le joueur avec tir par index/majeur et déclenchement au pouce. Elle devient la première scène de lancement sur cette branche. Les scènes Snake et du modèle VR restent disponibles.

Voir [le guide Hand Tracking MiniJeu](Assets/HandTrackingMiniJeu/README.md).

Dépôt GitHub privé : [baptisteauvert-fr/Projet_TestHardware](https://github.com/baptisteauvert-fr/Projet_TestHardware).

## Ouvrir le projet

1. Cloner le dépôt et ajouter son dossier dans Unity Hub.
2. Ouvrir avec Unity 6000.6.8f1 et attendre la restauration des packages et l'importation des ressources.
3. Ouvrir `Assets/SnakeArcade/Scenes/SnakeArcadeVR.unity`.

La dépendance MCP Unity est référencée sur GitHub avec une révision fixe dans `Packages/manifest.json` : aucune copie locale des serveurs MCP n'est nécessaire pour importer le projet.

## Jouer

- VR : saisir le joystick avec le grip du contrôleur ou le pincement des mains, puis toucher ou sélectionner Start.
- Sans casque : Play, puis Espace/Entrée pour commencer et les flèches/WASD pour diriger.
- Pour jouer sur Quest via le PC, utiliser un environnement OpenXR/Quest Link opérationnel. L'installation d'un APK Android sur le casque se fait séparément.

Voir [le guide Snake Arcade](Assets/SnakeArcade/README.md).

## Fichiers suivis par Git

Le dépôt conserve les ressources et leurs fichiers `.meta`, les scènes, les scripts, les réglages Unity, le manifeste et le verrouillage des packages, ainsi que les petits scripts d'aide.

Sont exclus : caches `Library`/`Temp`, journaux, réglages personnels, builds, fichiers IDE générés, sauvegardes automatiques de scènes, secrets et installations locales de Blender/MCP/Python. Ces exclusions ne suppriment aucun fichier du disque.

La sérialisation Unity est en mode texte et les métadonnées sont visibles. `.gitattributes` normalise les fins de ligne et conserve les ressources binaires sans conversion.

Les outils de contrôle externes et Blender sont facultatifs pour exécuter le jeu. Ils doivent être réinstallés et reconnectés sur une autre machine si nécessaire ; leurs exécutables et la configuration personnelle Codex ne sont pas versionnés. Voir [la documentation locale des intégrations](Tools/Integrations/LISEZ-MOI.md).

## Utilisation de Git

La branche principale est `main`. Les modifications futures peuvent être sauvegardées avec :

```sh
git status
git add Assets Packages ProjectSettings README.md .gitignore .gitattributes
git commit -m "Décrire la modification"
git push
```

Ne pas ignorer tous les fichiers `.meta` ni `Packages/packages-lock.json` : ils font partie du projet. Le dépôt distant `origin` pointe vers le dépôt GitHub privé indiqué ci-dessus.
