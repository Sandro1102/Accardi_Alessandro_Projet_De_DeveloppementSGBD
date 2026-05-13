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
    internal class VaccinDAO : AccesDBBase<Vaccin>
    {
        public override string NomDeLaTable => "vaccin";


        protected override string GetInsertSQL()
        {
            return "INSERT INTO vaccin (nom) VALUES (@nom);";
        }

        protected override string GetDeleteSQL()
        {
            return "DELETE FROM vaccin WHERE identifiant = @id;";
        }
        protected override string GetUpdateSQL()
        {
            return "UPDATE vaccin SET nom = @nom WHERE identifiant = @id;";
        }

        protected override Vaccin ConvertirEnObjet (IDataReader reader)
        {
            int id      = GetValueOrDefault<int>    (reader, "identifiant");
            string nom  = GetStringSafe             (reader, "nom");

            Vaccin receptionDBVaccin = Vaccin.Create(id, nom);

            return receptionDBVaccin;


        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Vaccin objet)
        {
            cmd.Parameters.AddWithValue("@nom", objet.Nom);
            //Le IF ci-dessous devrait permettre d'injecter dans la DB par erreur l'id zéro lors d'un insert, même si celui-ci ne figure pas dans les paramètres de la requêtes SQL.
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@id", objet.Identifiant);
        }
    }
}
