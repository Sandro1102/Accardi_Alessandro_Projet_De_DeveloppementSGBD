using Npgsql;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal class ConnexionDB
    {
        // La chaîne de connexion à du être adapter pour qu'elle soit fonctionnel avec Npgsql, je n'ai pu la copier telle qu'elle est indiqué dans Postgres.
        //Création d'une variable static public dans laquelle est stoqué la chaîne de connexion
        private static string chaineConnexion = "Host=localhost;Port=5432;Database=refuge_db;Username=postgres;Password=P@ssword;";

        // Cette méthode permet de récupérer une connexion prête à l'emploi
        public static NpgsqlConnection GetConnexion()
        {
            return new NpgsqlConnection(chaineConnexion);
        }
    }
}
