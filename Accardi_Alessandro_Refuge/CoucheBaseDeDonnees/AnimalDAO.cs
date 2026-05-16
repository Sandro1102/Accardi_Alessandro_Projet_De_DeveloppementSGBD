using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal class AnimalDAO : AccesDBBase<Animal>
    {
        public override string NomDeLaTable => "animal";

        // -------------------------------------------------------
        // SQL
        // -------------------------------------------------------

        protected override string GetInsertSQL()
        {
            return @"
                INSERT INTO animal 
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
            return @"
                UPDATE animal SET
                    nom                 = @nom,
                    type                = @type,
                    sexe                = @sexe,
                    particularites      = @particularites,
                    description         = @description,
                    date_naissance      = @date_naissance,
                    date_deces          = @date_deces,
                    date_sterilisation  = @date_sterilisation,
                    sterilise           = @sterilise
                WHERE identifiant = @identifiant";
        }

        public string GetSimilaireSQL()
        {
            return @"
                SELECT *
                FROM animal
                WHERE nom = @nom
                  AND type = @type
                  AND date_naissance = @date_naissance
                  AND date_sterilisation = @date_sterilisation;";
        }

        // -------------------------------------------------------
        // Recherche d'un animal identique
        // -------------------------------------------------------

        public async Task<Animal?> ChercherAnimalIdentiqueAsync(
            string nom, string type, DateTime dateNaissance, DateTime? dateSterilisation)
        {
            Animal? retVal = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                using (var cmd = new NpgsqlCommand(GetSimilaireSQL(), connexion))
                {
                    cmd.Parameters.AddWithValue("@nom", nom);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@date_naissance", dateNaissance);

                    var pSteril = cmd.Parameters.Add("@date_sterilisation", NpgsqlTypes.NpgsqlDbType.Date);
                    pSteril.Value = dateSterilisation.HasValue ? dateSterilisation.Value : DBNull.Value;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            retVal = ConvertirEnObjet(reader);
                    }
                }
            }

            return retVal;
        }

        // -------------------------------------------------------
        // Sélection par ID
        // -------------------------------------------------------

        public Task<Animal> SelectByIdAsync(string id)
        {
            return SelectByAsync("identifiant", id);
        }

        // -------------------------------------------------------
        // Conversion DB → Objet métier
        // -------------------------------------------------------

        protected override Animal ConvertirEnObjet(IDataReader reader)
        {
            string nom = GetStringSafe(reader, "nom");
            string type = GetStringSafe(reader, "type");
            string sexe = GetStringSafe(reader, "sexe");
            string sterilise = GetValueOrDefault<bool>(reader, "sterilise") ? "oui" : "non";
            string particularite = GetStringSafe(reader, "particularites");
            string description = GetStringSafe(reader, "description");

            DateTime dateNais = GetValueOrDefault<DateTime>(reader, "date_naissance");
            DateTime? dateDeces = GetDateTimeSafe(reader, "date_deces");
            DateTime? dateSteril = GetDateTimeSafe(reader, "date_sterilisation");

            Animal a = Animal.Create(
                nom, type, sexe, sterilise,
                particularite, description,
                dateNais, dateDeces, dateSteril
            );

            a.Identifiant = GetStringSafe(reader, "identifiant");

            return a;
        }

        // -------------------------------------------------------
        // Génération identifiant
        // -------------------------------------------------------

        private async Task<string> GenererIdentifiantAsync(DateTime dateNaissance)
        {
            string prefix = dateNaissance.ToString("yyMMdd");

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = @"
                    SELECT identifiant 
                    FROM animal 
                    WHERE identifiant LIKE @prefix || '%'
                    ORDER BY identifiant DESC
                    LIMIT 1";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);

                    var result = await cmd.ExecuteScalarAsync();

                    if (result == null)
                        return prefix + "00001";

                    string lastId = result.ToString();
                    int lastNumber = int.Parse(lastId.Substring(6, 5));
                    int newNumber = lastNumber + 1;

                    return prefix + newNumber.ToString("D5");
                }
            }
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Animal objet)
        {
            cmd.Parameters.AddWithValue("@identifiant", objet.Identifiant);
            cmd.Parameters.AddWithValue("@nom", objet.Nom);
            cmd.Parameters.AddWithValue("@type", objet.Type);
            cmd.Parameters.AddWithValue("@sexe", objet.Sexe);
            cmd.Parameters.AddWithValue("@sterilise", objet.Sterilise == "oui");
            cmd.Parameters.AddWithValue("@date_naissance", objet.DateDeNaissance);

            var pPartic = cmd.Parameters.Add("@particularites", NpgsqlTypes.NpgsqlDbType.Varchar);
            pPartic.Value = objet.Particularite != null ? objet.Particularite : DBNull.Value;

            var pDesc = cmd.Parameters.Add("@description", NpgsqlTypes.NpgsqlDbType.Varchar);
            pDesc.Value = objet.Description != null ? objet.Description : DBNull.Value;

            var pDeces = cmd.Parameters.Add("@date_deces", NpgsqlTypes.NpgsqlDbType.Date);
            pDeces.Value = objet.DateDeDeces.HasValue ? objet.DateDeDeces.Value : DBNull.Value;

            var pSteril = cmd.Parameters.Add("@date_sterilisation", NpgsqlTypes.NpgsqlDbType.Date);
            pSteril.Value = objet.DateDeSterilisation.HasValue ? objet.DateDeSterilisation.Value : DBNull.Value;
        }

        // -------------------------------------------------------
        // Insertion avec génération d'identifiant
        // -------------------------------------------------------

        public async Task InsertAsync(Animal obj)
        {
            obj.Identifiant = await GenererIdentifiantAsync(obj.DateDeNaissance);
            await base.InsertAsync(obj);
        }
    }
}
