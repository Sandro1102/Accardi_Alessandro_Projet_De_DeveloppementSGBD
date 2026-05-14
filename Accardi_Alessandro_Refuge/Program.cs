using System;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheMetier;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // ---------------------------------------------------------
            // 1) RECREATION DES OBJETS DE BASE (ANIMAL + CONTACT)
            // ---------------------------------------------------------

            var animal = Animal.Create(
                nom: "Rex",
                type: "chien",
                sexe: "M",
                sterilise: "oui",
                particularite: "Très joueur",
                description: "Chien affectueux",
                dateDeNaissance: new DateTime(2021, 3, 15),
                dateDeDeces: DateTime.MinValue,
                dateDeSterilisation: new DateTime(2022, 4, 10)
            );
            animal.Identifiant = "21031500001"; // ⚠ Mets l'identifiant réel


            var contact = Contact.Create(
                nom: "Dupont",
                prenom: "Marie",
                registreNational: "95061312345",
                rue: "Rue des Lilas 25",
                cp: "4000",
                localite: "Liège",
                gsm: "0485123456",
                telephoneFixe: null,
                email: "marie.dupont@example.com"
            );
            contact.Identifiant = 2; // ⚠ Ton contact existant


            // ---------------------------------------------------------
            // 2) INSERT
            // ---------------------------------------------------------

            var fa = Famille_Accueil.Create(
                animal: animal,
                contact: contact,
                date: DateTime.Today,
                dateFin: DateTime.Today.AddDays(30) // 1 mois de FA
            );

            var dao = new Famille_AccueilDAO();

            Console.WriteLine("=== INSERT FAMILLE ACCUEIL ===");
            await dao.InsertAsync(fa);
            Console.WriteLine($"Insertion réussie ! ID généré : {fa.Identifiant}");

            Console.WriteLine("\nAppuie sur ENTER pour continuer vers UPDATE...");
            Console.ReadLine();


            // ---------------------------------------------------------
            // 3) UPDATE
            // ---------------------------------------------------------

            fa.DateFin = DateTime.Today.AddDays(45); // prolongation de 15 jours

            Console.WriteLine("=== UPDATE FAMILLE ACCUEIL ===");
            await dao.UpdateAsync(fa);
            Console.WriteLine("Mise à jour réussie !");

            Console.WriteLine("\nAppuie sur ENTER pour continuer vers DELETE...");
            Console.ReadLine();


            // ---------------------------------------------------------
            // 4) DELETE
            // ---------------------------------------------------------

            Console.WriteLine("=== DELETE FAMILLE ACCUEIL ===");
            await dao.DeleteAsync(fa);
            Console.WriteLine("Suppression réussie !");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERREUR :");
            Console.WriteLine(ex.GetType().FullName + " - " + ex.Message);
            Console.WriteLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                Console.WriteLine("--- InnerException ---");
                Console.WriteLine(ex.InnerException.GetType().FullName + " - " + ex.InnerException.Message);
            }
        }
    }
}
