namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Entree : Mouvement
    {
        static public Entree Create(Animal animal, Contact contact, DateTime date, string raison)
        { return new Entree(0, animal, contact, date, raison); }
        static public Entree Create(int identifiant, Animal animal, Contact contact, DateTime date, string raison)
        { return new Entree(identifiant, animal, contact, date, raison); }

        private string      _raison;
        private Entree(int id, Animal animal, Contact contact, DateTime date, string raison)
                        : base(id, animal, contact, date)
        {
            this.Raison = raison;
        }

        public string Raison
        {
            get { return this._raison; }
            set
            {
                string[] valeursAdmises = ["abandon", "errant", "deces_proprietaire", "saisie", "retour_adoption", "retour_famille_accueil"];

                if (!valeursAdmises.Contains(value))
                    throw new Exception("La raison entrée n'est pas valide.");

                this._raison = value;
            }
        }
    }
}
