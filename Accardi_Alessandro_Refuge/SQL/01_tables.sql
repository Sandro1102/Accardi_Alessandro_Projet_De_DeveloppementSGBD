-- ============================
-- TABLE CONTACT
-- ============================
CREATE TABLE contact (
    contact_identifiant SERIAL PRIMARY KEY,
    nom VARCHAR(50) NOT NULL CHECK (char_length(nom) >= 2),
    prenom VARCHAR(50) NOT NULL CHECK (char_length(prenom) >= 2),
    rue VARCHAR(100),
    cp VARCHAR(10),
    localite VARCHAR(50),
    registre_national VARCHAR(20) UNIQUE
        CHECK (registre_national ~ '^[0-9]{2}\.[0-9]{2}\.[0-9]{2}-[0-9]{3}\.[0-9]{2}$'),
    gsm VARCHAR(20),
    telephone VARCHAR(20),
    email VARCHAR(100) CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CHECK (gsm IS NOT NULL OR telephone IS NOT NULL OR email IS NOT NULL)
);

-- ============================
-- TABLE ROLE
-- ============================
CREATE TABLE role (
    rol_identifiant SERIAL PRIMARY KEY,
    rol_nom VARCHAR(30) NOT NULL
);

-- ============================
-- TABLE PERSONNE_ROLE
-- ============================
CREATE TABLE personne_role (
    pers_identifiant INT REFERENCES contact(contact_identifiant) ON DELETE CASCADE,
    rol_identifiant INT REFERENCES role(rol_identifiant) ON DELETE CASCADE,
    PRIMARY KEY (pers_identifiant, rol_identifiant)
);

-- ============================
-- TABLE ANIMAL
-- ============================
CREATE TABLE animal (
    identifiant VARCHAR(11) PRIMARY KEY
        CHECK (identifiant ~ '^[0-9]{6}[0-9]{5}$'),

    nom VARCHAR(50),

    type VARCHAR(10) NOT NULL
        CHECK (type IN ('chat', 'chien')),

    sexe CHAR(1) NOT NULL
        CHECK (sexe IN ('M', 'F')),

    particularites TEXT,

    date_deces DATE
        CHECK (date_deces IS NULL OR date_deces >= date_naissance),

    description TEXT,

    date_sterilisation DATE,

    sterilise BOOLEAN NOT NULL,

    date_naissance DATE NOT NULL
        CHECK (date_naissance <= CURRENT_DATE),

    -- J'ai dû corriger cette contrainte car le selectallsync renvoyait une exception lorsque stérilise était vrai et datestérilise null
    CHECK (
        (sterilise = FALSE AND date_sterilisation IS NULL)
        OR
        (sterilise = TRUE AND date_sterilisation IS NOT NULL AND date_sterilisation >= date_naissance)
    )
);

-- ============================
-- TABLE COULEUR
-- ============================
CREATE TABLE couleur (
    col_identifiant SERIAL PRIMARY KEY,
    nom_couleur VARCHAR(50) NOT NULL
);

-- ============================
-- TABLE ANIMAL_COULEUR (N-N)
-- ============================
CREATE TABLE animal_couleur (
    col_identifiant INT REFERENCES couleur(col_identifiant) ON DELETE CASCADE,
    ani_identifiant VARCHAR(11) REFERENCES animal(identifiant) ON DELETE CASCADE,
    PRIMARY KEY (col_identifiant, ani_identifiant)
);

-- ============================
-- TABLE COMPATIBILITE
-- ============================
CREATE TABLE compatibilite (
    identifiant SERIAL PRIMARY KEY,
    type VARCHAR(50) NOT NULL
);

-- ============================
-- TABLE ANI_COMPATIBILITE
-- ============================
CREATE TABLE ani_compatibilite (
    valeur BOOLEAN NOT NULL,
    description TEXT,
    comp_identifiant INT REFERENCES compatibilite(identifiant) ON DELETE CASCADE,
    ani_identifiant VARCHAR(11) REFERENCES animal(identifiant) ON DELETE CASCADE,
    PRIMARY KEY (comp_identifiant, ani_identifiant)
);

-- ============================
-- TABLE ANI_ENTREE
-- ============================
CREATE TABLE ani_entree (
    entree_id SERIAL PRIMARY KEY,
    raison VARCHAR(30) NOT NULL CHECK (
        raison IN ('abandon', 'errant', 'deces_proprietaire', 'saisie', 'retour_adoption', 'retour_famille_accueil')
    ),
    date_entree DATE NOT NULL,
    ani_identifiant VARCHAR(11) REFERENCES animal(identifiant) ON DELETE CASCADE,
    entree_contact INT REFERENCES contact(contact_identifiant)
);

-- ============================
-- TABLE ANI_SORTIE
-- ============================
CREATE TABLE ani_sortie (
    sortie_id SERIAL PRIMARY KEY,
    raison VARCHAR(30) NOT NULL CHECK (
        raison IN ('adoption', 'retour_proprietaire', 'deces_animal', 'famille_accueil')
    ),
    date_sortie DATE NOT NULL,
    ani_identifiant VARCHAR(11) REFERENCES animal(identifiant) ON DELETE CASCADE,
    sortie_contact INT REFERENCES contact(contact_identifiant)
);

-- ============================
-- TABLE ADOPTION
-- ============================
CREATE TABLE adoption (
    adoption_id SERIAL PRIMARY KEY,
    statut VARCHAR(30) NOT NULL CHECK (
        statut IN ('demande', 'acceptee', 'rejet_environnement', 'rejet_comportement')
    ),
    date_demande DATE NOT NULL,
    ani_identifiant VARCHAR(11) REFERENCES animal(identifiant) ON DELETE CASCADE,
    adop_contact INT REFERENCES contact(contact_identifiant)
);

-- ============================
-- TABLE VACCIN
-- ============================
CREATE TABLE vaccin (
    identifiant SERIAL PRIMARY KEY,
    nom VARCHAR(50) NOT NULL
);

-- ============================
-- TABLE VACCINATION
-- ============================
CREATE TABLE vaccination (
    vaccination_date DATE,
    vac_animal VARCHAR(11) REFERENCES animal(identifiant) ON DELETE CASCADE,
    id_vaccin INT REFERENCES vaccin(identifiant) ON DELETE CASCADE,
    PRIMARY KEY (vac_animal, id_vaccin, vaccination_date)
);

-- ============================
-- TABLE FAMILLE_ACCUEIL
-- ============================
CREATE TABLE famille_accueil (
    fa_id SERIAL PRIMARY KEY,
    date_debut DATE NOT NULL,
    date_fin DATE CHECK (date_fin IS NULL OR date_fin >= date_debut),
    fa_ani_identifiant VARCHAR(11) REFERENCES animal(identifiant) ON DELETE CASCADE,
    fa_contact INT REFERENCES contact(contact_identifiant)
);
