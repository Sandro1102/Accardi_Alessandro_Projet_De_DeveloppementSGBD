using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge.CoucheMetier;

namespace Accardi_Alessandro_Refuge.CouchePresentation
{
    internal record AnimalUI
    {
        // -------------------------------------------------------
        //  Pattern Singleton (instance unique, constructeur privé)
        // -------------------------------------------------------
        public static AnimalUI Instance { get; } = new();
        private AnimalUI() { }

        // ============================
        //        MENU ANIMAL
        // ============================

        public async Task MenuAnimaux()
        {
            AnimalDAO dao = new AnimalDAO();
            int choix;

            do
            {
                Console.Clear();
                Console.WriteLine("===== MENU ANIMAUX =====");
                Console.WriteLine("1. Ajouter un animal");
                Console.WriteLine("2. Consulter un animal");
                Console.WriteLine("3. Supprimer un animal");
                Console.WriteLine("4. Ajouter une information sur un animal");
                Console.WriteLine("5. Supprimer une information sur un animal");
                Console.WriteLine("6. Lister tous les animaux présents au refuge");
                Console.WriteLine("0. Retour");
                Console.WriteLine("========================");
                Console.Write("Votre choix : ");

                int.TryParse(Console.ReadLine(), out choix);

                switch (choix)
                {
                    case 1: await AjouterAnimal(dao); break;
                    case 2: await ConsulterAnimal(dao); break;
                    case 3: await SupprimerAnimal(dao); break;
                    case 4: await AjouterInformationAnimal(dao); break;
                    case 5: await SupprimerInformationAnimal(dao); break;
                    case 6: await ListerAnimaux(dao); break;
                    case 0: break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        Console.ReadKey();
                        break;
                }

            } while (choix != 0);
        }

        // ============================
        //       AJOUTER UN ANIMAL
        // ============================

        private async Task AjouterAnimal(AnimalDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("=== AJOUTER UN ANIMAL ===");

                    string nom              = LireChaine("Nom");
                    string type             = LireChaine("Type (chien/chat)");
                    string sexe             = LireChaine("Sexe (M/F)");
                    string sterilise        = LireChaine("Stérilisé (oui/non)");
                    string particularite    = LireChaineOpt("Particularité (vide si aucune)");
                    string description      = LireChaineOpt("Description (vide si aucune)");
                    DateTime dateNaissance  = LireDate("Date de naissance (yyyy-MM-dd)");
                    DateTime dateDeces      = LireDateOpt("Date de décès (vide si aucune)");
                    DateTime dateSteril     = LireDateOpt("Date de stérilisation (vide si aucune)");

                    Animal animal = Animal.Create(nom, type, sexe, sterilise, particularite, description,dateNaissance, dateDeces, dateSteril);

                    await dao.InsertAsync(animal);

                    Console.WriteLine($"\nAnimal '{animal.Nom}' ajouté avec succès.");
                    Console.ReadKey();
                    continuer = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nErreur : {ex.Message}");
                    continuer = DemanderReessayer();
                }
            }
        }

        // ============================
        //      CONSULTER UN ANIMAL
        // ============================

        private async Task ConsulterAnimal(AnimalDAO dao)
        {
            Console.Clear();
            Console.WriteLine("=== CONSULTER UN ANIMAL ===");
            Console.Write("Connaissez-vous l'identifiant de l'animal ? (oui/non) : ");
            string reponse = Console.ReadLine()?.Trim().ToLower();

            if (reponse == "non")
                await ListerAnimaux(dao);

            Console.Write("\nEntrez l'identifiant de l'animal à consulter : ");
            string id = Console.ReadLine()?.Trim();

            try
            {
                Animal animal = await dao.SelectByIdAsync(id);

                if (animal == null)
                    Console.WriteLine("Aucun animal trouvé avec cet identifiant.");
                else
                    AfficherAnimal(animal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }

            Console.ReadKey();
        }

        // ============================
        //      SUPPRIMER UN ANIMAL
        // ============================

        private async Task SupprimerAnimal(AnimalDAO dao)
        {
            Console.Clear();
            Console.WriteLine("=== SUPPRIMER UN ANIMAL ===");
            Console.Write("Entrez l'identifiant de l'animal à supprimer : ");
            string id = Console.ReadLine()?.Trim();

            try
            {
                Animal animal = await dao.SelectByIdAsync(id);

                if (animal == null)
                {
                    Console.WriteLine("Aucun animal trouvé avec cet identifiant.");
                }
                else
                {
                    AfficherAnimal(animal);

                    Console.Write($"\nConfirmer la suppression de '{animal.Nom}' ? (oui/non) : ");
                    string confirmation = Console.ReadLine()?.Trim().ToLower();

                    if (confirmation == "oui")
                    {
                        await dao.DeleteAsync(animal);
                        Console.WriteLine("Animal supprimé avec succès.");
                    }
                    else
                    {
                        Console.WriteLine("Suppression annulée.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }

            Console.ReadKey();
        }

        // ============================
        //   AJOUTER UNE INFORMATION
        // ============================

        private async Task AjouterInformationAnimal(AnimalDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("=== MODIFIER LES INFORMATIONS D'UN ANIMAL ===");
                    Console.Write("Entrez l'identifiant de l'animal à modifier : ");
                    string id = Console.ReadLine()?.Trim();

                    Animal animal = await dao.SelectByIdAsync(id);

                    if (animal == null)
                    {
                        Console.WriteLine("Aucun animal trouvé avec cet identifiant.");
                        continuer = false;
                    }
                    else
                    {
                        AfficherAnimal(animal);

                        Console.WriteLine("\nQuelle information souhaitez-vous ajouter/modifier ?");
                        Console.WriteLine("1. Particularité");
                        Console.WriteLine("2. Description");
                        Console.WriteLine("3. Date de décès");
                        Console.WriteLine("4. Date de stérilisation");
                        Console.Write("Votre choix : ");

                        int.TryParse(Console.ReadLine(), out int choix);

                        switch (choix)
                        {
                            case 1: animal.Particularite = LireChaineOpt("Nouvelle particularité"); break;
                            case 2: animal.Description = LireChaineOpt("Nouvelle description"); break;
                            case 3: animal.DateDeDeces = LireDateOpt("Nouvelle date de décès (yyyy-MM-dd)"); break;
                            case 4: animal.DateDeSterilisation = LireDateOpt("Nouvelle date de stérilisation (yyyy-MM-dd)"); break;
                            default:
                                Console.WriteLine("Choix invalide.");
                                break;
                        }

                        await dao.UpdateAsync(animal);
                        Console.WriteLine("\nInformation mise à jour avec succès.");
                        continuer = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nErreur : {ex.Message}");
                    continuer = DemanderReessayer();
                }

                Console.ReadKey();
            }
        }

        // ============================
        //   SUPPRIMER UNE INFORMATION
        // ============================

        private async Task SupprimerInformationAnimal(AnimalDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("=== SUPPRIMER UNE INFORMATION D'UN ANIMAL ===");
                    Console.Write("Entrez l'identifiant de l'animal : ");
                    string id = Console.ReadLine()?.Trim();

                    Animal animal = await dao.SelectByIdAsync(id);

                    if (animal == null)
                    {
                        Console.WriteLine("Aucun animal trouvé avec cet identifiant.");
                        continuer = false;
                    }
                    else
                    {
                        AfficherAnimal(animal);

                        Console.WriteLine("\nQuelle information souhaitez-vous supprimer (vider) ?");
                        Console.WriteLine("1. Particularité");
                        Console.WriteLine("2. Description");
                        Console.Write("Votre choix : ");

                        int.TryParse(Console.ReadLine(), out int choix);

                        switch (choix)
                        {
                            case 1: animal.Particularite = null; break;
                            case 2: animal.Description = null; break;
                            default:
                                Console.WriteLine("Choix invalide.");
                                break;
                        }

                        await dao.UpdateAsync(animal);
                        Console.WriteLine("\nInformation supprimée avec succès.");
                        continuer = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nErreur : {ex.Message}");
                    continuer = DemanderReessayer();
                }

                Console.ReadKey();
            }
        }

        // ============================
        //       LISTER LES ANIMAUX
        // ============================

        private async Task ListerAnimaux(AnimalDAO dao)
        {
            Console.Clear();
            Console.WriteLine("=== ANIMAUX PRÉSENTS AU REFUGE ===\n");

            try
            {
                List<Animal> animaux = await dao.SelectAllAsync();

                if (animaux.Count == 0)
                {
                    Console.WriteLine("Aucun animal enregistré.");
                }
                else
                {
                    Console.WriteLine($"{"ID",-12} {"Nom",-20} {"Type",-8} {"Sexe",-6} {"Stérilisé",-10} {"Naissance",-12}");
                    Console.WriteLine(new string('-', 72));

                    foreach (Animal a in animaux)
                    {
                        string naissance = a.DateDeNaissance.ToString("yyyy-MM-dd");
                        Console.WriteLine($"{a.Identifiant,-12} {a.Nom,-20} {a.Type,-8} {a.Sexe,-6} {a.Sterilise,-10} {naissance,-12}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        // ============================
        //       MÉTHODES UTILITAIRES
        // ============================

        // Affiche toutes les informations d'un animal
        private void AfficherAnimal(Animal animal)
        {
            Console.WriteLine();
            Console.WriteLine($"  Identifiant        : {animal.Identifiant}");
            Console.WriteLine($"  Nom                : {animal.Nom}");
            Console.WriteLine($"  Type               : {animal.Type}");
            Console.WriteLine($"  Sexe               : {animal.Sexe}");
            Console.WriteLine($"  Stérilisé          : {animal.Sterilise}");
            Console.WriteLine($"  Particularité      : {animal.Particularite ?? "-"}");
            Console.WriteLine($"  Description        : {animal.Description ?? "-"}");
            Console.WriteLine($"  Date naissance     : {animal.DateDeNaissance:yyyy-MM-dd}");
            Console.WriteLine($"  Date décès         : {(animal.DateDeDeces == DateTime.MinValue ? "-" : animal.DateDeDeces.ToString("yyyy-MM-dd"))}");
            Console.WriteLine($"  Date stérilisation : {(animal.DateDeSterilisation == DateTime.MinValue ? "-" : animal.DateDeSterilisation.ToString("yyyy-MM-dd"))}");
        }

        // Lecture d'une chaîne obligatoire
        private string LireChaine(string libelle)
        {
            Console.Write($"{libelle} : ");
            return Console.ReadLine();
        }

        // Lecture d'une chaîne optionnelle (retourne null si vide)
        private string LireChaineOpt(string libelle)
        {
            Console.Write($"{libelle} : ");
            string valeur = Console.ReadLine();
            return string.IsNullOrWhiteSpace(valeur) ? null : valeur;
        }

        // Lecture d'une date obligatoire, boucle jusqu'à saisie valide
        private DateTime LireDate(string libelle)
        {
            DateTime date = DateTime.MinValue;
            bool valide = false;

            while (!valide)
            {
                Console.Write($"{libelle} : ");
                valide = DateTime.TryParse(Console.ReadLine(), out date);

                if (!valide)
                    Console.WriteLine("Format invalide. Réessayez (yyyy-MM-dd).");
            }

            return date;
        }

        // Lecture d'une date optionnelle (retourne DateTime.MinValue si vide)
        private DateTime LireDateOpt(string libelle)
        {
            DateTime date = DateTime.MinValue;
            bool valide = false;

            while (!valide)
            {
                Console.Write($"{libelle} : ");
                string saisie = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(saisie))
                {
                    valide = true;
                }
                else if (DateTime.TryParse(saisie, out date))
                {
                    valide = true;
                }
                else
                {
                    Console.WriteLine("Format invalide. Réessayez (yyyy-MM-dd) ou laissez vide.");
                }
            }

            return date;
        }

        // Demande si l'utilisateur veut réessayer après une erreur
        // Retourne true si oui, false sinon
        private bool DemanderReessayer()
        {
            Console.Write("Voulez-vous réessayer ? (oui/non) : ");
            string choix = Console.ReadLine()?.Trim().ToLower();
            bool reessayer = (choix == "oui");

            if (!reessayer)
            {
                Console.WriteLine("Retour au menu animaux.");
                Console.ReadKey();
            }

            return reessayer;
        }
    }
}