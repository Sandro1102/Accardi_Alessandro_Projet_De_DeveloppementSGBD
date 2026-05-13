using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Adoption : Mouvement
    {
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
    }
}
