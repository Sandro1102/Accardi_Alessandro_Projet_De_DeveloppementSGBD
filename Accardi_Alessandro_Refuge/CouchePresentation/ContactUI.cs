using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge.CoucheMetier;

namespace Accardi_Alessandro_Refuge.CouchePresentation
{
    internal record ContactUI
    {
        public static ContactUI Instance { get; } = new();
        private ContactUI() { }

        // ============================
        //        MENU CONTACT
        // ============================

        public async Task MenuContacts()
        {
            ContactDAO dao = new ContactDAO();
            int choix;

            do
            {
                Console.Clear();
                Console.WriteLine("===== MENU CONTACTS =====");
                Console.WriteLine("1. Ajouter une personne de contact");
                Console.WriteLine("2. Consulter une personne de contact");
                Console.WriteLine("3. Modifier les coordonnées");
                Console.WriteLine("4. Supprimer une personne de contact");
                Console.WriteLine("0. Retour");
                Console.WriteLine("========================");
                Console.Write("Votre choix : ");

                int.TryParse(Console.ReadLine(), out choix);

                switch (choix)
                {
                    case 1: await AjouterContact    (dao); break;
                    case 2: await ConsulterContact  (dao); break;
                    case 3: await ModifierContact   (dao); break;
                    case 4: await SupprimerContact  (dao); break;
                    case 0: break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        Console.ReadKey();
                        break;
                }

            } while (choix != 0);
        }

        // ============================
        //       AJOUTER CONTACT
        // ============================

        public async Task<Contact> AjouterContact(ContactDAO dao)
        {
            bool continuer = true;
            Contact nouveauContact = null;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("=== AJOUTER UNE PERSONNE DE CONTACT ===");

                    string nom = AccesConsole.LireChaine("Nom");
                    string prenom = AccesConsole.LireChaine("Prénom");
                    string rn = AccesConsole.LireChaine("Registre national (ex : 95.06.13-123.45)");
                    string rue = AccesConsole.LireChaine("Rue");
                    string cp = AccesConsole.LireChaine("Code postal");
                    string localite = AccesConsole.LireChaine("Localité");
                    string gsm = AccesConsole.LireChaineOpt("GSM (optionnel)");
                    string tel = AccesConsole.LireChaineOpt("Téléphone fixe (optionnel)");
                    string email = AccesConsole.LireChaineOpt("Email (optionnel)");

                    nouveauContact = Contact.Create(nom, prenom, rn, rue, cp, localite, gsm, tel, email);

                    await dao.InsertAsync(nouveauContact);

                    Console.WriteLine("\nContact ajouté avec succès !");
                    Console.ReadKey();
                    continuer = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nErreur : {ex.Message}");
                    continuer = AccesConsole.DemanderReessayer();
                }
            }

            return nouveauContact;
        }


        // ============================
        //      CONSULTER CONTACT
        // ============================

        private async Task ConsulterContact(ContactDAO dao)
        {
            Console.Clear();
            Console.WriteLine("=== CONSULTER UNE PERSONNE DE CONTACT ===");

            string rn = AccesConsole.DemanderId("Entrez le registre national");

            try
            {
                Contact c = await dao.SelectByRegistreAsync(rn);

                if (c == null)
                {
                    Console.WriteLine("Aucun contact trouvé avec ce registre national.");
                }
                else
                {
                    AfficherContact(c);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }

            Console.ReadKey();
        }

        // ============================
        //      MODIFIER CONTACT
        // ============================

        private async Task ModifierContact(ContactDAO dao)
        {
            bool continuer = true;

            while (continuer)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("=== MODIFIER LES COORDONNÉES ===");

                    string rn = AccesConsole.DemanderId("Registre national du contact");
                    Contact c = await dao.SelectByRegistreAsync(rn);

                    if (c == null)
                    {
                        Console.WriteLine("Aucun contact trouvé.");
                        continuer = false;
                    }
                    else
                    {
                        AfficherContact(c);

                        Console.WriteLine("\nQuelle information souhaitez-vous modifier ?");
                        Console.WriteLine("1. Rue");
                        Console.WriteLine("2. Code postal");
                        Console.WriteLine("3. Localité");
                        Console.WriteLine("4. GSM");
                        Console.WriteLine("5. Téléphone fixe");
                        Console.WriteLine("6. Email");
                        Console.Write("Votre choix : ");

                        int.TryParse(Console.ReadLine(), out int choix);

                        switch (choix)
                        {
                            case 1: c.Rue       = AccesConsole.LireChaine   ("Nouvelle rue");            break;
                            case 2: c.Cp        = AccesConsole.LireChaine   ("Nouveau code postal");     break;
                            case 3: c.Localite  = AccesConsole.LireChaine   ("Nouvelle localité");       break;
                            case 4: c.Gsm       = AccesConsole.LireChaineOpt("Nouveau GSM");             break;
                            case 5: c.Telephone = AccesConsole.LireChaineOpt("Nouveau téléphone fixe");  break;
                            case 6: c.Email     = AccesConsole.LireChaineOpt("Nouvel email");            break;
                            default:
                                Console.WriteLine("Choix invalide.");
                                break;
                        }

                        await dao.UpdateAsync(c);
                        Console.WriteLine("\nCoordonnées mises à jour avec succès.");
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
        //      SUPPRIMER CONTACT
        // ============================

        private async Task SupprimerContact(ContactDAO dao)
        {
            Console.Clear();
            Console.WriteLine("=== SUPPRIMER UNE PERSONNE DE CONTACT ===");

            string rn = AccesConsole.DemanderId("Registre national du contact");

            try
            {
                Contact c = await dao.SelectByRegistreAsync(rn);

                if (c == null)
                {
                    Console.WriteLine("Aucun contact trouvé.");
                }
                else
                {
                    AfficherContact(c);

                    if (AccesConsole.Confirmation($"Confirmer la suppression de {c.Nom} {c.Prenom}"))
                    {
                        await dao.DeleteAsync(c);
                        Console.WriteLine("Contact supprimé avec succès.");
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
        //   AFFICHAGE D'UN CONTACT
        // ============================

        private void AfficherContact(Contact c)
        {
            Console.WriteLine();
            Console.WriteLine($"  Identifiant        : {c.Identifiant}");
            Console.WriteLine($"  Nom                : {c.Nom}");
            Console.WriteLine($"  Prénom             : {c.Prenom}");
            Console.WriteLine($"  Registre national  : {c.RegistreNational}");
            Console.WriteLine($"  Rue                : {c.Rue}");
            Console.WriteLine($"  Code postal        : {c.Cp}");
            Console.WriteLine($"  Localité           : {c.Localite}");
            Console.WriteLine($"  GSM                : {c.Gsm ?? "-"}");
            Console.WriteLine($"  Téléphone fixe     : {c.Telephone ?? "-"}");
            Console.WriteLine($"  Email              : {c.Email ?? "-"}");
        }
    }
}
