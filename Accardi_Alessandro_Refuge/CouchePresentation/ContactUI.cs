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

                    try
                    {
                        await EnregistrerRoles(nouveauContact);
                    }
                    catch
                    {
                        // Si l'insert des rôles échoue le contact est supprimé sinon il y a un contact orphelin dans la DB (sans rôle)
                        await dao.DeleteAsync(nouveauContact);
                        throw; // relance l'erreur pour que le catch extérieur la gère
                    }

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
                        await AfficherContact(c);

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
                    await AfficherContact(c);

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

        private async Task AfficherContact(Contact c)
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

            // Récupération et affichage des rôles
            RoleDAO daoRole = new RoleDAO();
            Personne_RoleDAO daoPersonneRole = new Personne_RoleDAO();

            List<Personne_RoleDAO.PersonneRole> liens = await daoPersonneRole.SelectByContactAsync(c.Identifiant);
            List<Role> roles = await daoRole.SelectAllAsync();

            List<string> nomsRolesContact = liens
                .Select(lien => roles.FirstOrDefault(r => r.Identifiant == lien.RoleId)?.Nom ?? "?")
                .ToList();

            string affichageRoles = nomsRolesContact.Count > 0 ? string.Join(", ", nomsRolesContact) : "-";

            Console.WriteLine($"  Rôles              : {affichageRoles}");
        }

        private async Task EnregistrerRoles(Contact contact)
        {
            RoleDAO daoRole = new RoleDAO();
            Personne_RoleDAO daoPersonneRole = new Personne_RoleDAO();

            List<Role> roles = await daoRole.SelectAllAsync();

            if (roles == null || roles.Count == 0)
                throw new Exception("Aucun rôle disponible dans le système.");

            Console.WriteLine("\n=== RÔLES DISPONIBLES ===");
            Console.WriteLine($"{"ID",-5} {"Nom",-30}");
            Console.WriteLine(new string('-', 35));

            foreach (Role r in roles)
                Console.WriteLine($"{r.Identifiant,-5} {r.Nom,-30}");

            List<int> rolesChoisis = new List<int>();
            bool continuerSaisie = true;

            Console.WriteLine("\nIntroduisez les ID des rôles du contact (minimum 1).");
            Console.WriteLine("Appuyez sur Entrée sans valeur pour terminer la saisie.\n");

            while (continuerSaisie)
            {
                string saisie = AccesConsole.LireChaine("ID rôle");

                if (string.IsNullOrWhiteSpace(saisie) && rolesChoisis.Count == 0)
                {
                    Console.WriteLine("Vous devez saisir au moins un rôle.");
                }
                else if (string.IsNullOrWhiteSpace(saisie))
                {
                    continuerSaisie = false;
                }
                else if (!int.TryParse(saisie, out int idRole))
                {
                    Console.WriteLine("L'identifiant doit être un nombre.");
                }
                else if (roles.FirstOrDefault(r => r.Identifiant == idRole) == null)
                {
                    Console.WriteLine("Rôle introuvable, veuillez choisir un ID dans la liste.");
                }
                else if (rolesChoisis.Contains(idRole))
                {
                    Console.WriteLine("Ce rôle a déjà été ajouté.");
                }
                else
                {
                    rolesChoisis.Add(idRole);
                    Role roleChoisi = roles.First(r => r.Identifiant == idRole);
                    Console.WriteLine($"Rôle '{roleChoisi.Nom}' ajouté.");
                }
            }

            foreach (int idRole in rolesChoisis)
            {
                Personne_RoleDAO.PersonneRole lien = new Personne_RoleDAO.PersonneRole(contact.Identifiant, idRole);
                await daoPersonneRole.InsertAsync(lien);
            }
        }
    }
}
