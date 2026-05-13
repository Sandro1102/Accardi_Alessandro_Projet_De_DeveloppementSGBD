using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Vaccin
    {
        static public Vaccin Create(string nom)         { return new Vaccin (nom); }
        public static Vaccin Create(int id, string nom) { return new Vaccin(id, nom); }

        private string _nom;

        private Vaccin(string nom) : this(0, nom) { }
        private Vaccin (int id, string nom) 
        { 
            this.Nom = nom;
            this.Identifiant = id;
        }

        public int Identifiant { get; set; } //Nécessaire pour pouvoir modifier facilement le nom d'un vaccin en cas d'erreur lors de l'entrée.

        public string Nom
        { 
            get { return _nom; } 
            set 
            {
                if (value.Length > 50 || string.IsNullOrWhiteSpace(value))
                    throw new Exception("Le nom du vaccin ne peut être vide et/ou dépasser 50 caractères");
                this._nom = value; 
            } 
        }

    }
}
