using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge.CoucheMetier;

namespace Accardi_Alessandro_Refuge.CouchePresentation
{
    internal record Famille_AccueilUI
    {
        public static Famille_AccueilUI Instance { get; } = new();

        private Famille_AccueilUI() { }

        public async Task MenuFamilleAccueil()
        {
            Famille_AccueilDAO dao = new Famille_AccueilDAO();
            int choix;

            do
            {
                Console.Clear();
                Console.WriteLine("===== MENU FAMILLE D'ACCUEIL =====");
                Console.WriteLine("1. Enregistrer un départ en famille d'accueil");
                Console.WriteLine("2. Enregistrer un retour de famille d'accueil");
                Console.WriteLine("3. Lister les familles d'accueil");
                Console.WriteLine("4. Lister les familles d'accueil d'un animal");
                Console.WriteLine("0. Retour");
                Console.WriteLine("==================================");

                int.TryParse(
                    AccesConsole.LireChaine("Votre choix"),
                    out choix
                );

                switch (choix)
                { 
                    case 1:await EnregistrerDepart(dao);             break;

                    case 2:await EnregistrerRetour(dao);             break;

                    case 3:await ListerFamillesAccueil(dao);         break;

                    case 4:await ListerFamillesAccueilParAnimal(dao);break;

                    case 0:
                        break;

                    default:
                        Console.WriteLine("Choix invalide.");
                        Console.ReadKey();
                        break;
                }

            } while (choix != 0);
        }

        // ============================
        //   DÉPART EN FAMILLE ACCUEIL
        // ============================

        private async Task EnregistrerDepart(Famille_AccueilDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("===== DÉPART EN FAMILLE D'ACCUEIL =====\n");

                    // 1. Lire et charger l'animal
                    string      identifiantAnimal   = AccesConsole.LireChaine("Id de l'animal");
                    AnimalDAO   daoAnimal           = new AnimalDAO();
                    Animal      animal              = await daoAnimal.SelectByIdAsync(identifiantAnimal);

                    if (animal == null)
                        throw new Exception("Animal introuvable.");

                    // 2. Vérifier que l'animal n'est pas décédé
                    Animal.AnimalDecede(animal.DateDeDeces);

                    // 3. Vérifier qu'il n'est pas déjà en famille d'accueil
                    Famille_Accueil? faActive = await dao.SelectFaActiveParAnimalAsync(animal.Identifiant);

                    if (faActive != null)
                        throw new Exception("Cet animal est déjà en famille d'accueil.");

                    // 4. Lire et charger le contact
                    string      identifiantContact  = AccesConsole.LireChaine("Registre national du contact");
                    ContactDAO  daoContact          = new ContactDAO();
                    Contact     contact             = await daoContact.SelectByRegistreAsync(identifiantContact);

                    if (contact == null)
                        throw new Exception("Contact introuvable.");

                    // 5. Lire la date de départ
                    DateTime dateDepart = AccesConsole.LireDate("Date de départ (yyyy-MM-dd)");

                    // 6. Créer et insérer la FA (sans date de fin)
                    Famille_Accueil nouvelleFa = Famille_Accueil.Create(animal, contact, dateDepart, null);
                    await dao.InsertAsync(nouvelleFa);

                    // 7. Créer la sortie correspondante
                    SortieDAO   daoSortie       = new SortieDAO();
                    Sortie      nouvelleSortie  = Sortie.Create(animal, contact, dateDepart, "famille_accueil");
                    await       daoSortie.InsertAsync(nouvelleSortie);

                    Console.WriteLine("\nDépart en famille d'accueil enregistré avec succès !");
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
        //   RETOUR DE FAMILLE ACCUEIL
        // ============================

        private async Task EnregistrerRetour(Famille_AccueilDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("===== RETOUR DE FAMILLE D'ACCUEIL =====\n");

                    // 1. Lire et charger l'animal
                    string      identifiantAnimal   = AccesConsole.LireChaine("Id de l'animal");
                    AnimalDAO   daoAnimal           = new AnimalDAO();
                    Animal      animal              = await daoAnimal.SelectByIdAsync(identifiantAnimal);

                    if (animal == null)
                        throw new Exception("Animal introuvable.");

                    // 2. Vérifier qu'il est bien en famille d'accueil
                    Famille_Accueil? faActive = await dao.SelectFaActiveParAnimalAsync(animal.Identifiant);

                    if (faActive == null)
                        throw new Exception("Cet animal n'est pas actuellement en famille d'accueil.");

                    // 3. Lire la date de retour
                    DateTime dateRetour = AccesConsole.LireDate("Date de retour (yyyy-MM-dd)");

                    // 4. Mettre à jour la FA avec la date de fin
                    faActive.DateFin = dateRetour;
                    await dao.UpdateAsync(faActive);

                    // 5. Créer l'entrée correspondante
                    EntreeDAO   daoEntree       = new EntreeDAO();
                    Entree      nouvelleEntree  = Entree.Create(animal, faActive.ContactConcerne, dateRetour, "retour_famille_accueil");
                    await       daoEntree.InsertAsync(nouvelleEntree);

                    Console.WriteLine("\nRetour de famille d'accueil enregistré avec succès !");
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
        //       LISTER FA
        // ============================

        private async Task ListerFamillesAccueil(Famille_AccueilDAO dao)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("===== LISTE DES FAMILLES D'ACCUEIL =====\n");

                List<Famille_Accueil> liste = await dao.SelectAllAsync();

                if (liste == null || liste.Count == 0)
                {
                    Console.WriteLine("Aucune famille d'accueil trouvée.");
                    Console.ReadKey();
                    return;
                }

                AccesConsole.AfficherListe(
                    liste,
                    fa =>
                        $"ID        : {fa.Identifiant} | " +
                        $"Animal    : {fa.AnimalConcerne.Nom} ({fa.AnimalConcerne.Identifiant}) | " +
                        $"Contact   : {fa.ContactConcerne.Nom} {fa.ContactConcerne.Prenom} | " +
                        $"Départ    : {fa.Date:yyyy-MM-dd} | " +
                        $"Retour    : {(fa.DateFin.HasValue ? fa.DateFin.Value.ToString("yyyy-MM-dd") : "En cours")}"
                );

                Console.WriteLine();
                Console.WriteLine("Appuyez sur une touche pour continuer...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nErreur : {ex.Message}");
                Console.ReadKey();
            }
        }

        // ===================================================
        //       LISTER FA PAR LESQUELLES UN ANIMAL EST PASSE
        // ===================================================
        private async Task ListerFamillesAccueilParAnimal(Famille_AccueilDAO dao)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("===== FAMILLES D'ACCUEIL PAR ANIMAL =====\n");

                string identifiantAnimal = AccesConsole.LireChaine("Id de l'animal");

                AnimalDAO   daoAnimal   = new AnimalDAO();
                Animal      animal      = await daoAnimal.SelectByIdAsync(identifiantAnimal);

                if (animal == null)
                    throw new Exception("Animal introuvable.");

                List<Famille_Accueil> liste = await dao.SelectByAnimalAsync(animal.Identifiant);

                if (liste == null || liste.Count == 0)
                {
                    Console.WriteLine($"\nAucune famille d'accueil trouvée pour {animal.Nom}.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"\nFamilles d'accueil de {animal.Nom} ({animal.Identifiant}) :\n");

                AccesConsole.AfficherListe(
                    liste,
                    fa =>
                        $"ID        : {fa.Identifiant} | " +
                        $"Contact   : {fa.ContactConcerne.Nom} {fa.ContactConcerne.Prenom} | " +
                        $"Départ    : {fa.Date:yyyy-MM-dd} | " +
                        $"Retour    : {(fa.DateFin.HasValue ? fa.DateFin.Value.ToString("yyyy-MM-dd") : "En cours")}"
                );

                Console.WriteLine();
                Console.WriteLine("Appuyez sur une touche pour continuer...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nErreur : {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}