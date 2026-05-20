using System.Data;
using Npgsql;
using static Accardi_Alessandro_Refuge.CoucheBaseDeDonnees.Personne_RoleDAO;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal class Personne_RoleDAO : AccesDBBase<PersonneRole>
    {
        public override string NomDeLaTable => "personne_role";

        // Petit record interne pour représenter la relation
        internal record PersonneRole(
            int PersonneId,
            int RoleId
        );

        // ---------------------------------------------------------
        // SQL
        // ---------------------------------------------------------

        protected override string GetInsertSQL()
        {
            return $@"
                INSERT INTO {NomDeLaTable} (pers_identifiant, rol_identifiant)
                VALUES (@pers_identifiant, @rol_identifiant)";
        }

        protected override string GetDeleteSQL()
        {
            return $@"
                DELETE FROM {NomDeLaTable}
                WHERE pers_identifiant = @pers_identifiant
                  AND rol_identifiant = @rol_identifiant";
        }

        protected override string GetUpdateSQL()
        {
            // Table de lien → pas d'UPDATE logique
            return "SELECT 1";
        }

        // ---------------------------------------------------------
        // Mapping SQL → Objet
        // ---------------------------------------------------------

        protected override PersonneRole ConvertirEnObjet(IDataReader reader)
        {
            int persId = GetValueOrDefault<int>(reader, "pers_identifiant");
            int roleId = GetValueOrDefault<int>(reader, "rol_identifiant");

            return new PersonneRole(persId, roleId);
        }

        // ---------------------------------------------------------
        // Mapping Objet → SQL
        // ---------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, PersonneRole obj)
        {
            cmd.Parameters.AddWithValue("@pers_identifiant", obj.PersonneId);
            cmd.Parameters.AddWithValue("@rol_identifiant", obj.RoleId);
        }
        public async Task<List<PersonneRole>> SelectByContactAsync(int idContact)
        {
            List<PersonneRole> liste = new List<PersonneRole>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $"SELECT * FROM {NomDeLaTable} WHERE pers_identifiant = @id";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@id", idContact);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            liste.Add(ConvertirEnObjet(reader));
                    }
                }
            }

            return liste;
        }
    }
}
