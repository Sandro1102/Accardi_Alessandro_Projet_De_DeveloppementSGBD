using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge.CoucheMetier;

namespace Accardi_Alessandro_Refuge.CouchePresentation
{
    internal record AdoptionUI
    {
        public static AdoptionUI Instance { get; } = new();

        private AdoptionUI() { }

        public async Task MenuAdoptions()
        {
            AdoptionDAO dao = new AdoptionDAO();
            int choix;

            do
            {
                Console.Clear();

                Console.WriteLine("===== MENU ADOPTIONS =====");
                Console.WriteLine("1. Ajouter une adoption");
                Console.WriteLine("2. Modifier une adoption");
                Console.WriteLine("3. Lister les adoptions et leur statut");
                Console.WriteLine("0. Retour");
                Console.WriteLine("==========================");

                int.TryParse(
                    AccesConsole.LireChaine("Votre choix"),
                    out choix
                );

                switch (choix)
                {
                    case 1:
                        await AjouterAdoption(dao);
                        break;

                    case 2:
                        await ModifierAdoption(dao);
                        break;

                    case 3:
                        await ListerAdoption(dao);
                        break;

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
        //       AJOUTER ADOPTION
        // ============================

        private async Task AjouterAdoption(AdoptionDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("=== AJOUTER ADOPTION ===");

                    string identifiantAnimal =
                        AccesConsole.LireChaine("Id animal");

                    string identifiantContact =
                        AccesConsole.LireChaine("Registre national du contact");

                    DateTime date =
                        AccesConsole.LireDate("Date de demande (yyyy-MM-dd)");

                    string statut =
                        AccesConsole.LireChaine("Statut");

                    AnimalDAO daoAnimal = new AnimalDAO();
                    ContactDAO daoContact = new ContactDAO();

                    Animal animal =
                        await daoAnimal.SelectByIdAsync(identifiantAnimal);

                    Contact contact =
                        await daoContact.SelectByRegistreAsync(identifiantContact);

                    if (animal == null)
                        throw new Exception("Animal introuvable");

                    if (contact == null)
                        throw new Exception("Contact introuvable");

                    Adoption adoptionAValider =
                        Adoption.Create(animal, contact, date, statut);

                    List<Adoption> adoptionsExistantes =
                        await dao.SelectByAnimalAsync(animal.Identifiant);

                    if (!adoptionAValider.EstDisponiblePourAdoption(adoptionsExistantes))
                    {
                        throw new Exception(
                            "Cet animal possède déjà une demande en cours ou acceptée."
                        );
                    }

                    await dao.InsertAsync(adoptionAValider);

                    Console.WriteLine("\nAdoption ajoutée avec succès !");
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
        //       MODIFIER ADOPTION
        // ============================

        private async Task ModifierAdoption(AdoptionDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("===== MODIFIER ADOPTION =====\n");

                    await ListerAdoption(dao);

                    string cible =
                        AccesConsole.DemanderId(
                            "\nIntroduisez l'id de l'adoption à modifier"
                        );

                    Adoption adoptionAModifier =
                        await dao.SelectByIdAsync(cible);

                    if (adoptionAModifier == null)
                        throw new Exception("Adoption introuvable");

                    Console.WriteLine();
                    Console.WriteLine("Que souhaitez-vous modifier ?");
                    Console.WriteLine("1. Statut");
                    Console.WriteLine("2. Date");
                    Console.WriteLine("0. Annuler");

                    int.TryParse(
                        AccesConsole.LireChaine("Votre choix"),
                        out int choix
                    );

                    switch (choix)
                    {
                        case 1:

                            string nouveauStatut =
                                AccesConsole.LireChaine("Nouveau statut");

                            adoptionAModifier.Statut = nouveauStatut;
                            break;

                        case 2:

                            DateTime nouvelleDate =
                                AccesConsole.LireDate(
                                    "Nouvelle date (yyyy-MM-dd)"
                                );

                            adoptionAModifier.Date = nouvelleDate;
                            break;

                        case 0:

                            Console.WriteLine("Modification annulée.");
                            Console.ReadKey();
                            return;

                        default:

                            Console.WriteLine("Choix invalide.");
                            Console.ReadKey();
                            return;
                    }

                    await dao.UpdateAsync(adoptionAModifier);

                    Console.WriteLine("\nAdoption modifiée avec succès !");
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
        //       LISTER ADOPTIONS
        // ============================

        private async Task ListerAdoption(AdoptionDAO dao)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("===== LISTE DES ADOPTIONS =====\n");

                List<Adoption> liste = await dao.SelectAllAsync();

                if (liste == null || liste.Count == 0)
                {
                    Console.WriteLine("Aucune adoption trouvée.");
                    Console.ReadKey();
                    return;
                }

                AccesConsole.AfficherListe(
                    liste,
                    adoption =>
                        $"ID : {adoption.Identifiant} | " +
                        $"Animal : {adoption.AnimalConcerne.Nom} ({adoption.AnimalConcerne.Identifiant}) | " +
                        $"Contact : {adoption.ContactConcerne.Nom} {adoption.ContactConcerne.Prenom} | " +
                        $"Date : {adoption.Date:yyyy-MM-dd} | " +
                        $"Statut : {adoption.Statut}"
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