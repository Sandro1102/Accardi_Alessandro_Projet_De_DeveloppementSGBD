using System.Data;
using Accardi_Alessandro_Refuge.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal class CompatibiliteDAO : AccesDBBase<Compatibilite>
    {
        public override string NomDeLaTable => "compatibilite";


        protected override string GetInsertSQL()
        {
            return "INSERT INTO compatibilite (type) VALUES (@type);";
        }

        protected override string GetDeleteSQL()
        {
            return "DELETE FROM compatibilite WHERE identifiant = @identifiant;";
        }
        protected override string GetUpdateSQL()
        {
            return "UPDATE compatibilite SET type = @type WHERE identifiant = @identifiant;";
        }

        protected override Compatibilite ConvertirEnObjet(IDataReader reader)
        {
            int identifiant = GetValueOrDefault<int>     (reader, "identifiant");
            string nom      = GetStringSafe              (reader, "type");

            Compatibilite receptionDBCompatibilite = Compatibilite.Create(identifiant, nom);

            return receptionDBCompatibilite;


        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Compatibilite objet)
        {
            cmd.Parameters.AddWithValue("@type", objet.Type);
            //Le IF ci-dessous devrait permettre d'injecter dans la DB par erreur l'id zéro lors d'un insert, même si celui-ci ne figure pas dans les paramètres de la requêtes SQL.
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@identifiant", objet.Identifiant);
        }
    }
}
