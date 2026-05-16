using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Adoption : Mouvement
    {
        public static bool AdoptionEnCoursOuAcceptee(string statut)
        {
            return statut == "demande" || statut == "acceptee";
        }

        public static void VerifierNouvelleDemandePossible (string statut)
        {
            if (AdoptionEnCoursOuAcceptee(statut))
                if (statut == "demande")
                    throw new Exception("Demande d'adoption en cours impossible d'introduire une nouvelle demande actuellement");
                else
                    throw new Exception("Demande d'adoption déjà acceptee impossible d'introduire une nouvelle demande");

            Console.WriteLine("Pas de demande actuellement en cours vous pouvez introduire une demande d'adoption ou l'acceptee");
        }

        static public Adoption Create(Animal animal, Contact contact, DateTime date, string statut)                  { return new Adoption(0, animal, contact, date, statut); }
        static public Adoption Create(int identifiant, Animal animal, Contact contact, DateTime date, string statut) { return new Adoption(identifiant, animal, contact, date, statut); }

        private string _statut;
        private Adoption( int id, Animal animal, Contact contact, DateTime date, string statut)
                        : base(id, animal, contact, date)
        {
          this.Statut = statut;  
        }

        public string Statut
        { 
            get { return this._statut; } 
            set 
            {
                string[] valeursAdmises = ["demande", "acceptee", "rejet_environnement", "rejet_comportement"];

                if (!valeursAdmises.Contains(value))
                    throw new Exception("la statut entré n'est pas valide");
                this._statut = value; 
            } 
        }

        public bool EstDisponiblePourAdoption(List<Adoption> adoptionsExistantes, Animal checkAnimal)
        {
            bool retVal = true;

            foreach (Adoption a in adoptionsExistantes)
            {
                if (a.Statut == "demande" || a.Statut == "acceptee" || checkAnimal.DateDeDeces != DateTime.MinValue)
                {
                    retVal = false;
                }
            }

            return retVal;
        }
    }
}
