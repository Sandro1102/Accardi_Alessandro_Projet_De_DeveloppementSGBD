using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal abstract class AccesDBBase<T>
    {
        //Chaque DAO définit sa table.
        public abstract string NomDeLaTable { get; }

        //SQL d'insertion fourni par chaque DAO
        //Rappel un élément abstract exige que l'héritier définisse l'élément
        protected abstract string GetInsertSQL();

        protected abstract string GetDeleteSQL();

        protected abstract string GetUpdateSQL();

        //Mapping d'une ligne SQL (l'objet métier)
        //Dans cette méthode T sera remplacé par l'objet métier
        //IDataReader est l'objet qui me permet de parcourir les lignes.
        // La méthode Map va permettre de récupérer toute une ligne (un enregistrement) et la transformer en un objet C# (récupération)
        //Quant à IDataReader il s'agit de l'objet qui contiendra le résultat de la requête SQL (la ligne ne cours de lecture)
        protected abstract T ConvertirEnObjet(IDataReader reader);

        //A l'inverse de la méthode ci-dessus la méthode BindParameters permet de transformer un objet C# en valeurs pour les paramètres de la requête SQL (envoi)
        //NpgsqlCommand cmd : C'est la commande SQL dans laquelle on va injecter les valeurs.
        protected abstract void AssignerParametreSQL(NpgsqlCommand cmd, T objet);


        /********************************************************************************************************************************
         *                                      OUTILS DE MAPPING (UTILISABLE PAR LES DAO)
         ********************************************************************************************************************************/

        // Lecture sécurisée (null-safe)
        //Dans cette méthode les mots clefs TValue vont prendre le type de retour sélectionné par le développeur.
        //<TValue> : sera remplacé par le type de retour attendu par la variable de la classe C#
        protected TValue GetValueOrDefault<TValue>(IDataRecord reader, string nomColonne)
        {
            TValue resultat;
            int index = reader.GetOrdinal(nomColonne);

            if (reader.IsDBNull(index))
            {
                resultat = default(TValue);
            }
            else
            {
                object val = reader.GetValue(index);

                // Conversion DateOnly -> DateTime si nécessaire
                if (typeof(TValue) == typeof(DateTime) && val is DateOnly d)
                {
                    resultat = (TValue)(object)d.ToDateTime(TimeOnly.MinValue);
                }
                else
                {
                    resultat = (TValue)val;
                }
            }

            return resultat;
        }

        // Lecture string null-safe
        protected string GetStringSafe(IDataRecord reader, string nomColonne)
        {
            int index = reader.GetOrdinal(nomColonne);
            return reader.IsDBNull(index) ? null : reader.GetString(index);
        }

        // Lecture DateTime? null-safe
        protected DateTime? GetDateTimeSafe(IDataRecord reader, string nomColonne)
        {
            DateTime? resultat;
            int index = reader.GetOrdinal(nomColonne);

            if (reader.IsDBNull(index))
            {
                resultat = null;
            }
            else
            {
                object val = reader.GetValue(index);

                if (val is DateOnly d)
                {
                    resultat = d.ToDateTime(TimeOnly.MinValue);
                }
                else
                {
                    resultat = (DateTime)val;
                }
            }

            return resultat;
        }

        // Lecture bool null-safe
        protected bool? GetBoolSafe(IDataRecord reader, string nomColonne)
        {
            int index = reader.GetOrdinal(nomColonne);
            return reader.IsDBNull(index) ? (bool?)null : reader.GetBoolean(index);
        }


        /*******************************************************************************************************************************
         *                                                  SELECT ALL (ASYNCHRONE)
         ******************************************************************************************************************************/

        //L'utilité de Task pour le système asynchrone et de "promettre" que le retour sera fourni, mais en attendant la lecture de la DB
        //l'application peut faire autre chose et ne pas être bloqué en attendant la fin de la lecture de la DB.
        //Raisons pour lesquelles la méthode ci-dessous contient trois using :
        //              - La connexion doit vivre du début à la fin             (using var connexion = ConnexionDB.GetConnexion()))
        //              - La commande doit vivre juste le temps de l’exécution  (using (var cmd = new NpgsqlCommand(sql, connexion)))
        //              - Le reader doit vivre juste le temps de la lecture     (using (var reader = await cmd.ExecuteReaderAsync()))
        //Cette méthode permet d'obtenir tous les enregistrements qui figurent dans une table.
        public async Task<List<T>> SelectAllAsync()
        {
            List<T> liste = new List<T>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetSelectAllSQL();

                //cmd doit être libéré (buffers internes, ressources non managées)
                //Ressources non managées : quelque chose que le framework .NET ne peut nettoyer seul d'où l'utilité de using.
                using (var cmd = new NpgsqlCommand(sql, connexion))
                //reader doit être fermé(sinon la connexion reste bloquée)
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        liste.Add(ConvertirEnObjet(reader));
                    }
                }
            }
            return liste;
        }

        //Création d'un SELECT modifiable par les classes filles afin de pouvoir récupérer, via jointure, un enregistrement venant d'une autre table.
        // Par défaut, on fait un SELECT * simple. 
        // "virtual" permet aux classes filles de redéfinir cette méthode. Rappel virutal, contrairement à abstract, permet d'être directement appelé et redéfini si nécessire.
        //La propriété NomDeLaTable est définie dans chacune des classes filles.
        protected virtual string GetSelectAllSQL()
        {
            return $"SELECT * FROM {NomDeLaTable}";
        }




        /********************************************************************************************************************************
         *                                                  INSERT (ASYNCHRONE)
         *******************************************************************************************************************************/

        public async Task InsertAsync(T obj)
        {
            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetInsertSQL();

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    AssignerParametreSQL(cmd, obj);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /********************************************************************************************************************************
        *                                                  DELETE (ASYNCHRONE)
        *******************************************************************************************************************************/

        public async Task DeleteAsync(T obj)
        {
            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetDeleteSQL();

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    AssignerParametreSQL(cmd, obj);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /********************************************************************************************************************************
       *                                                  UPDATE (ASYNCHRONE)
       *******************************************************************************************************************************/

        public async Task UpdateAsync (T obj)
        {
            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = GetUpdateSQL();

                using (var cmd = new NpgsqlCommand (sql, connexion))
                {
                    AssignerParametreSQL(cmd, obj);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
