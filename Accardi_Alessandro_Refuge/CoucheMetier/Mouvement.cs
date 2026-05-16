using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public abstract class Mouvement
    {
        
        
        private Animal      _animalConcerne;
        private Contact     _contactConcerne;
        private DateTime    _date;          //Cette date ne sera jamais la date de fin dans les classes filles, celle-ci sera renseignée dans chacune des classes.
        
        protected Mouvement (int id, Animal animal, Contact contact, DateTime date)
        {
            this.Identifiant        = id;
            this.AnimalConcerne     = animal;
            this.ContactConcerne    = contact;
            this.Date               = date;
        }

        public int Identifiant { get; set; }

 
        public Animal AnimalConcerne
        {
            get
            {
                return this._animalConcerne;
            }
            set
            {
                if (value == null)
                {
                    throw new Exception("L'opération doit être liée à un animal.");
                }
                this._animalConcerne = value;
            }
        }

        public Contact ContactConcerne
        {
            get
            {
                return this._contactConcerne;
            }
            set
            {
                if (value == null)
                {
                    throw new Exception("L'opération doit être liée à un contact (adoptant ou famille).");
                }
                this._contactConcerne = value;
            }
        }

        public DateTime Date
        {
            get
            {
                return this._date;
            }
            set
            {
                // Vérification pour éviter une date trop lointaine dans le futur
                //J'avais fait l'erreur d'indiquer value < DateTime.Now la condition était d'office vrai puisque ce test tenait compte de l'heure !!
                if (value < DateTime.Today.AddMonths(-1) || value > DateTime.Today)
                    throw new Exception("La date d'entrée doit être comprise entre aujourd'hui et un mois dans le passé maximum.");
                this._date = value;
            }
        }

        
    }
}
