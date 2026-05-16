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
    internal class EntreeDAO : AccesDBBase<Entree>
    {
        public override string NomDeLaTable => "ani_entree";

        protected override string GetInsertSQL()
        {
            return $@"
                INSERT INTO {NomDeLaTable} (raison, date_entree, ani_identifiant, entree_contact)
                VALUES (@raison, @date_entree, @ani_identifiant, @entree_contact)";
        }

        protected override string GetDeleteSQL()
        {
            return $@"
                DELETE FROM {NomDeLaTable}
                WHERE entree_id = @entree_id";
        }

        protected override string GetUpdateSQL()
        {
            return $@"
                UPDATE {NomDeLaTable}
                SET raison         = @raison,
                    date_entree    = @date_entree,
                    ani_identifiant= @ani_identifiant,
                    entree_contact = @entree_contact
                WHERE entree_id = @entree_id";
        }

        protected override string GetSelectAllSQL()
        {
            return $@"
                SELECT
                    en.entree_id    AS en_id,
                    en.raison       AS en_raison,
                    en.date_entree  AS date_entree,
                    -- Colonnes ANIMAL (ani_)
                    a.identifiant   AS ani_identifiant, a.nom AS ani_nom, a.type AS ani_type,
                    a.sexe          AS ani_sexe, a.particularites AS ani_particularites, a.date_deces AS ani_date_deces,
                    a.description   AS ani_description, a.date_sterilisation AS ani_date_sterilisation,
                    a.sterilise     AS ani_sterilise, a.date_naissance AS ani_date_naissance,
                    -- Colonnes CONTACT (con_)
                    c.nom           AS con_nom, c.prenom AS con_prenom,
                    c.rue           AS con_rue, c.cp AS con_cp, c.localite AS con_localite,
                    c.registre_national AS con_registre_national, c.gsm AS con_gsm,
                    c.telephone     AS con_telephone, c.email AS con_email
                FROM {NomDeLaTable} en
                JOIN animal a ON en.ani_identifiant = a.identifiant
                JOIN contact c ON en.entree_contact = c.contact_identifiant";
        }

        protected override Entree ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Contact contact = ConstruireContact(reader);

            int id = GetValueOrDefault<int>(reader, "en_id");
            DateTime date = GetDateTimeSafe(reader, "date_entree") ?? DateTime.MinValue;
            string raison = GetStringSafe(reader, "en_raison");

            return Entree.Create(id, animal, contact, date, raison);
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

        private Contact ConstruireContact(IDataReader reader)
        {
            return Contact.Create(
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
        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Entree objet)
        {
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@entree_id", objet.Identifiant);

            cmd.Parameters.AddWithValue("@raison", objet.Raison);
            cmd.Parameters.AddWithValue("@date_entree", objet.Date);
            cmd.Parameters.AddWithValue("@ani_identifiant", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("@entree_contact", objet.ContactConcerne.Identifiant);
        }
    }
}
