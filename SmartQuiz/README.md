# 🧠 SmartQuiz

SmartQuiz est une plateforme web intelligente dédiée à l’apprentissage,
à la gestion de quiz et à l’assistance pédagogique via l’intelligence artificielle (IA).

Le projet intègre un chatbot IA basé sur Ollama, adapté au rôle de l’utilisateur
(Étudiant, Professeur, Administrateur).
## ✨ Fonctionnalités

- 👨‍🎓 Gestion des étudiants, professeurs et administrateurs
- 📝 Création et gestion de quiz
- 🤖 Génération automatique de quiz avec IA (Ollama)
- 💬 Chatbot IA avec historique des conversations
- 💬 Messagerie entre utilisateurs
- 🔐 Authentification et autorisation par rôles
- 📊 Suivi des résultats et performances
## 🏗️ Architecture

Le projet suit une architecture ASP.NET MVC en couches :

UI (Razor / JS)
↓
Controllers
↓
Services métier
↓
Service IA (Ollama)
↓
Moteur IA local (CLI)

La persistance des données est assurée par MongoDB.
## 🤖 Intelligence Artificielle

SmartQuiz utilise Ollama comme moteur d’intelligence artificielle locale.

Deux usages principaux :
- Génération automatique de quiz
- Chatbot IA avec historique et adaptation au rôle utilisateur

L’IA est appelée via une interface CLI (`ollama run llama3`).
## 🛠️ Technologies

- ASP.NET Core MVC
- MongoDB
- Ollama (LLM local)
- JavaScript / Bootstrap
- SignalR (messagerie temps réel)
## 🚀 Installation

### Prérequis
- .NET 7 ou 8
- MongoDB
- Ollama installé
- Modèle IA téléchargé (`llama3`)

### Étapes
```bash
git clone https://github.com/adam-boubchir/SmartQuiz.git
cd SmartQuiz
dotnet restore
dotnet run
## 📂 Structure du projet

- Controllers/ → gestion des routes et requêtes
- Services/ → logique métier et IA
- Models/ → modèles de données
- Views/ → interfaces utilisateur
## 👤 Auteur

- Nom : Adam Boubchir - Safaa Bouhouch - Assmaa Chafi
- GitHub : https://github.com/adam-boubchir
- Projet académique / personnel
## 📄 Licence

Projet à but pédagogique.

