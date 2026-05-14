using System;
using System.Data;
using Npgsql;
using static Accardi_Alessandro_Refuge.CoucheBaseDeDonnees.Animal_CompatibiliteDAO;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal class Animal_CompatibiliteDAO : AccesDBBase<AnimalCompatibilite>
    {
        public override string NomDeLaTable => "ani_compatibilite";

        internal record AnimalCompatibilite(
            bool Valeur,
            string Description,
            int CompatibiliteId,
            string AnimalId
        );

        // ---------------------------------------------------------
        // SQL
        // ---------------------------------------------------------

        protected override string GetInsertSQL()
        {
            return $@"
                INSERT INTO {NomDeLaTable} (valeur, description, comp_identifiant, ani_identifiant)
                VALUES (@valeur, @description, @comp_identifiant, @ani_identifiant)";
        }

        protected override string GetDeleteSQL()
        {
            return $@"
                DELETE FROM {NomDeLaTable}
                WHERE comp_identifiant = @comp_identifiant
                  AND ani_identifiant = @ani_identifiant";
        }

        protected override string GetUpdateSQL()
        {
            // Table de lien → pas d'UPDATE logique
            return "SELECT 1";
        }

        // ---------------------------------------------------------
        // Mapping SQL → Objet
        // ---------------------------------------------------------

        protected override AnimalCompatibilite ConvertirEnObjet(IDataReader reader)
        {
            bool valeur = GetValueOrDefault<bool>(reader, "valeur");
            string description = GetStringSafe(reader, "description");
            int compId = GetValueOrDefault<int>(reader, "comp_identifiant");
            string aniId = GetStringSafe(reader, "ani_identifiant");

            return new AnimalCompatibilite(valeur, description, compId, aniId);
        }

        // ---------------------------------------------------------
        // Mapping Objet → SQL
        // ---------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, AnimalCompatibilite obj)
        {
            cmd.Parameters.AddWithValue("@valeur", obj.Valeur);
            cmd.Parameters.AddWithValue("@description", (object?)obj.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@comp_identifiant", obj.CompatibiliteId);
            cmd.Parameters.AddWithValue("@ani_identifiant", obj.AnimalId);
        }
    }
}
