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
    internal class CouleurDAO : AccesDBBase <Couleur>
    {
        public override string NomDeLaTable => "couleur";


        protected override string GetInsertSQL()
        {
            return "INSERT INTO couleur (nom_couleur) VALUES (@nom);";
        }

        protected override string GetDeleteSQL()
        {
            return "DELETE FROM couleur WHERE col_identifiant = @id;";
        }
        protected override string GetUpdateSQL()
        {
            return "UPDATE couleur SET nom_couleur = @nom WHERE col_identifiant = @id;";
        }

        protected override Couleur ConvertirEnObjet(IDataReader reader)
        {
            int id      = GetValueOrDefault<int>(reader, "col_identifiant");
            string nom  = GetStringSafe         (reader, "nom_couleur");

            Couleur receptionDBCouleur = Couleur.Create(id, nom);

            return receptionDBCouleur;


        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Couleur objet)
        {
            cmd.Parameters.AddWithValue("@nom", objet.Nom);
            //Le IF ci-dessous devrait permettre d'injecter dans la DB par erreur l'id zéro lors d'un insert, même si celui-ci ne figure pas dans les paramètres de la requêtes SQL.
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@id", objet.Identifiant);
        }
    }
}
