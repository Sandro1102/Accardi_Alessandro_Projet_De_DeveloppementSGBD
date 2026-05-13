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
    internal class ContactDAO : AccesDBBase<Contact>
    {
        public override string NomDeLaTable => "contact";

        protected override string GetInsertSQL()
        {
            return @"INSERT INTO contact
                                        (nom, prenom, registre_national, rue, cp, localite, gsm, telephone, email)
                                 VALUES
                                        (@nom, @prenom, @registre_national, @rue, @cp, @localite, @gsm, @telephone, @email)";
        }
        protected override string GetDeleteSQL()
        {
            return @"DELETE FROM contact WHERE registre_national = @registre_national";
        }
       
        protected override string GetUpdateSQL()
        {
            return @"UPDATE contact SET
                                        nom                 = @nom,
                                        prenom              = @prenom,
                                        registre_national   = @registre_national,
                                        rue                 = @rue,
                                        cp                  = @cp,
                                        localite            = @localite,
                                        gsm                 = @gsm,
                                        telephone           = @telephone,
                                        email               = @email
                    WHERE registre_national = @registre_national";
        }

        protected override Contact ConvertirEnObjet (IDataReader reader)
        {
            string nom              =                GetStringSafe(reader, "nom");
            string prenom           =             GetStringSafe(reader, "prenom");
            string registreNational =  GetStringSafe(reader, "registre_national");

            string rue              =                GetStringSafe(reader, "rue");
            string cp               =                 GetStringSafe(reader, "cp");
            string localite         =           GetStringSafe(reader, "localite");

            string gsm              =                GetStringSafe(reader, "gsm");
            string telephoneFixe    =          GetStringSafe(reader, "telephone");
            string email            =              GetStringSafe(reader, "email");

            Contact receptionDBContact = Contact.Create (nom, prenom, registreNational, rue, cp, localite, gsm, telephoneFixe, email);

            return receptionDBContact;


        }

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Contact objet)
        {
            cmd.Parameters.AddWithValue ("@nom",                objet.Nom);
            cmd.Parameters.AddWithValue ("@prenom",             objet.Prenom);
            cmd.Parameters.AddWithValue ("@registre_national",  objet.RegistreNational);

            cmd.Parameters.AddWithValue ("@rue",                objet.Rue);
            cmd.Parameters.AddWithValue ("@cp",                 objet.Cp);
            cmd.Parameters.AddWithValue ("@localite",           objet.Localite);

            cmd.Parameters.AddWithValue ("@gsm",                string.IsNullOrWhiteSpace(objet.Gsm)        ? DBNull.Value : objet.Gsm);
            cmd.Parameters.AddWithValue ("@telephone",          string.IsNullOrWhiteSpace(objet.Telephone)  ? DBNull.Value : objet.Telephone);
            cmd.Parameters.AddWithValue ("@email",              string.IsNullOrWhiteSpace(objet.Email)      ? DBNull.Value : objet.Email);
        }
    }
}
