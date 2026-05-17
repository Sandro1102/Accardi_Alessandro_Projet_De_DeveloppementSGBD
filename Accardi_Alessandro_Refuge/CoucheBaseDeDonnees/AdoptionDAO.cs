using System.Data;
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
                SET statut          = @statut,
                    date_demande    = @date_demande,
                    ani_identifiant = @ani_identifiant,
                    adop_contact    = @adop_contact
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
                    c.contact_identifiant AS con_contact_id,
                    c.nom            AS con_nom, c.prenom AS con_prenom,
                    c.rue            AS con_rue, c.cp AS con_cp, c.localite AS con_localite,
                    c.registre_national AS con_registre_national, c.gsm AS con_gsm,
                    c.telephone      AS con_telephone, c.email AS con_email
                FROM {NomDeLaTable} ad
                JOIN animal a  ON ad.ani_identifiant = a.identifiant
                JOIN contact c ON ad.adop_contact    = c.contact_identifiant";
        }

        // -------------------------------------------------------
        // Requête pour chercher une demande en cours sur un animal
        // -------------------------------------------------------

        private string GetStatutAdoptionSQL()
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
                JOIN animal a  ON ad.ani_identifiant = a.identifiant
                JOIN contact c ON ad.adop_contact    = c.contact_identifiant
                WHERE ad.statut IN ('demande', 'acceptee')
                  AND ad.ani_identifiant = @ani_identifiant
                ORDER BY ad.date_demande DESC
                LIMIT 1";
        }

        // -------------------------------------------------------
        // Recherche la dernière demande en cours pour un animal
        // -------------------------------------------------------

        public async Task<Adoption?> RechercheDemandeAcceptee(string idAnimal)
        {
            Adoption? resultat = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetStatutAdoptionSQL();

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@ani_identifiant", idAnimal);

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
        // Retourne toutes les adoptions liées à un animal
        // -------------------------------------------------------

        public async Task<List<Adoption>> SelectByAnimalAsync(string idAnimal)
        {
            List<Adoption> liste = new List<Adoption>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();


                string sql = GetSelectAllSQL() + " WHERE ad.ani_identifiant = @id";

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
        // Retourne une adoption par son id
        // -------------------------------------------------------

        public Task<Adoption> SelectByIdAsync(string id)
        {
            if (!int.TryParse(id, out int identifiant))
                throw new Exception("L'identifiant doit être un nombre.");


            return SelectByIdAvecJoinsAsync(identifiant);
        }


        private async Task<Adoption> SelectByIdAvecJoinsAsync(int identifiant)
        {
            Adoption? resultat = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetSelectAllSQL() + " WHERE ad.adoption_id = @adoption_id";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.Parameters.AddWithValue("@adoption_id", identifiant);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            resultat = ConvertirEnObjet(reader);
                    }
                }
            }

            return resultat ?? throw new Exception("Adoption introuvable.");
        }

        // -------------------------------------------------------

        protected override Adoption ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Contact contact = ConstruireContact(reader);

            int id = GetValueOrDefault<int>(reader, "ad_id");
            DateTime date = GetDateTimeSafe(reader, "date_demande") ?? DateTime.MinValue; //une fois les tests terminé si possible modifier cette ligne afin de ne plus passer minvalue, mais
            string statut = GetStringSafe(reader, "ad_statut");                           //il faudra tout revérifier

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
                GetDateTimeSafe(reader, "ani_date_deces"),           //l'affichage de la table ne fonctionnait pas car suite à la modification de minvalue pour les dates vers la possibilité
                GetDateTimeSafe(reader, "ani_date_sterilisation")    //d'avoir des valeurs null minvalue dans le test indiquait un animal décédé ......
            );

            animal.Identifiant = GetStringSafe(reader, "ani_identifiant");

            return animal;
        }

        // -------------------------------------------------------

        private Contact ConstruireContact(IDataReader reader)
        {
            Contact contact = Contact.Create(
                GetStringSafe(reader, "con_nom"),                // J'ai rencontré une erreur que j'ai fréquemment produite dans le code. N'ayant pas respecté l'ordre des paramètres
                GetStringSafe(reader, "con_prenom"),             // lors de l'appel registre national était envoyé à la propriété nom. Le nom ne pouvant contenir de chiffre il y avait
                GetStringSafe(reader, "con_registre_national"),  // une levée d'exception ....
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

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Adoption objet)
        {
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@adoption_id", objet.Identifiant);

            cmd.Parameters.AddWithValue("@statut", objet.Statut);
            cmd.Parameters.AddWithValue("@date_demande", objet.Date);
            cmd.Parameters.AddWithValue("@ani_identifiant", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("@adop_contact", objet.ContactConcerne.Identifiant);
        }
    }
}