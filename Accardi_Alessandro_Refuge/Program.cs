// Très important pour voir ta classe ConnexionDB (comme avec TypeScript c'est l'import)
// ASTUCE : En tapant le nom d'une classe (ex: ConnexionDB) si elle est soulignée en rouge,
// tu peux faire Ctrl + . (la touche contrôle et le point).
// Visual Studio te proposera automatiquement d'ajouter le using correspondant en haut du fichier. C'est l'équivalent de l'auto-import dans VS Code !
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge.CoucheMetier;
using System.Collections.Generic;
using Npgsql;


namespace Accardi_Alessandro_Refuge
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Test du Système de Refuge ===");

            // 1. Instanciation du DAO
            AnimalDAO animalDao = new AnimalDAO();

            try
            {
                // 2. Test de récupération de tous les animaux
                // Cette méthode va ouvrir la connexion, faire le SELECT *,
                // et pour chaque ligne, appeler ta méthode Map().
                Console.WriteLine("\nRécupération des animaux en base...");
                List<Animal> mesAnimaux = await animalDao.SelectAllAsync();

                if (mesAnimaux.Count == 0)
                {
                    Console.WriteLine("Aucun animal trouvé dans la table.");
                }
                else
                {
                    foreach (var a in mesAnimaux)
                    {
                        Console.WriteLine("--------------------------------------");
                        Console.WriteLine($"ID    : {a.Identifiant}");
                        Console.WriteLine($"Nom   : {a.Nom} ({a.Type})");
                        Console.WriteLine($"Sexe  : {a.Sexe}");
                        Console.WriteLine($"Sté.  : {a.Sterilise}"); // Affiche "oui" ou "non" grâce à ton getter
                        Console.WriteLine($"Né le : {a.DateDeNaissance.ToShortDateString()}");

                        if (a.DateDeDeces != DateTime.MinValue)
                            Console.WriteLine($"Décès : {a.DateDeDeces.ToShortDateString()}");
                    }
                }

                // 3. Test d'insertion (Optionnel)
                /*
                Animal nouvelAnimal = Animal.Create(
                    "Rex", "Chien", "M", "non", 
                    "Tache sur l'oeil", "Chien très joueur", 
                    new DateTime(2022, 05, 10), DateTime.MinValue, DateTime.MinValue
                );

                Console.WriteLine("\nInsertion de Rex...");
                await animalDao.InsertAsync(nouvelAnimal);
                Console.WriteLine($"Rex a été inséré avec l'ID : {nouvelAnimal.Identifiant}");
                */

            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nErreur rencontrée : {ex.Message}");
                // Si ton Animal.Create lève une ArgumentException (ex: nom avec chiffre),
                // elle sera rattrapée ici !
            }

            Console.WriteLine("\nAppuyez sur une touche pour quitter...");
            Console.ReadKey();
        }
    }
}