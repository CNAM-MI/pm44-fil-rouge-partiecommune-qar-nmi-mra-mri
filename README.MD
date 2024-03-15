# RestOlympe
## Notre problématique 
Comment faciliter la décision du lieu de restauration d'un groupe de personnes ?

# Table des matières

1. [Informations liées à la théorie du choix social](#1-informations-liées-à-la-théorie-du-choix-social)  
    1.1. [L’expression des préférences](#11-lexpression-des-préférences)  
    1.2. [La fonction d'agrégation et ses propriétés](#12-la-fonction-daggrégation-et-ses-propriétés)
2. [Spécificités du développement mobile](#2-spécificités-du-développement-mobile)  
    2.1. [Quels sont les fonctionnalités mobiles employées](#21-quels-sont-les-fonctionnalités-mobiles-employées)  
    2.2. [Comment gère-t-on la communication](#22-comment-gère-t-on-la-communication)  
    2.3. [Comment gère-t-on la persistance des données](#23-comment-gère-t-on-la-persistence-des-données)  
        2.3.1. [Quelles données devons nous enregistrer au niveau du serveur](#231-quelles-données-devons-nous-enregistrer-au-niveau-du-serveur)  
        2.3.2. [Quelles données devons nous enregistrer au niveau du client](#232-quelles-données-devons-nous-enregistrer-au-niveau-du-client)  
    2.4. [Où effectue-t-on les calculs](#24-où-effectue-t-on-les-calculs)  
        2.4.1. [Côté client](#241-côté-client)  
        2.4.2. [Côté serveur](#242-côté-serveur)  
3. [Technologies utilisées](#3-technologies-utilisées)  
    3.1. [Pour l’application mobile](#31-pour-lapplication-mobile)  
    3.2. [Pour le serveur](#32-pour-le-serveur)  

---

## 1. Informations liées à la théorie du choix social
### 1.1 L’expression des préférences
L’utilisateur possède 100 points et les répartis entre les différentes propositions. Les points peuvent être placés dans le positif ou le négatif.

### 1.2 La fonction d'agrégation et ses propriétés
Le résultat est défini différemment en fonction des paramètre choisis par l’administrateur du salon lors de la création de ce dernier : 

- Option 1 : Le choix final sera celui qui aura cumulé le plus grand nombre de points.
- Option 2 : Le choix sera effectué de manière aléatoire, la probabilité étant déterminée par les points obtenus.

### TODO : Justifier les choix avec le cours de choix social


## 2. Spécificités du développement mobile
### 2.1 Quels sont les fonctionnalités mobiles employées
Plusieurs fonctionnalités inhérente au téléphones portables seront employés dans l’application :
- La géolocalisation permettra de sélectionner les restaurants alentours  
- Le lien salon peut être partagé grâce à la fonctionnalité partage du téléphone d’un utilisateur  
- Un utilisateur peut également générer un QR code qui permet de rejoindre le salon  
- Des notifications push indiquent à l’utilisateur quand les autres utilisateurs ont choisi/quand le choix a pris fin.

### 2.2 Comment gère-t-on la communication
Toutes les technos communiquent avec le serveur et doivent donc employer les mêmes canaux. Le serveur propose pour cette raison une API REST, puisque cette solution est disponible pour l'immense majorité des langages. REST sera utilisé avec des websocket pour permettre une communication bidirectionnelle.
### 2.3 Comment gère-t-on la persistance des données
Le serveur en question sert une application ASP.NET conteneurisée qui embarque une API REST. Ce serveur avec une base de données PostgreSQL.
#### 2.3.1 Quelles données devons nous enregistrer au niveau du serveur
Pour les salons : 
- Titre
- Limite géographique (point central + limite en kilométrage)  

Les participants à un salon : 
- Nom/Pseudo
- Avatar  

La répartition des points de chaque personne pour un salon donné.

#### 2.3.2 Quelles données devons nous enregistrer au niveau du client
Le client doit pouvoir définir son profil avec plusieurs caractéristiques :

Un nom d’utilisateur (pseudonyme/nom prénom)

Un avatar (sélection limitée parmis des éléments prédéfinis: cadavre exquis à la manière de skribbl.io) 

### 2.4 Où effectue-t-on les calculs
#### 2.4.1 Côté client
Les calculs côté client sont limités au comptage des points de vote (limités à 100 on le rappelle) po
#### 2.4.2 Côté serveur
Les calculs et l’algorithme d'agrégation sont effectués côté serveur une fois la totalité des votes pris en compte.


## 3. Technologies utilisées
### 3.1 Pour l’application mobile
- Kotlin multiplateforme: par Mathis RICHARD
https://github.com/CNAM-MI/pm44-fil-rouge-appindividuelle-Mathis-Richard
- Kotlin natif pour Android: par Maëlis RAKOWSKI
https://github.com/CNAM-MI/pm44-fil-rouge-appindividuelle-Maelis-Rakowski
- React-Native: par Nellou MICHEL
- Flutter: par Quentin ARZALIER
https://github.com/CNAM-MI/pm44-fil-rouge-appindividuelle-quentin-arzalier
### 3.2 Pour le serveur
Un serveur ASP.NET C# déployé sur Docker offrant des points d’API REST avec Swagger OpenAPI. Il proposera également un serveur websocket qui enverra les données 
