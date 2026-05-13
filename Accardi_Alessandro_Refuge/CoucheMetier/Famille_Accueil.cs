using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Famille_Accueil : Mouvement
    {
        static public Famille_Accueil Create(Animal animal, Contact contact, DateTime date, DateTime dateFin) 
        { return new Famille_Accueil(0, animal, contact, date, dateFin); }
        static public Famille_Accueil Create(int identifiant, Animal animal, Contact contact, DateTime date, DateTime dateFin) 
        { return new Famille_Accueil (identifiant, animal, contact, date, dateFin); }

        private DateTime _dateFin;
        private Famille_Accueil (int id, Animal animal, Contact contact, DateTime date, DateTime dateFin)
                        : base(id, animal, contact, date)
        {
            this.DateFin = dateFin;
        }

        public DateTime DateFin
        {
            get { return this._dateFin; }
            set
            {
                if (value < base.Date)
                    throw new Exception("La date de fin ne peut être inférieure à la date de début");
                this._dateFin = value;
            }
        }
    }
}
