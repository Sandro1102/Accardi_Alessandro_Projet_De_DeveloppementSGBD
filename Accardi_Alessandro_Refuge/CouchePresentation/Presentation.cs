//Comme pour mes classes liens j'ai préférer utiliser le patern que l'IA m'a montré. Créer une classe qui ne nécessite pas d'écrire new. 
//Je trouve que c'est plus propre lorsque j'appel la méthode menu principal dans la classe program.

//REMARQUE : CE PATERN SERVICE CREEE UN SINGLETON !!!!!!
//-----------------------------------------------
//Rappel la structure de création d'un singleton est : une propriété ou méthode static un constructeur privé un accès global à la classe via une seule instance !
using System;

namespace Accardi_Alessandro_Refuge.CouchePresentation
{
    internal record Presentation
    {
        // Instance unique (pattern service)
        public static Presentation Instance { get; } = new();

        // Constructeur privé : empêche toute instanciation externe sans quoi il serait possible d'écrire dans le programme principale new Presentation () ce que je souhaite éviter
        private Presentation() { }

        // ============================
        //        MENU PRINCIPAL
        // ============================
        public async Task MenuPrincipal()
        {
            int choix = -1;

            do
            {
                Console.Clear();
                Console.WriteLine("===== MENU PRINCIPAL =====");
                Console.WriteLine("1. Gestion des animaux");
                Console.WriteLine("2. Gestion des personnes de contact");
                Console.WriteLine("3. Gestion des adoptions");
                Console.WriteLine("4. Gestion des familles d'accueil");
                Console.WriteLine("5. Gestion des vaccins");
                Console.WriteLine("0. Quitter");
                Console.WriteLine("==========================");
                Console.Write("Votre choix : ");

                int.TryParse(Console.ReadLine(), out choix);

                switch (choix)
                {
                    case 1: await AnimalUI.Instance.MenuAnimaux();break;

                    //case 2: MenuContacts(); break;

                    //case 3: MenuAdoptions(); break;

                    //case 4: MenuFamillesAccueil(); break;

                    //case 5: MenuVaccins(); break;

                    case 0:
                        Console.WriteLine("Fermeture de l'application");
                        break;

                    default:
                        Console.WriteLine("Choix invalide.");
                        Console.ReadKey();
                        break;
                }

            } while (choix != 0);
        }
    }
}
