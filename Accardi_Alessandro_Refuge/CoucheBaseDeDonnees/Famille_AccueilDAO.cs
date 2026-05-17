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
    internal class Famille_AccueilDAO : AccesDBBase<Famille_Accueil>
    {
        public override string NomDeLaTable => "famille_accueil";

        protected override string GetInsertSQL()
        {
            return $@"
                INSERT INTO {NomDeLaTable} (date_debut, date_fin, fa_ani_identifiant, fa_contact)
                VALUES (@date_debut, @date_fin, @fa_ani_identifiant, @fa_contact)";
        }

        protected override string GetDeleteSQL()
        {
            return $@"
                DELETE FROM {NomDeLaTable}
                WHERE fa_id = @fa_id";
        }

        protected override string GetUpdateSQL()
        {
            return $@"
                UPDATE {NomDeLaTable}
                SET date_debut         = @date_debut,
                    date_fin           = @date_fin,
                    fa_ani_identifiant = @fa_ani_identifiant,
                    fa_contact         = @fa_contact
                WHERE fa_id = @fa_id";
        }

        protected override string GetSelectAllSQL()
        {
            return $@"
                SELECT
                    fa.fa_id, fa.date_debut, fa.date_fin,
                    -- Colonnes ANIMAL (ani_)
                    a.identifiant    AS ani_identifiant, a.nom AS ani_nom, a.type AS ani_type,
                    a.sexe           AS ani_sexe, a.particularites AS ani_particularites, a.date_deces AS ani_date_deces,
                    a.description    AS ani_description, a.date_sterilisation AS ani_date_sterilisation,
                    a.sterilise      AS ani_sterilise, a.date_naissance AS ani_date_naissance,
                    -- Colonnes CONTACT (con_)
                    c.contact_identifiant AS con_contact_id,
                    c.nom            AS con_nom, c.prenom AS con_prenom,
                    c.rue            AS con_rue, c.cp AS con_cp, c.localite AS con_localite,
                    c.registre_national AS con_registre_national, c.gsm AS con_gsm,
                    c.telephone      AS con_telephone, c.email AS con_email
                FROM {NomDeLaTable} fa
                JOIN animal a  ON fa.fa_ani_identifiant = a.identifiant
                JOIN contact c ON fa.fa_contact         = c.contact_identifiant";
        }

        // -------------------------------------------------------
        // Retourne toutes les FA liées à un animal
        // -------------------------------------------------------

        public async Task<List<Famille_Accueil>> SelectByAnimalAsync(string idAnimal)
        {
            List<Famille_Accueil> liste = new List<Famille_Accueil>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

           
                string sql = GetSelectAllSQL() + " WHERE fa.fa_ani_identifiant = @id";

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
        // Retourne toutes les FA liées à un contact
        // -------------------------------------------------------

        public async Task<List<Famille_Accueil>> SelectByIdContactlAsync(string idContact)
        {
            List<Famille_Accueil> liste = new List<Famille_Accueil>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

            
                string sql = GetSelectAllSQL() + " WHERE fa.fa_contact = @id";

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

        // -------------------------------------------------------
        // Retourne une FA active (sans date de fin) pour un animal
        // -------------------------------------------------------

        public async Task<Famille_Accueil?> SelectFaActiveParAnimalAsync(string idAnimal)
        {
            Famille_Accueil? resultat = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetSelectAllSQL() + " WHERE fa.fa_ani_identifiant = @id AND fa.date_fin IS NULL";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@id", idAnimal);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            resultat = ConvertirEnObjet(reader);
                    }
                }
            }

            return resultat;
        }

        // -------------------------------------------------------

        protected override Famille_Accueil ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Contact contact = ConstruireContact(reader);

            int id = GetValueOrDefault<int>(reader, "fa_id");
            DateTime debut = GetDateTimeSafe(reader, "date_debut") ?? DateTime.MinValue;

        
            DateTime? fin = GetDateTimeSafe(reader, "date_fin");

            return Famille_Accueil.Create(id, animal, contact, debut, fin);
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

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Famille_Accueil objet)
        {
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@fa_id", objet.Identifiant);

            cmd.Parameters.AddWithValue("@date_debut", objet.Date);

            if (objet.DateFin.HasValue)
                cmd.Parameters.AddWithValue("@date_fin", objet.DateFin.Value);
            else
                cmd.Parameters.AddWithValue("@date_fin", DBNull.Value);

            cmd.Parameters.AddWithValue("@fa_ani_identifiant", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("@fa_contact", objet.ContactConcerne.Identifiant);
        }
    }
}