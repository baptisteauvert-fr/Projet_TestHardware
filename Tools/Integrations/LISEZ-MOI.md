# Unity et Blender avec MCP

Installation locale pour `D:/Projets/Unity/Projet_TestHardware`.

## Ouvrir les logiciels

- Unity : ouvrir ce projet avec Unity Hub (éditeur 6000.6.8f1).
- Blender : double-cliquer sur `Ouvrir Blender.lnk` à la racine du projet.
- Attendre la fin des importations et compilations Unity avant de demander une modification.
- Après l'installation des connexions, redémarrer Codex pour charger les nouveaux outils MCP.

La connexion Unity démarre automatiquement avec ce projet. Pour la relancer manuellement :
`Tools > Local MCP > Reconnect Unity` dans Unity.
L'extension Blender démarre sa connexion locale automatiquement à l'ouverture de Blender.
Dans Blender, le panneau `MCP for Blender` se trouve dans la barre latérale de la vue 3D (touche N).

## Logiciels installés

| Élément | Version | Emplacement |
| --- | --- | --- |
| Blender officiel, portable | 4.5.9 LTS | `Tools/Integrations/Blender/blender-4.5.9-windows-x64` |
| MCP for Unity (CoplayDev) | 10.3.0 | `Tools/Integrations/unity-mcp` |
| MCP for Blender (ahujasid) | 2.2.7 | `Tools/Integrations/blender-mcp` |
| uv | 0.13.7 | `Tools/Integrations/uv` |

Les environnements Python sont installés dans les dossiers `.venv` des serveurs.
L'extension Blender est également installée dans le dossier utilisateur Blender 4.5.
La configuration des connexions se trouve dans `C:/Users/Bapti/.codex/config.toml`.
Sa sauvegarde avant installation est `config.toml.before-unity-blender.bak` dans le même dossier.
Les connexions communiquent sur l'ordinateur local. Les serveurs MCP sont configurés avec `DISABLE_TELEMETRY=true`.
Ces extensions communautaires ne demandent aucune clé API pour les opérations locales de scène et de code.
Les fonctions facultatives de génération et de téléchargement de ressources peuvent demander des services externes ; elles n'ont pas été configurées.

## Vérifications

Les scripts `check-mcp.py` et `verify-unity.py` utilisent le véritable protocole MCP.
Les comptes rendus sont `blender-check.json`, `unity-check.json` et `unity-verification.json`.
Vérification du 10 novembre 2026 : lecture et modification réussies dans les deux logiciels.
Unity : scène `SampleScene`, objet temporaire créé, déplacé vers `(1, 2, 3)`, puis retiré ; recherche finale vide et zéro erreur retournée par la console lors du contrôle.
Blender : scène lue, objet temporaire créé, déplacé puis retiré avec succès.
Le premier contrôle lit les outils et la scène Blender, puis crée, modifie et retire un objet temporaire.
Le contrôle Unity attend l'éditeur, lit le projet et la scène, puis crée, déplace et retire un objet temporaire.

Pour relancer les contrôles depuis PowerShell, à la racine du projet :

```powershell
& ./Tools/Integrations/unity-mcp/Server/.venv/Scripts/python.exe ./Tools/Integrations/check-mcp.py
& ./Tools/Integrations/unity-mcp/Server/.venv/Scripts/python.exe ./Tools/Integrations/verify-unity.py
```

## Sources

- [Blender officiel](https://download.blender.org/release/Blender4.5/)
- [MCP for Unity](https://github.com/CoplayDev/unity-mcp), révision `81542a7e86292b5c4c21b1782c5af23b567f45ce`
- [MCP for Blender](https://github.com/ahujasid/mcp-for-blender), révision `2e4b0bc294694b1570d76ed01be657c22e6d02a0`
- [Configuration MCP dans Codex](https://developers.openai.com/codex/mcp/)

La configuration personnelle Codex utilise les chemins locaux des serveurs sous `Tools/Integrations` : conserver les installations à cet emplacement sur ce poste. Git conserve les scripts d'aide et ce guide, mais exclut les logiciels, serveurs clonés, environnements Python, captures et rapports temporaires.

Le package chargé par Unity est désormais référencé par son URL GitHub avec une révision fixe dans `Packages/manifest.json`. Unity le restaure automatiquement sur une nouvelle copie du projet ; il ne dépend plus du clone local `Tools/Integrations/unity-mcp/MCPForUnity`. Les serveurs Codex et Blender restent des installations locales facultatives, à reconfigurer sur une autre machine.
