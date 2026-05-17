namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Vaccination
    {
        private Animal      _animalConcerne;
        private Vaccin      _vaccinApplique;
        private DateTime    _dateVaccination;

        // Pas d'ID ici, car la clé est le trio (Animal, Vaccin, Date)
        public static Vaccination Create(Animal animal, Vaccin vaccin, DateTime date)
        {
            return new Vaccination(animal, vaccin, date);
        }

        private Vaccination(Animal animal, Vaccin vaccin, DateTime date)
        {
            this.AnimalConcerne     = animal;
            this.VaccinApplique     = vaccin;
            this.DateVaccination    = date;
        }

        public Animal AnimalConcerne
        {
            get { return this._animalConcerne; }
            set
            {
                if (value == null)
                    throw new Exception("Une vaccination doit être liée à un animal.");
                this._animalConcerne = value;
            }
        }

        public Vaccin VaccinApplique
        {
            get { return this._vaccinApplique; }
            set
            {
                if (value == null)
                    throw new Exception("Le type de vaccin est obligatoire.");
                this._vaccinApplique = value;
            }
        }

        public DateTime DateVaccination
        {
            get { return _dateVaccination; }
            set
            {
                if (value > DateTime.Now.AddDays(1))
                    throw new Exception("La date de vaccination saisie est invalide maximum le jour courant.");
                this._dateVaccination = value;
            }
        }
    }
}
