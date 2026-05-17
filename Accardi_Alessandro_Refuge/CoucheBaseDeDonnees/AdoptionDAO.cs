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
    internal class AdoptionDAO : AccesDBBase<Adoption>
    {
        public override string NomDeLaTable => "adoption";

        protected override string GetInsertSQL()
        {
            return $@"
                INSERT INTO {NomDeLaTable} (statut, date_demande, ani_identifiant, adop_contact)
                VALUES (@statut, @date_demande, @ani_identifiant, @adop_contact)";
        }

        protected override string GetDeleteSQL()
        {
            return $@"
                DELETE FROM {NomDeLaTable}
                WHERE adoption_id = @adoption_id";
        }

        protected override string GetUpdateSQL()
        {
            return $@"
                UPDATE {NomDeLaTable}
                SET statut         = @statut,
                    date_demande   = @date_demande,
                    ani_identifiant= @ani_identifiant,
                    adop_contact   = @adop_contact
                WHERE adoption_id = @adoption_id";
        }

        protected override string GetSelectAllSQL()
        {
            return $@"
                SELECT
                    ad.adoption_id   AS ad_id,
                    ad.statut        AS ad_statut,
                    ad.date_demande  AS date_demande,
                    -- Colonnes ANIMAL (ani_)
                    a.identifiant    AS ani_identifiant, a.nom AS ani_nom, a.type AS ani_type,
                    a.sexe           AS ani_sexe, a.particularites AS ani_particularites, a.date_deces AS ani_date_deces,
                    a.description    AS ani_description, a.date_sterilisation AS ani_date_sterilisation,
                    a.sterilise      AS ani_sterilise, a.date_naissance AS ani_date_naissance,
                    -- Colonnes CONTACT (con_)
                    c.nom            AS con_nom, c.prenom AS con_prenom,
                    c.rue            AS con_rue, c.cp AS con_cp, c.localite AS con_localite,
                    c.registre_national AS con_registre_national, c.gsm AS con_gsm,
                    c.telephone      AS con_telephone, c.email AS con_email
                FROM {NomDeLaTable} ad
                JOIN animal a ON ad.ani_identifiant = a.identifiant
                JOIN contact c ON ad.adop_contact = c.contact_identifiant";
        }

        private string GetStatutAdoption ()
        {
            return @"
                     SELECT *
                     FROM adoption a
                     WHERE a.statut IN ('demande', 'acceptee')
                           AND a.ani_identifiant = @ani_identifiant
                     ORDER BY a.date_demande DESC
                     LIMIT 1;
                        ";
        }

        public async Task<Adoption?> RechercheDemandeAcceptee(string idAnimal)
        {
            Adoption? resultat = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetStatutAdoption();

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@ani_identifiant", idAnimal);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            resultat = ConvertirEnObjet(reader);
                        }
                    }
                }
            }
            return resultat;
        }


        public async Task<List<Adoption>> SelectByAnimalAsync(string idAnimal)
        {
            List<Adoption> liste = new List<Adoption>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $"SELECT * FROM {NomDeLaTable} WHERE ani_identifiant = @id";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@id", idAnimal);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            liste.Add(ConvertirEnObjet(reader));
                        }
                    }
                }
            }

            return liste;
        }

        public Task<Adoption> SelectByIdAsync (string id)
        {
            if (!int.TryParse(id, out int identifiant))
                throw new Exception("L'identifiant doit être un nombre.");

            return SelectByAsync("adoption_id", identifiant);
        }


        protected override Adoption ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Contact contact = ConstruireContact(reader);

            int id = GetValueOrDefault<int>(reader, "ad_id");
            DateTime date = GetDateTimeSafe(reader, "date_demande") ?? DateTime.MinValue;
            string statut = GetStringSafe(reader, "ad_statut");

            return Adoption.Create(id, animal, contact, date, statut);
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
                GetStringSafe(reader, "con_registre_national"),
                GetStringSafe(reader, "con_nom"),
                GetStringSafe(reader, "con_prenom"),
                GetStringSafe(reader, "con_rue"),
                GetStringSafe(reader, "con_cp"),
                GetStringSafe(reader, "con_localite"),
                GetStringSafe(reader, "con_gsm"),
                GetStringSafe(reader, "con_telephone"),
                GetStringSafe(reader, "con_email")
            );
        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Adoption objet)
        {
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@adoption_id", objet.Identifiant);

            cmd.Parameters.AddWithValue("@statut",          objet.Statut);
            cmd.Parameters.AddWithValue("@date_demande",    objet.Date);
            cmd.Parameters.AddWithValue("@ani_identifiant", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("@adop_contact",    objet.ContactConcerne.Identifiant);
        }
    }
}
