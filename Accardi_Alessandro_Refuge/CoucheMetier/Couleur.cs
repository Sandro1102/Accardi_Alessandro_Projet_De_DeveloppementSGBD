using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Couleur
    {
        static public Couleur Create(string nom) { return new Couleur(nom); }
        public static Couleur Create(int id, string nom) { return new Couleur(id, nom); }

        private string _nom;

        private Couleur(string nom) : this(0, nom) { }
        private Couleur(int id, string nom)
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
                if (value.Length > 50 || string.IsNullOrWhiteSpace(value))
                    throw new Exception("Le nom de la couleur ne peut être vide et/ou dépasser 50 caractères");
                this._nom = value;
            }
        }
    }
}
