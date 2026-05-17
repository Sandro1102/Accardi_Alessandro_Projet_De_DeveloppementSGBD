namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Role
    {
        static public Role Create(string nom) { return new Role(nom); }
        public static Role Create(int id, string nom) { return new Role(id, nom); }

        private string _nom;

        private Role(string nom) : this(0, nom) { }
        private Role(int id, string nom)
        {
            this.Nom = nom;
            this.Identifiant = id; 
        }

        public int Identifiant { get; set; }

        public string Nom
        {
            get { return _nom; }
            set
            {
                if (value.Length > 30 || string.IsNullOrWhiteSpace(value))
                    throw new Exception("Le nom du rôle ne peut être vide et/ou dépasser 30 caractères");
                this._nom = value;
            }
        }
    }
}
