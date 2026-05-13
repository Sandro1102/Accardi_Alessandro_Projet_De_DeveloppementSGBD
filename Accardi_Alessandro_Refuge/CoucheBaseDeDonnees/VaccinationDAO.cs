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
                WHERE vaccination_date = @vaccination_date
                  AND vac_animal = @vac_animal
                  AND id_vaccin = @id_vaccin";
        }

        protected override string GetUpdateSQL()
        {
            // Pour une table à clé composite (vac_animal, id_vaccin, vaccination_date),
            // l'UPDATE des clés n'est pas recommandé. Si besoin, faire DELETE + INSERT.
            // J'ai écrit l'update, mais il est préférable d'éviter de l'utiliser
            return $@"
                UPDATE {NomDeLaTable}
                SET vaccination_date = @vaccination_date
                WHERE vaccination_date = @vaccination_date
                  AND vac_animal = @vac_animal
                  AND id_vaccin = @id_vaccin";
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
                JOIN animal a ON v.vac_animal = a.identifiant
                JOIN vaccin vc ON v.id_vaccin = vc.identifiant";
        }

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
                GetStringSafe(reader, "ani_nom"),
                GetStringSafe(reader, "ani_type"),
                GetStringSafe(reader, "ani_sexe"),
                GetValueOrDefault<bool>(reader, "ani_sterilise") ? "oui" : "non",
                GetStringSafe(reader, "ani_particularites"),
                GetStringSafe(reader, "ani_description"),
                GetValueOrDefault<DateTime>(reader, "ani_date_naissance"),
                GetDateTimeSafe(reader, "ani_date_deces") ?? DateTime.MinValue,
                GetDateTimeSafe(reader, "ani_date_sterilisation") ?? DateTime.MinValue
            );

            animal.Identifiant = GetStringSafe(reader, "ani_identifiant");
            return animal;
        }

        // -------------------------------------------------------

        private Vaccin ConstruireVaccin(IDataReader reader)
        {
            // Utilise la factory Vaccin.Create(int id, string nom)
            int id = GetValueOrDefault<int>(reader, "vac_identifiant");
            string nom = GetStringSafe(reader, "vac_nom");
            return Vaccin.Create(id, nom);
        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Vaccination objet)
        {
            // clé composite : vaccination_date + vac_animal + id_vaccin
            cmd.Parameters.AddWithValue("@vaccination_date", objet.DateVaccination);
            cmd.Parameters.AddWithValue("@vac_animal", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("@id_vaccin", objet.VaccinApplique.Identifiant);
        }
    }
}
