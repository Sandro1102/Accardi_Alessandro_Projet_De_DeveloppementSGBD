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
            // 1) Recréation de l'objet Contact existant
            //    ⚠ Le registre national DOIT être identique à celui en base
            var contact = Contact.Create(
                nom: "Dupont",
                prenom: "Marie",
                registreNational: "95061312345",
                rue: "Rue des Lilas 25",
                cp: "4000",
                localite: "Liège",
                gsm: "0485123456",
                telephoneFixe: null,
                email: "marie.dupont.new@example.com"
            );

            // 2) DAO
            var dao = new ContactDAO();

            Console.WriteLine("Suppression du contact en base...");
            await dao.DeleteAsync(contact);

            Console.WriteLine("Suppression réussie !");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERREUR lors de la suppression :");
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
