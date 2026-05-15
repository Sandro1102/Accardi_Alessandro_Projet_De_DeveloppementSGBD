using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge.CoucheMetier;

namespace Accardi_Alessandro_Refuge.CouchePresentation
{
    internal record AnimalUI
    {
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

                    string nom = AccesConsole.LireChaine("Nom");
                    string type = AccesConsole.LireChaine("Type (chien/chat)");
                    string sexe = AccesConsole.LireChaine("Sexe (M/F)");
                    string sterilise = AccesConsole.LireChaine("Stérilisé (oui/non)");
                    string particularite = AccesConsole.LireChaineOpt("Particularité (vide si aucune)");
                    string description = AccesConsole.LireChaineOpt("Description (vide si aucune)");
                    DateTime dateN = AccesConsole.LireDate("Date de naissance (yyyy-MM-dd)");
                    DateTime dateD = AccesConsole.LireDateOpt("Date de décès (vide si aucune)");
                    DateTime dateS = AccesConsole.LireDateOpt("Date de stérilisation (vide si aucune)");

                    Animal animal = Animal.Create(
                        nom, type, sexe, sterilise,
                        particularite, description,
                        dateN, dateD, dateS
                    );

                    await dao.InsertAsync(animal);

                    Console.WriteLine($"\nAnimal '{animal.Nom}' ajouté avec succès (ID : {animal.Identifiant}).");
                    Console.ReadKey();
                    continuer = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nErreur : {ex.Message}");
                    continuer = AccesConsole.DemanderReessayer();
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

            bool connaitId = AccesConsole.Confirmation("Connaissez-vous l'identifiant de l'animal");

            if (!connaitId)
                await ListerAnimaux(dao);

            string id = AccesConsole.DemanderId("Entrez l'identifiant de l'animal");

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

            string id = AccesConsole.DemanderId();

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

                    if (AccesConsole.Confirmation($"Confirmer la suppression de '{animal.Nom}'"))
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

                    string id = AccesConsole.DemanderId();
                    Animal animal = await dao.SelectByIdAsync(id);

                    if (animal == null)
                    {
                        Console.WriteLine("Aucun animal trouvé avec cet identifiant.");
                        continuer = false;
                    }
                    else
                    {
                        AfficherAnimal(animal);

                        Console.WriteLine("\n1. Particularité");
                        Console.WriteLine("2. Description");
                        Console.WriteLine("3. Date de décès");
                        Console.WriteLine("4. Date de stérilisation");
                        Console.Write("Votre choix : ");

                        int.TryParse(Console.ReadLine(), out int choix);

                        switch (choix)
                        {
                            case 1: animal.Particularite = AccesConsole.LireChaineOpt("Nouvelle particularité"); break;
                            case 2: animal.Description = AccesConsole.LireChaineOpt("Nouvelle description"); break;
                            case 3: animal.DateDeDeces = AccesConsole.LireDateOpt("Nouvelle date de décès"); break;
                            case 4: animal.DateDeSterilisation = AccesConsole.LireDateOpt("Nouvelle date de stérilisation"); break;
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
                    continuer = AccesConsole.DemanderReessayer();
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

                    string id = AccesConsole.DemanderId();
                    Animal animal = await dao.SelectByIdAsync(id);

                    if (animal == null)
                    {
                        Console.WriteLine("Aucun animal trouvé avec cet identifiant.");
                        continuer = false;
                    }
                    else
                    {
                        AfficherAnimal(animal);

                        Console.WriteLine("\n1. Particularité");
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
                    continuer = AccesConsole.DemanderReessayer();
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
                    Console.WriteLine($"{"ID",-12} {"Nom",-20} {"Type",-8} {"Sexe",-6} {"Stérilisé",-10} {"Naissance",-12} {"Décès",-12} {"Stérilisation",-15} {"Particularité",-20} {"Description",-30}");
                    Console.WriteLine(new string('-', 150));

                    foreach (Animal a in animaux)
                    {
                        Console.WriteLine(
                            $"{a.Identifiant,-12} " +
                            $"{a.Nom,-20} " +
                            $"{a.Type,-8} " +
                            $"{a.Sexe,-6} " +
                            $"{a.Sterilise,-10} " +
                            $"{a.DateDeNaissance:yyyy-MM-dd,-12} " +
                            $"{(a.DateDeDeces == DateTime.MinValue ? "-" : a.DateDeDeces.ToString("yyyy-MM-dd")),-12} " +
                            $"{(a.DateDeSterilisation == DateTime.MinValue ? "-" : a.DateDeSterilisation.ToString("yyyy-MM-dd")),-15} " +
                            $"{(a.Particularite ?? "-"),-20} " +
                            $"{(a.Description ?? "-"),-30}"
                        );
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
        //   AFFICHAGE D'UN ANIMAL
        // ============================

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
    }
}
