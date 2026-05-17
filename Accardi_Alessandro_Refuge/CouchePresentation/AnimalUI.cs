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
            AnimalDAO animalDAO = new AnimalDAO();
            EntreeDAO entreeDAO = new EntreeDAO();
            SortieDAO sortieDAO = new SortieDAO();
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
                Console.WriteLine("7. Enregistrer un décès");
                Console.WriteLine("0. Retour");
                Console.WriteLine("========================");
                Console.Write("Votre choix : ");

                int.TryParse(Console.ReadLine(), out choix);

                switch (choix)
                {
                    case 1: await AjouterAnimal(animalDAO); break;
                    case 2: await ConsulterAnimal(animalDAO); break;
                    case 3: await SupprimerAnimal(animalDAO); break;
                    case 4: await AjouterInformationAnimal(animalDAO); break;
                    case 5: await SupprimerInformationAnimal(animalDAO); break;
                    case 6: await ListerAnimaux(animalDAO); break;
                    case 7: await EnregistrerDeces(animalDAO, entreeDAO, sortieDAO); break;
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

                    string nom              = AccesConsole.LireChaine("Nom");
                    string type             = AccesConsole.LireChaine("Type (chien/chat)");
                    string sexe             = AccesConsole.LireChaine("Sexe (M/F)");
                    string sterilise        = AccesConsole.LireChaine("Stérilisé (oui/non)");
                    string particularite    = AccesConsole.LireChaineOpt("Particularité (vide si aucune)");
                    string description      = AccesConsole.LireChaineOpt("Description (vide si aucune)");
                    DateTime dateN          = AccesConsole.LireDate("Date de naissance (yyyy-MM-dd)");
                    DateTime? dateD         = AccesConsole.LireDateOpt("Date de décès (yyyy-MM-dd vide si aucune)");
                    DateTime? dateS         = AccesConsole.LireDateOpt("Date de stérilisation (yyyy-MM-dd vide si aucune)");

                    Animal nouveauAnimal    = Animal.Create(nom, type, sexe, sterilise, particularite, description, dateN, dateD, dateS);

                    Animal? animalTrouve    = await dao.ChercherAnimalIdentiqueAsync(nouveauAnimal.Nom, nouveauAnimal.Type, nouveauAnimal.DateDeNaissance, nouveauAnimal.DateDeSterilisation);

                    if (animalTrouve != null && nouveauAnimal.EstIdentiqueA(animalTrouve))
                        throw new Exception("\nCet animal existe déjà dans la base !");

                    await dao.InsertAsync(nouveauAnimal);

                    try
                    {
                        await EnregistrerPremiereEntree(nouveauAnimal);
                    }
                    catch
                    {
                        // Si l'insert dans la table entrée échoue l'animal est supprimé sinon il y a un animal orphelin dans la DB (sans raison d'entrée)
                        await dao.DeleteAsync(nouveauAnimal);
                        throw; //relance l’erreur pour que le catch extérieur la gère
                    }

                    Console.WriteLine($"\nAnimal '{nouveauAnimal.Nom}' ajouté avec succès (ID : {nouveauAnimal.Identifiant}).");
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

                        Console.WriteLine   ("\n1. Particularité");
                        Console.WriteLine   ("2. Description");
                        Console.WriteLine   ("3. Date de décès");
                        Console.WriteLine   ("4. Date de stérilisation");
                        Console.Write       ("Votre choix : ");

                        int.TryParse(Console.ReadLine(), out int choix);

                        switch (choix)
                        {
                            case 1: animal.Particularite        = AccesConsole.LireChaineOpt("Nouvelle particularité"); break;
                            case 2: animal.Description          = AccesConsole.LireChaineOpt("Nouvelle description"); break;
                            case 3: animal.DateDeDeces          = AccesConsole.LireDateOpt("Nouvelle date de décès"); break;
                            case 4: animal.DateDeSterilisation  = AccesConsole.LireDateOpt("Nouvelle date de stérilisation"); break;
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

                        Console.WriteLine   ("\n1. Particularité");
                        Console.WriteLine   ("2. Description");
                        Console.Write       ("Votre choix : ");

                        int.TryParse(Console.ReadLine(), out int choix);

                        switch (choix)
                        {
                            case 1: animal.Particularite    = null; break;
                            case 2: animal.Description      = null; break;
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
                    Console.WriteLine   ($"{"ID",-12} {"Nom",-20} {"Type",-8} {"Sexe",-6} {"Stérilisé",-10} " +
                                        $"{"Naissance",-12} {"Décès",-12} {"Stérilisation",-15} {"Particularité",-20} {"Description",-30}");
                    Console.WriteLine(new string('-', 150));

                    foreach (Animal a in animaux)
                    {
                        string dateNaissance    = a.DateDeNaissance.ToString("yyyy-MM-dd");
                        string dateDeces        = a.DateDeDeces.HasValue ? a.DateDeDeces.Value.ToString("yyyy-MM-dd") : "-";
                        string dateSteril       = a.DateDeSterilisation.HasValue ? a.DateDeSterilisation.Value.ToString("yyyy-MM-dd") : "-";

                        Console.WriteLine(
                            $"{a.Identifiant,-12} " +
                            $"{a.Nom,-20} " +
                            $"{a.Type,-8} " +
                            $"{a.Sexe,-6} " +
                            $"{a.Sterilise,-10} " +
                            $"{dateNaissance,-12} " +
                            $"{dateDeces,-12} " +
                            $"{dateSteril,-15} " +
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

            string dateNaissance = animal.DateDeNaissance.ToString("yyyy-MM-dd");
            string dateDeces = animal.DateDeDeces.HasValue ? animal.DateDeDeces.Value.ToString("yyyy-MM-dd") : "-";
            string dateSteril = animal.DateDeSterilisation.HasValue ? animal.DateDeSterilisation.Value.ToString("yyyy-MM-dd") : "-";

            Console.WriteLine($"  Date naissance     : {dateNaissance}");
            Console.WriteLine($"  Date décès         : {dateDeces}");
            Console.WriteLine($"  Date stérilisation : {dateSteril}");
        }


        //=============================================================================================================================
        //                                                      ENREGISTRER DECES
        //=============================================================================================================================

        private async Task EnregistrerDeces(AnimalDAO animalDAO, EntreeDAO entreeDAO, SortieDAO sortieDAO)
        {
            string message = "Une erreur inconnue est survenue.";

            try
            {
                Console.WriteLine("===== ENREGISTRER UN ANIMAL DECEDE =====");

                string id = AccesConsole.DemanderId("Entrez l'identifiant de l'animal");

                // 1. Vérifier que l'animal existe
                Animal? animal = await animalDAO.SelectByIdAsync(id);

                if (animal == null)
                    throw new Exception("Aucun animal trouvé avec cet identifiant.");

                // 2. Récupérer entrée max et sortie max
                DateTime? dateEntreeMax = await entreeDAO.GetEntreeMaxAsync(id);
                DateTime? dateSortieMax = await sortieDAO.GetSortieMaxAsync(id);

                if (dateEntreeMax == null)
                    throw new Exception("Aucune entrée trouvée pour cet animal.");

                // 3. Lire la date de décès
                string saisie = AccesConsole.LireChaine("Saisissez la date de décès (JJ/MM/AAAA)");

                if (!DateTime.TryParse(saisie, out DateTime dateDeces))
                    throw new Exception("Format de date invalide.");

                // 4. Vérifications logiques
                if (dateDeces < dateEntreeMax)
                    throw new Exception("La date de décès ne peut pas être avant la dernière entrée.");

                if (dateSortieMax != null && dateDeces < dateSortieMax)
                    throw new Exception("La date de décès ne peut pas être avant la dernière sortie.");

                if (dateDeces > DateTime.Today)
                    throw new Exception("La date de décès ne peut pas être dans le futur.");

                // 5. Mise à jour de l'animal
                animal.DateDeDeces = dateDeces;
                await animalDAO.UpdateAsync(animal);

                // 6. Affichage des contacts disponibles
                ContactDAO daoContact = new ContactDAO();
                List<Contact> contacts = await daoContact.SelectAllAsync();

                Console.WriteLine("\n=== PERSONNES DE CONTACT DISPONIBLES ===");
                Console.WriteLine("Registre national".PadRight(20) + " | " + "Nom".PadRight(15) + " | " + "Prénom".PadRight(15));
                Console.WriteLine(new string('-', 55));

                foreach (var c in contacts)
                    Console.WriteLine(c.RegistreNational.PadRight(20) + " | " + c.Nom.PadRight(15) + " | " + c.Prenom.PadRight(15));

                // 7. Saisie du contact
                string registre = AccesConsole.LireChaine("Registre national du contact");
                Contact? contact = await daoContact.SelectByRegistreAsync(registre);

                if (contact == null)
                {
                    Console.WriteLine("\nAucun contact trouvé. Création d'un nouveau contact :");
                    contact = await ContactUI.Instance.AjouterContact(daoContact);
                }

                // 8. Insertion dans ani_sortie
                Sortie nouvelleSortie = Sortie.Create(animal, contact, dateDeces, "deces_animal");
                await sortieDAO.InsertAsync(nouvelleSortie);

                message = "Le décès a été enregistré avec succès.";
            }
            catch (Exception ex)
            {
                message = "Erreur : " + ex.Message;
            }

            Console.WriteLine(message);
            Console.ReadKey();
        }
        // ============================
        //   ENREGISTRER 1ÈRE ENTRÉE
        // ============================
        private async Task EnregistrerPremiereEntree(Animal nouveauAnimal)
        {
            // 1. Saisie de la raison — seule partie qui boucle si saisie invalide
            string raison = null;

            while (raison == null)
            {
                Console.WriteLine("\nChoisissez la raison de l'entrée :");
                Console.WriteLine(" 1. abandon");
                Console.WriteLine(" 2. errant");
                Console.WriteLine(" 3. deces_proprietaire");
                Console.WriteLine(" 4. saisie");
                Console.WriteLine(" 5. retour_adoption");
                Console.WriteLine(" 6. retour_famille_accueil");

                string choix = AccesConsole.LireChaine("Votre choix (1-6)");

                raison = choix switch
                {
                    "1" => "abandon",
                    "2" => "errant",
                    "3" => "deces_proprietaire",
                    "4" => "saisie",
                    "5" => "retour_adoption",
                    "6" => "retour_famille_accueil",
                    _ => null
                };

                if (raison == null)
                    Console.WriteLine("Choix invalide. Veuillez entrer un chiffre entre 1 et 6.");
            }

            // 2. Affichage de la liste des personnes de contact
            ContactDAO daoContact = new ContactDAO();
            List<Contact> contacts = await daoContact.SelectAllAsync();

            Console.WriteLine("\n=== PERSONNES DE CONTACT DISPONIBLES ===");
            Console.WriteLine("Registre national".PadRight(20) + " | " + "Nom".PadRight(15) + " | " + "Prénom".PadRight(15));
            Console.WriteLine(new string('-', 55));

            foreach (var c in contacts)
                Console.WriteLine(c.RegistreNational.PadRight(20) + " | " + c.Nom.PadRight(15) + " | " + c.Prenom.PadRight(15));

            // 3. Saisie du registre national
            string registre = AccesConsole.LireChaine("Registre national du contact");

            // 4. Vérification si le contact existe déjà ou non
            Contact? contact = await daoContact.SelectByRegistreAsync(registre);

            if (contact == null)
            {
                Console.WriteLine("\nAucun contact trouvé. Création d'un nouveau contact :");
                contact = await ContactUI.Instance.AjouterContact(daoContact);
            }

            // 5. Saisie de la date d'entrée
            DateTime dateEntree = AccesConsole.LireDate("Date d'entrée (yyyy-MM-dd)");

            // 6. Création de l'objet entrée et insertion en DB
            Entree nouvelleEntree = Entree.Create(nouveauAnimal, contact, dateEntree, raison);
            EntreeDAO daoEntree = new EntreeDAO();
            await daoEntree.InsertAsync(nouvelleEntree);
        }
    }
}