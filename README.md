# Projet TestHardware — Snake Arcade VR

Projet Unity **6000.6.8f1**, destiné à Meta Quest avec OpenXR.

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
