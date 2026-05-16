using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Accardi_Alessandro_Refuge.CoucheMetier
{
    public class Contact
    {
        //                                                      Elément statique
        static public Contact Create ( string nom, string prenom, string registreNational,
                                string rue, string cp, string localite,
                                string gsm, string telephoneFixe, string email )
        {
            return new Contact (nom, prenom, registreNational, rue, cp, localite, gsm, telephoneFixe, email);
        }

        //                                                      Elément d'instance

        //Variables d'instance

        private string  _nom,
                        _prenom,
                        _registreNational,
                        _rue,
                        _cp,
                        _localite,
                        _gsm,
                        _telephoneFixe,
                        _email;
                        
                
        //Constructeur
        private Contact (   string nom, string prenom, string registreNational,
                            string rue, string cp, string localite,
                            string gsm, string telephoneFixe, string email )
        {
            this.Nom                = nom;
            this.Prenom             = prenom;
            this.RegistreNational   = registreNational;

            this.Rue        = rue;
            this.Localite   = localite;
            this.Cp         = cp;

            this.Gsm        = gsm;
            this.Telephone  = telephoneFixe;
            this.Email      = email;

            VerificationContact();

        }

        //Méthodes
        public static string VerifierRegistreNational(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Le registre national ne peut pas être vide.");

            // Format attendu : 11 chiffres + ponctuation : xx.xx.xx-xxx.xx
            // Exemple : 89.03.25-345.23
            string pattern = @"^\d{2}\.\d{2}\.\d{2}-\d{3}\.\d{2}$";

            if (!Regex.IsMatch(input, pattern))
                throw new ArgumentException("Format invalide. Format attendu : yy.MM.dd-xxx.xx (ex : 89.03.25-345.23)");

            return input.Trim();
        }

        private string ValiderGsmBelge(string input)
        {
            string resultat = string.Empty;

            if (!string.IsNullOrWhiteSpace(input))
            {
                string value = input.Trim();

                bool longueurOK = value.Length == 10;
                bool queDesChiffres = value.All(char.IsDigit);
                bool commencePar04 = value.StartsWith("04");

                if (!(longueurOK && queDesChiffres && commencePar04))
                    throw new ArgumentException("Le GSM doit être au format belge : 0485359516");

                resultat = value;
            }

            return resultat;
        }

        private string ValiderTelephoneFixeBelge(string input)
        {
            string resultat = null;

            if (!string.IsNullOrWhiteSpace(input))
            {
                string value = input.Trim();
                bool longueurOK = value.Length == 9;
                bool queDesChiffres = value.All(char.IsDigit);
                bool commencePar0 = value.StartsWith("0");

                if (longueurOK && queDesChiffres && commencePar0)
                {
                    resultat = value;
                }
                else
                {
                    throw new ArgumentException("Le téléphone fixe doit comporter 9 chiffres et commencer par 0 (ex: 041234567).");
                }
            }

            return resultat;
        }
        private string ValiderEmail(string input)
        {
            string resultat = null;

            if (!string.IsNullOrWhiteSpace(input))
            {
                string value = input.Trim();
                string pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";

                if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                {
                    resultat = value;
                }
                else
                {
                    throw new ArgumentException("L'adresse email ne respecte pas le format requis (ex: contact@domaine.com).");
                }
            }

            return resultat;
        }
        public void VerificationContact()
        {
            bool gsmOK      = !string.IsNullOrWhiteSpace(Gsm);
            bool telOK      = !string.IsNullOrWhiteSpace(Telephone);
            bool emailOK    = !string.IsNullOrWhiteSpace(Email);

            if (!(gsmOK || telOK || emailOK))
                throw new ArgumentException("Au moins un moyen de contact doit être renseigné : GSM, téléphone fixe ou email.");
        }





        //Propriétés

        public int Identifiant {  get; set; }   
        public string Nom 
        { 
            get { return this._nom; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length <= 1)
                    throw new Exception("Le nom saisie doit avoir une longueur de minimum deux caractrès");

                if (value.Any(char.IsDigit))
                    throw new ArgumentException("Le nom ne peut pas contenir de chiffres.");

                if (value.Length > 50)
                    throw new Exception("Le nom saisi est trop long");

                this._nom = value;

            }
        }
        public string Prenom
        {
            get { return this._prenom; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length <= 1)
                    throw new Exception("Le prénom saisie doit avoir une longueur de minimum deux caractrès");

                if (value.Any(char.IsDigit))
                    throw new ArgumentException("Le nom ne peut pas contenir de chiffres.");

                if (value.Length > 50)
                    throw new Exception("Le prénom saisi est trop long");

                this._prenom = value;

            }
        }

        public string RegistreNational
        { 
            get { return this._registreNational; }
            set
            {
                this._registreNational = VerifierRegistreNational(value);
            }
        }
        public string Rue 
        {
            get { return this._rue; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length <= 1)
                    throw new Exception("Nom de rue invalide");

                if (value.Length > 100)
                    throw new Exception("Nom de rue trop long");

                this._rue = value;
            }
        }
        public string Localite
        {
            get { return this._localite; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length <= 1)
                    throw new Exception("Nom de localité invalide");

                if (value.Length > 50)
                    throw new Exception("Nom de localité trop long");

                this._localite = value;
            }
        }
        public string Cp
        {
            get { return this._cp; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length <= 1 || !Regex.IsMatch(value, @"^\d+$"))
                    throw new Exception("Code postale invalide");

                if (value.Length > 10)
                    throw new Exception("Code postale trop long");

                this._cp = value;
            }
        }
        public string Gsm
        {
            get { return this._gsm; }
            set
            {
                this._gsm = ValiderGsmBelge(value);
            }
        }
        public string Telephone
        {
            get
            { return this._telephoneFixe; }
            set
            {
              this._telephoneFixe = ValiderTelephoneFixeBelge(value);  
            }
        }
        public string Email
        {
            get { return this._email; }
            set
            {
                this._email = ValiderEmail(value);
            }
        }
    }
}
