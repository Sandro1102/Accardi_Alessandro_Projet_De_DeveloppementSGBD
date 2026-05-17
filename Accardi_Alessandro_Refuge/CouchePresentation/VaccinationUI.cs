using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge.CoucheMetier;

namespace Accardi_Alessandro_Refuge.CouchePresentation
{
    internal record VaccinationUI
    {
        public static VaccinationUI Instance { get; } = new();

        private VaccinationUI() { }

        public async Task MenuVaccinations()
        {
            VaccinationDAO dao = new VaccinationDAO();
            int choix;

            do
            {
                Console.Clear();
                Console.WriteLine("===== MENU VACCINATIONS =====");
                Console.WriteLine("1. Enregistrer une vaccination");
                Console.WriteLine("2. Supprimer une vaccination");
                Console.WriteLine("3. Lister les vaccinations d'un animal");
                Console.WriteLine("0. Retour");
                Console.WriteLine("=============================");

                int.TryParse(
                    AccesConsole.LireChaine("Votre choix"),
                    out choix
                );

                switch (choix)
                {
                    case 1:
                        await EnregistrerVaccination(dao);
                        break;

                    case 2:
                        await SupprimerVaccination(dao);
                        break;

                    case 3:
                        await ListerVaccinationsParAnimal(dao);
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
        //   ENREGISTRER VACCINATION
        // ============================

        private async Task EnregistrerVaccination(VaccinationDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("===== ENREGISTRER UNE VACCINATION =====\n");

                    // 1. Charger l'animal
                    string identifiantAnimal = AccesConsole.LireChaine("Id de l'animal");
                    AnimalDAO daoAnimal = new AnimalDAO();
                    Animal animal = await daoAnimal.SelectByIdAsync(identifiantAnimal);

                    if (animal == null)
                        throw new Exception("Animal introuvable.");

                    // 2. Vérifier que l'animal n'est pas décédé
                    Animal.AnimalDecede(animal.DateDeDeces);

                    // 3. Afficher la liste des vaccins disponibles
                    VaccinDAO daoVaccin = new VaccinDAO();
                    List<Vaccin> vaccins = await daoVaccin.SelectAllAsync();

                    if (vaccins == null || vaccins.Count == 0)
                        throw new Exception("Aucun vaccin disponible dans le système.");

                    Console.WriteLine("\nVaccins disponibles :\n");

                    AccesConsole.AfficherListe(
                        vaccins,
                        v => $"ID : {v.Identifiant} | Nom : {v.Nom}"
                    );

                    // 4. Saisir l'id du vaccin
                    string saisieVaccin = AccesConsole.LireChaine("\nId du vaccin");

                    if (!int.TryParse(saisieVaccin, out int idVaccin))
                        throw new Exception("L'identifiant du vaccin doit être un nombre.");

                    Vaccin vaccinChoisi = vaccins.FirstOrDefault(v => v.Identifiant == idVaccin);

                    if (vaccinChoisi == null)
                        throw new Exception("Vaccin introuvable.");

                    // 5. Saisir la date de vaccination
                    DateTime dateVaccination = AccesConsole.LireDate("Date de vaccination (yyyy-MM-dd)");

                    if (dateVaccination < animal.DateDeNaissance)
                        throw new Exception("La date de vaccination ne peut pas être avant la date de naissance de l'animal.");

                    // 6. Vérifier que cette vaccination n'existe pas déjà
                    bool dejaVaccine = await dao.VaccinationExisteAsync(animal.Identifiant, idVaccin, dateVaccination);

                    if (dejaVaccine)
                        throw new Exception("Cet animal a déjà reçu ce vaccin à cette date.");

                    // 7. Créer et insérer la vaccination
                    Vaccination nouvelleVaccination = Vaccination.Create(animal, vaccinChoisi, dateVaccination);
                    await dao.InsertAsync(nouvelleVaccination);

                    Console.WriteLine("\nVaccination enregistrée avec succès !");
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
        //   SUPPRIMER VACCINATION
        // ============================

        private async Task SupprimerVaccination(VaccinationDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("===== SUPPRIMER UNE VACCINATION =====\n");

                    // 1. Charger l'animal
                    string identifiantAnimal = AccesConsole.LireChaine("Id de l'animal");
                    AnimalDAO daoAnimal = new AnimalDAO();
                    Animal animal = await daoAnimal.SelectByIdAsync(identifiantAnimal);

                    if (animal == null)
                        throw new Exception("Animal introuvable.");

                    // 2. Afficher les vaccinations de cet animal
                    List<Vaccination> vaccinations = await dao.SelectByAnimalAsync(animal.Identifiant);

                    if (vaccinations == null || vaccinations.Count == 0)
                    {
                        Console.WriteLine($"\nAucune vaccination trouvée pour {animal.Nom}.");
                        Console.ReadKey();
                        continuer = false;
                    }
                    else
                    {
                        Console.WriteLine($"\nVaccinations de {animal.Nom} :\n");

                        AccesConsole.AfficherListe(
                            vaccinations,
                            v =>
                                $"Vaccin ID : {v.VaccinApplique.Identifiant} | " +
                                $"Nom : {v.VaccinApplique.Nom} | " +
                                $"Date : {v.DateVaccination:yyyy-MM-dd}"
                        );

                        // 3. Saisir l'id du vaccin à supprimer
                        string saisieVaccin = AccesConsole.LireChaine("\nId du vaccin à supprimer");

                        if (!int.TryParse(saisieVaccin, out int idVaccin))
                            throw new Exception("L'identifiant du vaccin doit être un nombre.");

                        // 4. Saisir la date
                        DateTime dateVaccination = AccesConsole.LireDate("Date de la vaccination (yyyy-MM-dd)");

                        // 5. Vérifier que la vaccination existe bien
                        bool existe = await dao.VaccinationExisteAsync(animal.Identifiant, idVaccin, dateVaccination);

                        if (!existe)
                            throw new Exception("Aucune vaccination trouvée avec ces critères.");

                        // 6. Supprimer
                        await dao.SupprimerVaccinationAsync(animal.Identifiant, idVaccin, dateVaccination);

                        Console.WriteLine("\nVaccination supprimée avec succès !");
                        Console.ReadKey();
                        continuer = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nErreur : {ex.Message}");
                    continuer = AccesConsole.DemanderReessayer();
                }
            }
        }

        // ============================
        //   LISTER VACCINATIONS
        // ============================

        private async Task ListerVaccinationsParAnimal(VaccinationDAO dao)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("===== VACCINATIONS PAR ANIMAL =====\n");

                string identifiantAnimal = AccesConsole.LireChaine("Id de l'animal");
                AnimalDAO daoAnimal = new AnimalDAO();
                Animal animal = await daoAnimal.SelectByIdAsync(identifiantAnimal);

                if (animal == null)
                    throw new Exception("Animal introuvable.");

                List<Vaccination> liste = await dao.SelectByAnimalAsync(animal.Identifiant);

                if (liste == null || liste.Count == 0)
                {
                    Console.WriteLine($"\nAucune vaccination trouvée pour {animal.Nom}.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"\nVaccinations de {animal.Nom} ({animal.Identifiant}) :\n");

                AccesConsole.AfficherListe(
                    liste,
                    v =>
                        $"Vaccin : {v.VaccinApplique.Nom} (ID : {v.VaccinApplique.Identifiant}) | " +
                        $"Date : {v.DateVaccination:yyyy-MM-dd}"
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