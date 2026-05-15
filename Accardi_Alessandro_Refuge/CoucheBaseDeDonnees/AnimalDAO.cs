using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal class AnimalDAO : AccesDBBase<Animal>
    {
        public override string NomDeLaTable => "animal";

        protected override string GetInsertSQL()
        {
            return @"INSERT INTO animal 
                                        (identifiant, nom, type, sexe, 
                                        particularites, description,
                                        date_naissance, date_deces, date_sterilisation,
                                        sterilise)
                     VALUES 
                                        (@identifiant, @nom, @type, @sexe, 
                                        @particularites, @description,
                                        @date_naissance, @date_deces, @date_sterilisation,
                                        @sterilise)";
        }


        protected override string GetDeleteSQL()
        {
            return "DELETE FROM animal WHERE identifiant = @identifiant";
        }

        protected override string GetUpdateSQL()
        {
            return @"UPDATE animal SET
                                    nom                     = @nom,
                                    type                    = @type,
                                    sexe                    = @sexe,
                                    particularites          = @particularites,
                                    description             = @description,
                                    date_naissance          = @date_naissance,
                                    date_deces              = @date_deces,
                                    date_sterilisation      = @date_sterilisation,
                                    sterilise               = @sterilise
                    WHERE identifiant = @identifiant";
        }

        public Task<Animal> SelectByIdAsync(string id)
        {
            return SelectByAsync("identifiant", id);
        }


        protected override Animal ConvertirEnObjet(IDataReader reader)
        {
            //Extraction des données de la DB de manière sécurisée.
            // Pour les strings, si c'est NULL en DB, on reçoit null.
            string nom              = GetStringSafe(reader, "nom");
            string type             = GetStringSafe(reader, "type");
            string sexe             = GetStringSafe(reader, "sexe");

            // Ici, on récupère le booléen de la DB pour savoir s'il est stérilisé.
            // ATTENTION la propriété de la classe se charge de la conversion en bool selon la chaine de caractère reçue
            string sterilise        = GetValueOrDefault<bool>(reader, "sterilise") ? "oui" : "non";

            string particularite    = GetStringSafe(reader, "particularites");
            string description      = GetStringSafe(reader, "description");

            // Gestion des dates. 
            // Le constructeur Animal utilise DateTime.MinValue pour les dates absentes.
            DateTime dateNais       = GetValueOrDefault<DateTime>(reader, "date_naissance");

            // On utilise l'opérateur ?? (null-coalescing) : si GetDateTimeSafe renvoie null, 
            // on prend DateTime.MinValue.
            DateTime dateDeces      = GetDateTimeSafe(reader, "date_deces") ?? DateTime.MinValue;
            DateTime dateSteril     = GetDateTimeSafe(reader, "date_sterilisation") ?? DateTime.MinValue;

            // 3) Création de l'objet via ta méthode Statique Create
            // Les setters de la classe se chargeront des vérifications lié à l'objet.
            Animal receptionDBAnimal = Animal.Create(nom,type,sexe,sterilise,particularite,description,dateNais,dateDeces,dateSteril);

            // Ajout l'identifiant (qui a son propre Setter avec Regex)
            receptionDBAnimal.Identifiant           = GetStringSafe(reader, "identifiant");

            return receptionDBAnimal;
        }



        //Méthode permettant de créer l'identifiant unique.
        private async Task<string> GenererIdentifiantAsync(DateTime dateNaissance)
        {
            string prefix = dateNaissance.ToString("yyMMdd");

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = @"SELECT identifiant 
                       FROM animal 
                       WHERE identifiant LIKE @prefix || '%'
                       ORDER BY identifiant DESC
                       LIMIT 1";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);

                    var result = await cmd.ExecuteScalarAsync();

                    // Aucun identifiant pour cette date → commencer à 00001
                    if (result == null)
                        return prefix + "00001";

                    string lastId = result.ToString();

                    // Extraire les 5 derniers chiffres
                    int lastNumber = int.Parse(lastId.Substring(6, 5));

                    // Incrémenter
                    int newNumber = lastNumber + 1;

                    // Reformater en 5 chiffres
                    return prefix + newNumber.ToString("D5");
                }
            }
        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Animal objet)
        {
            cmd.Parameters.AddWithValue ("@identifiant",         objet.Identifiant);
            cmd.Parameters.AddWithValue ("@nom",                 objet.Nom);
            cmd.Parameters.AddWithValue ("@type",                objet.Type);
            cmd.Parameters.AddWithValue ("@sexe",                objet.Sexe);
            cmd.Parameters.AddWithValue ("@particularites",      objet.Particularite);
            cmd.Parameters.AddWithValue ("@description",         objet.Description);
            cmd.Parameters.AddWithValue ("@sterilise",           objet.Sterilise == "oui"); //le test doit être réaliser, car le Get renvoie un string et non un bool.

            cmd.Parameters.AddWithValue ("@date_deces",          objet.DateDeDeces           == DateTime.MinValue ? (object)DBNull.Value : objet.DateDeDeces);
            cmd.Parameters.AddWithValue ("@date_sterilisation",  objet.DateDeSterilisation   == DateTime.MinValue ? (object)DBNull.Value : objet.DateDeSterilisation);
            cmd.Parameters.AddWithValue ("@date_naissance",      objet.DateDeNaissance);
            
            
        }

        public async Task InsertAsync(Animal obj)
        { 
            // Générer l'identifiant avant l'insertion
            obj.Identifiant = await GenererIdentifiantAsync(obj.DateDeNaissance);

            // Appeler la méthode générique de la classe mère
            await base.InsertAsync(obj);
        }

    }
}
