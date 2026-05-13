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
    internal class RoleDAO : AccesDBBase<Role>
    {
        public override string NomDeLaTable => "role";


        protected override string GetInsertSQL()
        {
            return "INSERT INTO role (rol_nom) VALUES (@nom);";
        }

        protected override string GetDeleteSQL()
        {
            return "DELETE FROM role WHERE rol_identifiant = @id;";
        }
        protected override string GetUpdateSQL()
        {
            return "UPDATE role SET rol_nom = @nom WHERE rol_identifiant = @id;";
        }

        protected override Role ConvertirEnObjet(IDataReader reader)
        {
            int id      = GetValueOrDefault<int>(reader, "rol_identifiant");
            string nom  = GetStringSafe         (reader, "rol_nom");

            Role receptionDBRole = Role.Create(id, nom);

            return receptionDBRole;


        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Role objet)
        {
            cmd.Parameters.AddWithValue("@nom", objet.Nom);
            //Le IF ci-dessous devrait permettre d'injecter dans la DB par erreur l'id zéro lors d'un insert, même si celui-ci ne figure pas dans les paramètres de la requêtes SQL.
            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@id", objet.Identifiant);
        }
    }
}
