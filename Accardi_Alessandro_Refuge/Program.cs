using System;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheMetier;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using static Accardi_Alessandro_Refuge.CoucheBaseDeDonnees.Personne_RoleDAO;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // ---------------------------------------------------------
            // 1) RECREATION DU CONTACT MARIE DUPONT
            // ---------------------------------------------------------

            var contact = Contact.Create(
                nom: "Dupont",
                prenom: "Marie",
                telephoneFixe: null,                         // pas de fixe
                gsm: "0471223344",
                email: "marie.dupont@example.com",
                rue: "Rue des Fleurs 12",
                cp: "4000",
                localite: "Liège",
                registreNational: "93052112345"          // exemple correct
            );

            contact.Identifiant = 2; // identifiant réel dans ta DB


            // ---------------------------------------------------------
            // 2) INSERT D'UN ROLE POUR CE CONTACT
            // ---------------------------------------------------------

            // ⚠ Choisis un rôle existant dans ta table role (ex: id = 1 → adoptant)
            var lien = new PersonneRole(
                PersonneId: contact.Identifiant,
                RoleId: 1
            );

            var dao = new Personne_RoleDAO();

            Console.WriteLine("=== INSERT ROLE POUR CONTACT ===");
            await dao.InsertAsync(lien);
            Console.WriteLine("Insertion réussie !");

            Console.WriteLine("\nAppuie sur ENTER pour continuer vers DELETE...");
            Console.ReadLine();


            // ---------------------------------------------------------
            // 3) DELETE DU LIEN
            // ---------------------------------------------------------

            Console.WriteLine("=== DELETE ROLE POUR CONTACT ===");
            await dao.DeleteAsync(lien);
            Console.WriteLine("Suppression réussie !");

            Console.WriteLine("\nTest terminé !");
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
