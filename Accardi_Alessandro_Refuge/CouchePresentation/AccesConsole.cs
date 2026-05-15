using System;
using System.Collections.Generic;

namespace Accardi_Alessandro_Refuge.CouchePresentation
{
    internal static class AccesConsole
    {
        // ============================
        //   LECTURE DE CHAINES
        // ============================

        public static string LireChaine(string libelle)
        {
            Console.Write($"{libelle} : ");
            return Console.ReadLine()?.Trim();
        }

        public static string LireChaineOpt(string libelle)
        {
            Console.Write($"{libelle} : ");
            string valeur = Console.ReadLine()?.Trim();
            return string.IsNullOrWhiteSpace(valeur) ? null : valeur;
        }

        // ============================
        //   LECTURE DE DATES
        // ============================

        public static DateTime LireDate(string libelle)
        {
            while (true)
            {
                Console.Write($"{libelle} : ");
                string saisie = Console.ReadLine();

                if (DateTime.TryParse(saisie, out DateTime date))
                    return date;

                Console.WriteLine("Format invalide. Réessayez (yyyy-MM-dd).");
            }
        }

        public static DateTime LireDateOpt(string libelle)
        {
            while (true)
            {
                Console.Write($"{libelle} : ");
                string saisie = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(saisie))
                    return DateTime.MinValue;

                if (DateTime.TryParse(saisie, out DateTime date))
                    return date;

                Console.WriteLine("Format invalide. Réessayez (yyyy-MM-dd) ou laissez vide.");
            }
        }

        // ============================
        //   CONFIRMATION / REESSAI
        // ============================

        public static bool Confirmation(string message)
        {
            Console.Write($"{message} (oui/non) : ");
            string rep = Console.ReadLine()?.Trim().ToLower();
            return rep == "oui" || rep == "o";
        }

        public static bool DemanderReessayer()
        {
            Console.Write("Voulez-vous réessayer ? (oui/non) : ");
            string choix = Console.ReadLine()?.Trim().ToLower();
            return choix == "oui" || choix == "o";
        }

        // ============================
        //   AFFICHAGE GENERIQUE
        // ============================

        public static void AfficherListe<T>(IEnumerable<T> liste, Func<T, string> format)
        {
            foreach (var item in liste)
                Console.WriteLine(format(item));
        }

        public static void AfficherObjet<T>(T obj, Func<T, string> format)
        {
            Console.WriteLine(format(obj));
        }

        // ============================
        //   SELECTION PAR ID
        // ============================

        public static string DemanderId(string message = "Entrez l'identifiant")
        {
            Console.Write($"{message} : ");
            return Console.ReadLine()?.Trim();
        }
    }
}
