using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Animal
    {
        //                                          Eléments Statique
        static public Animal Create(string nom, string type, string sexe, string sterilise,
                                    string particularite, string description,
                                    DateTime dateDeNaissance, DateTime? dateDeDeces, DateTime? dateDeSterilisation)
        {
            return new Animal(nom, type, sexe, sterilise, particularite, description,
                              dateDeNaissance, dateDeDeces, dateDeSterilisation);
        }

        static public bool EstActuellementAuRefuge (DateTime dateEntreeMax, DateTime? dateSortirMax)
        {
            bool retVal = false;
            if (dateEntreeMax > dateSortirMax || dateSortirMax == null) 
                retVal = true;

            return retVal;
        }

       


        //                                          Eléments d'instance

        //Dans ce constructeur la liste couleur n'est pas présente car une table réalise le lien entre le(s) couleur(s) d'un animal
        //J'ai donc préféré scinder l'enregistrement d'un animal et ses couleurs. Celles-ci seront enregistrées seulement si un animal
        //est réellement inséré dans la table.
        private Animal(string nom, string type, string sexe, string sterilise,
                       string particularite, string description,
                       DateTime dateDeNaissance, DateTime? dateDeDeces, DateTime? dateDeSterilistion)
        {
            this.Nom                 =                  nom;
            this.Type                =                 type;
            this.Sexe                =                 sexe;
            this.Sterilise           =            sterilise;
            this.Particularite       =        particularite;
            this.Description         =          description;

            this.DateDeNaissance     =      dateDeNaissance;
            this.DateDeDeces         =          dateDeDeces;
            this.DateDeSterilisation =   dateDeSterilistion;

            this.VerifierCohérenceSterilisation();
        }

        //Variables d'instance

        private bool        _sterilise;

        private string      _identifiant,
                            _nom,
                            _type,
                            _sexe,
                            _particularite,
                            _description;

        private List<string> _couleurs;

        private DateTime    _dateDeNaissance;
        private DateTime?   _dateDeDeces,
                            _dateDeSterilisation;

        // Méthodes
        public bool EstIdentiqueA(Animal autre)
        {
            bool retVal = false;

            if (autre != null &&
                this.Nom == autre.Nom &&
                this.Type == autre.Type &&
                this.DateDeNaissance == autre.DateDeNaissance &&
                Nullable.Equals(this.DateDeSterilisation, autre.DateDeSterilisation))
            {
                retVal = true;
            }

            return retVal;
        }
        // APRÈS
        private void VerifierCohérenceSterilisation()
        {
            if (!this._sterilise && this._dateDeSterilisation.HasValue)
                throw new ArgumentException("Un animal non stérilisé ne peut pas avoir de date de stérilisation.");

            if (this._sterilise && !this._dateDeSterilisation.HasValue)
                throw new ArgumentException("Un animal stérilisé doit avoir une date de stérilisation.");
        }

        // -------------------- Propriétés --------------------

        public string Identifiant
        {
            get { return this._identifiant; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Identifiant invalide.");

                if (!Regex.IsMatch(value, @"^[0-9]{11}$"))
                    throw new ArgumentException("L'identifiant doit respecter le format yymmdd00000.");

                this._identifiant = value;
            }
        }

        public string Nom
        {
            get { return this._nom; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le nom ne peut pas être vide.");

                if (value.Any(char.IsDigit))
                    throw new ArgumentException("Le nom ne peut pas contenir de chiffres.");


                if (value.Length > 50)
                    throw new ArgumentException("Le nom ne peut pas dépasser 50 caractères.");

                this._nom = value;
            }
        }

        public string Type
        {
            get { return this._type; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le type ne peut pas être vide.");

                if (value.Length > 10)
                    throw new ArgumentException("Le type ne peut pas dépasser 10 caractères.");

                string test = value.ToLower();

                if (test != "chien" && test != "chat")
                    throw new ArgumentException("Le type doit être 'chien' ou 'chat'.");

                this._type = test;
            }
        }

        public string Sexe
        {
            get { return this._sexe; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le sexe ne peut pas être vide.");

                char c = char.ToUpper(value[0]);

                if (c != 'M' && c != 'F')
                    throw new ArgumentException("Le sexe doit être 'M' ou 'F'.");

                this._sexe = c.ToString();
            }
        }

        public string Sterilise
        {
            get { return this._sterilise ? "oui" : "non"; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Valeur invalide pour stérilisé.");

                string test = value.Trim().ToLower();

                if (test == "oui")
                    this._sterilise = true;
                else if (test == "non")
                    this._sterilise = false;
                else
                    throw new ArgumentException("La valeur pour 'stérilisé' doit être 'oui' ou 'non'.");
            }
        }

        public string Particularite
        {
            get { return this._particularite; }
            set
            {
                if (value != null && value.Length > 500)
                    throw new ArgumentException("La particularité est trop longue.");

                this._particularite = value;
            }
        }

        public string Description
        {
            get { return this._description; }
            set
            {
                if (value != null && value.Length > 2000)
                    throw new ArgumentException("La description est trop longue.");

                this._description = value;
            }
        }

        public DateTime DateDeNaissance
        {
            get { return this._dateDeNaissance; }
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("La date de naissance ne peut pas être dans le futur.");

                this._dateDeNaissance = value;
            }
        }

        public DateTime? DateDeDeces
        {
            get { return this._dateDeDeces; }
            set
            {
                if (value.HasValue && value < this.DateDeNaissance)
                    throw new ArgumentException("La date de décès ne peut pas être avant la naissance.");

                this._dateDeDeces = value;
            }
        }

        // APRÈS
        public DateTime? DateDeSterilisation
        {
            get { return this._dateDeSterilisation; }
            set
            {
                if (value.HasValue && value < this.DateDeNaissance)
                    throw new ArgumentException("La date de stérilisation ne peut pas être avant la naissance.");

                if (this.DateDeDeces.HasValue && value > this.DateDeDeces)
                    throw new ArgumentException("La date de stérilisation ne peut pas être après le décès.");

                this._dateDeSterilisation = value;
            }
        }

        public List<string> Couleurs
        {
            get { return this._couleurs; }
            set
            {
                if (value == null)
                    throw new ArgumentException("La liste des couleurs ne peut pas être nulle.");

                if (value.Count == 0)
                    throw new ArgumentException("L'animal doit avoir au moins une couleur.");

                foreach (string c in value)
                {
                    if (string.IsNullOrWhiteSpace(c))
                        throw new ArgumentException("Une couleur ne peut pas être vide.");
                }

                this._couleurs = value;
            }
        }

        
    }
}
