namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Sortie : Mouvement
    {
        static public Sortie Create(Animal animal, Contact contact, DateTime date, string raison)
        { return new Sortie(0, animal, contact, date, raison); }
        static public Sortie Create(int identifiant, Animal animal, Contact contact, DateTime date, string raison)
        { return new Sortie(identifiant, animal, contact, date, raison); }

        private string _raison;
        private Sortie(int id, Animal animal, Contact contact, DateTime date, string raison)
                        : base(id, animal, contact, date)
        {
            this.Raison = raison;
        }

        public string Raison
        {
            get { return this._raison; }
            set
            {
                string[] valeursAdmises = ["adoption", "retour_proprietaire", "deces_animal", "famille_accueil"];
                if (!valeursAdmises.Contains(value))
                    throw new Exception("la raison entrée n'est pas valide");
                this._raison = value;
            }
        }
    }
}
