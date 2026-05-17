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
    internal class SortieDAO : AccesDBBase<Sortie>
    {
        public override string NomDeLaTable => "ani_sortie";

        protected override string GetInsertSQL()
        {
            return $@"
                INSERT INTO {NomDeLaTable} (raison, date_sortie, ani_identifiant, sortie_contact)
                VALUES (@raison, @date_sortie, @ani_identifiant, @sortie_contact)";
        }

        protected override string GetDeleteSQL()
        {
            return $@"
                DELETE FROM {NomDeLaTable}
                WHERE sortie_id = @sortie_id";
        }

        protected override string GetUpdateSQL()
        {
            return $@"
                UPDATE {NomDeLaTable}
                SET raison          = @raison,
                    date_sortie     = @date_sortie,
                    ani_identifiant = @ani_identifiant,
                    sortie_contact  = @sortie_contact
                WHERE sortie_id = @sortie_id";
        }

        protected override string GetSelectAllSQL()
        {
            return $@"
                SELECT
                    so.sortie_id    AS so_id,
                    so.raison       AS so_raison,
                    so.date_sortie  AS date_sortie,
                    -- Colonnes ANIMAL (ani_)
                    a.identifiant   AS ani_identifiant, a.nom AS ani_nom, a.type AS ani_type,
                    a.sexe          AS ani_sexe, a.particularites AS ani_particularites, a.date_deces AS ani_date_deces,
                    a.description   AS ani_description, a.date_sterilisation AS ani_date_sterilisation,
                    a.sterilise     AS ani_sterilise, a.date_naissance AS ani_date_naissance,
                    -- Colonnes CONTACT (con_)
                    c.contact_identifiant AS con_contact_id,
                    c.nom           AS con_nom, c.prenom AS con_prenom,
                    c.rue           AS con_rue, c.cp AS con_cp, c.localite AS con_localite,
                    c.registre_national AS con_registre_national, c.gsm AS con_gsm,
                    c.telephone     AS con_telephone, c.email AS con_email
                FROM {NomDeLaTable} so
                JOIN animal a  ON so.ani_identifiant = a.identifiant
                JOIN contact c ON so.sortie_contact  = c.contact_identifiant";
        }

        // -------------------------------------------------------

        private string GetSortieMax()
        {
            return @"
                SELECT date_sortie
                FROM ani_sortie
                WHERE ani_identifiant = @id
                ORDER BY date_sortie DESC
                LIMIT 1";
        }

        public async Task<DateTime?> GetSortieMaxAsync(string identifiant)
        {
            DateTime? retVal = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetSortieMax();

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@id", identifiant);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            retVal = GetDateTimeSafe(reader, "date_sortie");
                    }
                }
            }

            return retVal;
        }

        // -------------------------------------------------------

        protected override Sortie ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Contact contact = ConstruireContact(reader);

            int id = GetValueOrDefault<int>(reader, "so_id");
            DateTime date = GetDateTimeSafe(reader, "date_sortie") ?? DateTime.MinValue;
            string raison = GetStringSafe(reader, "so_raison");

            return Sortie.Create(id, animal, contact, date, raison);
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
                GetDateTimeSafe(reader, "ani_date_deces"),         
                GetDateTimeSafe(reader, "ani_date_sterilisation")   
            );

            animal.Identifiant = GetStringSafe(reader, "ani_identifiant");

            return animal;
        }

        // -------------------------------------------------------

        private Contact ConstruireContact(IDataReader reader)
        {
            Contact contact = Contact.Create(
                GetStringSafe(reader, "con_nom"),
                GetStringSafe(reader, "con_prenom"),
                GetStringSafe(reader, "con_registre_national"),
                GetStringSafe(reader, "con_rue"),
                GetStringSafe(reader, "con_cp"),
                GetStringSafe(reader, "con_localite"),
                GetStringSafe(reader, "con_gsm"),
                GetStringSafe(reader, "con_telephone"),
                GetStringSafe(reader, "con_email")
            );

            contact.Identifiant = GetValueOrDefault<int>(reader, "con_contact_id"); 

            return contact;
        }

        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Sortie objet)
        {
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@sortie_id", objet.Identifiant);

            cmd.Parameters.AddWithValue("@raison", objet.Raison);
            cmd.Parameters.AddWithValue("@date_sortie", objet.Date);
            cmd.Parameters.AddWithValue("@ani_identifiant", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("@sortie_contact", objet.ContactConcerne.Identifiant);
        }
    }
}