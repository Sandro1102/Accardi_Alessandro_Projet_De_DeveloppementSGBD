using System.Data;
using Accardi_Alessandro_Refuge.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal class VaccinationDAO : AccesDBBase<Vaccination>
    {
        public override string NomDeLaTable => "vaccination";

        protected override string GetInsertSQL()
        {
            return $@"
                INSERT INTO {NomDeLaTable} (vaccination_date, vac_animal, id_vaccin)
                VALUES (@vaccination_date, @vac_animal, @id_vaccin)";
        }

        protected override string GetDeleteSQL()
        {
            return $@"
                DELETE FROM {NomDeLaTable}
                WHERE vac_animal       = @vac_animal
                  AND id_vaccin        = @id_vaccin
                  AND vaccination_date = @vaccination_date";
        }

        protected override string GetUpdateSQL()
        {
            return $@"
                UPDATE {NomDeLaTable}
                SET vaccination_date = @vaccination_date
                WHERE vac_animal       = @vac_animal
                  AND id_vaccin        = @id_vaccin
                  AND vaccination_date = @vaccination_date";
        }

        protected override string GetSelectAllSQL()
        {
            return $@"
                SELECT
                    v.vaccination_date   AS vaccination_date,
                    v.vac_animal         AS vac_animal,
                    v.id_vaccin          AS id_vaccin,
                    -- Colonnes ANIMAL (ani_)
                    a.identifiant        AS ani_identifiant, a.nom AS ani_nom, a.type AS ani_type,
                    a.sexe               AS ani_sexe, a.particularites AS ani_particularites, a.date_deces AS ani_date_deces,
                    a.description        AS ani_description, a.date_sterilisation AS ani_date_sterilisation,
                    a.sterilise          AS ani_sterilise, a.date_naissance AS ani_date_naissance,
                    -- Colonnes VACCIN (vc_)
                    vc.identifiant       AS vac_identifiant, vc.nom AS vac_nom
                FROM {NomDeLaTable} v
                JOIN animal a  ON v.vac_animal = a.identifiant
                JOIN vaccin vc ON v.id_vaccin  = vc.identifiant";
        }

        // -------------------------------------------------------
        // Retourne toutes les vaccinations d'un animal
        // -------------------------------------------------------

        public async Task<List<Vaccination>> SelectByAnimalAsync(string idAnimal)
        {
            List<Vaccination> liste = new List<Vaccination>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetSelectAllSQL() + " WHERE v.vac_animal = @id";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@id", idAnimal);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            liste.Add(ConvertirEnObjet(reader));
                    }
                }
            }

            return liste;
        }

        // -------------------------------------------------------
        // Vérifie si un animal a déjà reçu un vaccin ce jour-là
        // -------------------------------------------------------

        public async Task<bool> VaccinationExisteAsync(string idAnimal, int idVaccin, DateTime date)
        {
            bool retVal = false;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $@"
                    SELECT 1 FROM {NomDeLaTable}
                    WHERE vac_animal       = @vac_animal
                      AND id_vaccin        = @id_vaccin
                      AND vaccination_date = @vaccination_date";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@vac_animal", idAnimal);
                    cmd.Parameters.AddWithValue("@id_vaccin", idVaccin);
                    cmd.Parameters.AddWithValue("@vaccination_date", date);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            retVal = true;
                    }
                }
            }

            return retVal;
        }

        // -------------------------------------------------------
        // Supprime une vaccination par animal + vaccin + date
        // -------------------------------------------------------

        public async Task SupprimerVaccinationAsync(string idAnimal, int idVaccin, DateTime date)
        {
            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $@"
                    DELETE FROM {NomDeLaTable}
                    WHERE vac_animal       = @vac_animal
                      AND id_vaccin        = @id_vaccin
                      AND vaccination_date = @vaccination_date";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@vac_animal", idAnimal);
                    cmd.Parameters.AddWithValue("@id_vaccin", idVaccin);
                    cmd.Parameters.AddWithValue("@vaccination_date", date);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // -------------------------------------------------------

        protected override Vaccination ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Vaccin vaccin = ConstruireVaccin(reader);

            DateTime date = GetDateTimeSafe(reader, "vaccination_date") ?? DateTime.MinValue;

            return Vaccination.Create(animal, vaccin, date);
        }

        // -------------------------------------------------------

        private Animal ConstruireAnimal(IDataReader reader)
        {
            Animal animal = Animal.Create(
                GetStringSafe               (reader, "ani_nom"),
                GetStringSafe               (reader, "ani_type"),
                GetStringSafe               (reader, "ani_sexe"),
                GetValueOrDefault<bool>     (reader, "ani_sterilise") ? "oui" : "non",
                GetStringSafe               (reader, "ani_particularites"),
                GetStringSafe               (reader, "ani_description"),
                GetValueOrDefault<DateTime> (reader, "ani_date_naissance"),
                GetDateTimeSafe             (reader, "ani_date_deces"),          
                GetDateTimeSafe             (reader, "ani_date_sterilisation")   
            );

            animal.Identifiant = GetStringSafe(reader, "ani_identifiant");

            return animal;
        }

        // -------------------------------------------------------

        private Vaccin ConstruireVaccin(IDataReader reader)
        {
            int id = GetValueOrDefault<int>(reader, "vac_identifiant");
            string nom = GetStringSafe(reader, "vac_nom");

            return Vaccin.Create(id, nom);
        }

        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Vaccination objet)
        {
            cmd.Parameters.AddWithValue("@vaccination_date",    objet.DateVaccination);
            cmd.Parameters.AddWithValue("@vac_animal",          objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("@id_vaccin",           objet.VaccinApplique.Identifiant);
        }
    }
}