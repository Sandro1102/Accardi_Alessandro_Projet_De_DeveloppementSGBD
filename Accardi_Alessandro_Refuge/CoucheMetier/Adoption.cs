namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Adoption : Mouvement
    {
        public static bool AdoptionEnCoursOuAcceptee(string statut)
        {
            return statut == "demande" || statut == "acceptee";
        }

        public static void VerifierNouvelleDemandePossible(Adoption? adoptionExistante)
        {
            if (adoptionExistante == null)
                return; // aucune adoption → OK

            if (adoptionExistante.Statut == "demande")
                throw new Exception("Une demande d'adoption est déjà en cours.");

            if (adoptionExistante.Statut == "acceptee")
                throw new Exception("Cet animal a déjà été adopté.");
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
