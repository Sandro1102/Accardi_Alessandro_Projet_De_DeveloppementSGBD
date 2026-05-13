using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Compatibilite
    {
        static public Compatibilite Create(string type) { return new Compatibilite(type); }
        public static Compatibilite Create(int identifiant, string type) { return new Compatibilite(identifiant, type); }

        private string _type;

        private Compatibilite(string type) : this(0, type) { }
        private Compatibilite(int identifiant, string type)
        {
            this.Type           = type;
            this.Identifiant    = identifiant;
        }

        public int Identifiant { get; set; }

        public string Type
        {
            get { return _type; }
            set
            {
                if (value.Length > 50 || string.IsNullOrWhiteSpace(value))
                    throw new Exception("Le nom de la compatibilité ne peut être vide et/ou dépasser 50 caractères");
                this._type = value;
            }
        }
    }
}
