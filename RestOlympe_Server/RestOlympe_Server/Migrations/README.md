# Pour utiliser les migrations

Une fois les modifications apportées aux entités et les liens déclarés dans ApplicationDbContext, il est possible de générer des fichiers de migration qui mettront la base à jour.

## Préparation : 

Il faut commencer par installer les outils dotnet ef sur l'invite de commande de votre choix (Powershell recommandé)

`dotnet tool install --global dotnet-ef`

## Création de la migration

Ensuite, en ayant le terminal ouvert à la racine du projet (\RestOlympe_Server\RestOlympe_Server) Il faut exécuter la commande suivante

`dotnet ef migrations add NomDeVotreMigration`

Ainsi, la migration sera créée et appliquée au lancement de l'application via docker compose.