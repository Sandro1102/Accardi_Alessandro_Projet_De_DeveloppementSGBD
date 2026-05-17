using System.Data;
using Npgsql;
using static Accardi_Alessandro_Refuge.CoucheBaseDeDonnees.Animal_CouleurDAO;

namespace Accardi_Alessandro_Refuge.CoucheBaseDeDonnees
{
    internal class Animal_CouleurDAO : AccesDBBase<AnimalCouleur>
    {
        public override string NomDeLaTable => "animal_couleur";

        // J'avais créé une classe métier qui n'avais pas de réelle utilité afin l'aléger le code l'IA m'a montré que la ligne ci-dessous remplace
        //la classe que j'avais initialement créée dans la couche métier qui me permettait de lancer le new plus bas dans le code.
        //Cette ligne remplace le code suivant :
        //internal class AnimalCouleur
        //{
        //    public string AniIdentifiant { get; set; }
        //    public int CouleurId { get; set; }
        //}
        //Rappel : un constructeur existe dans les classes même lorsqu'il n'est pas écrit. Exemple : quand j'écris var dao = new VaccinationDAO();
        //Il n'y a aucun constructeur dans cette classe et pourtant il est possible d'écrire la ligne

        internal record AnimalCouleur(string AniIdentifiant, int CouleurId);

        // ---------------------------------------------------------
        // SQL
        // ---------------------------------------------------------

        protected override string GetInsertSQL()
        {
            return $@"
                INSERT INTO {NomDeLaTable} (col_identifiant, ani_identifiant)
                VALUES (@col_identifiant, @ani_identifiant)";
        }

        protected override string GetDeleteSQL()
        {
            return $@"
                DELETE FROM {NomDeLaTable}
                WHERE col_identifiant = @col_identifiant
                  AND ani_identifiant = @ani_identifiant";
        }

        protected override string GetUpdateSQL()
        {
            // Table de lien → pas d'UPDATE logique
            return "SELECT 1";
        }

        // ---------------------------------------------------------
        // Mapping SQL → Objet
        // ---------------------------------------------------------

        protected override AnimalCouleur ConvertirEnObjet(IDataReader reader)
        {
            int couleurId = GetValueOrDefault<int>(reader, "col_identifiant");
            string animalId = GetStringSafe(reader, "ani_identifiant");

            return new AnimalCouleur(animalId, couleurId);
        }

        // ---------------------------------------------------------
        // Mapping Objet → SQL
        // ---------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, AnimalCouleur objet)
        {
            cmd.Parameters.AddWithValue("@col_identifiant", objet.CouleurId);
            cmd.Parameters.AddWithValue("@ani_identifiant", objet.AniIdentifiant);
        }
    }
}
