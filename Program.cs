using MySql.Data.MySqlClient;
using static System.Console;
using static System.ConsoleColor;

using static MDD_FlowerStore.ConsoleVisuals;

namespace MDD_FlowerStore;

public static class Program
{
    #region Champs
    public static MySqlConnection? connection;
    public static string idStaff = "BelleFleur";
    public static string mdpStaff = "staff";
    public static Move execution = Move.MainMenu;
    public static Profil user = Profil.NonDefini;
    public static string identifiant = "";

    private static string BDDname = "mdd";
    private static string BDDmdp = "mdd";
    private static int positionDefaut = 0;
    #endregion

    #region Enum
    public enum Profil
    {
        Staff,
        Client,
        NonDefini
    }
    public enum Move
    {
        Continue,
        MainMenu,
        Commande,
        Actions,
        Options,
        Couleur,
        Back,
        Exit
    }
    #endregion

    #region Méthodes
    public static void Main(string[] args)
    {
        
        WriteFullScreen(true);

        #region MainMenu
        MainMenu:

        MainMenu();
        switch(execution)
        {
            case Move.MainMenu:
                goto MainMenu;
            case Move.Options:
                goto Options;
            case Move.Actions:
                goto Actions;
            case Move.Exit:
                goto Exit;
            default:
                break;
        }
        #endregion

        #region Client

        Commande:

        #region Séléction Boutique
        int commandePrix = 0;
        string boutique;
        MySqlDataReader reader = Query("SELECT nom FROM Boutique;");
        List<string> boutiques = new List<string>();
        while (reader.Read())
            boutiques.Add(reader.GetString(0));
        reader.Close();
        int numBoutique = ScrollingMenu("Veuillez choisir la boutique de livraison", boutiques.ToArray());
        if (numBoutique == -1)
            goto Commande;
        boutique = boutiques[numBoutique];
        numBoutique++;
        #endregion

        Delai:

        #region Séléction Date livraison
        DateTime dateLivraison = Convert.ToDateTime(WritePrompt("Veuillez saisir le délai de livraison souhaité : "));
        if (dateLivraison < DateTime.Now)
        {
            WriteParagraph(new string[]{ "La date de livraison ne peut pas être antérieure à la date actuelle."}, true);
            ReadKey(true);
            goto Delai;
        }
        #endregion

        Bouquets:

        #region Séléction Bouquet
        List<string> bouquetsNoms = new List<string>();
        reader = Query("SELECT nom FROM Bouquet");
        while (reader.Read())
            bouquetsNoms.Add(reader.GetString(0));
        reader.Close();
        bouquetsNoms.Add("Aucun");
        List<int> bouquetsPrix = new List<int>();
        reader = Query("SELECT prix FROM Bouquet");
        while (reader.Read())
            bouquetsPrix.Add(reader.GetInt32(0));
        reader.Close();
        switch(ScrollingMenu("Veuillez choisir le bouquet à commander", bouquetsNoms.ToArray()))
        { 
            case 0:
                commandePrix += bouquetsPrix[0];
                break;
            case 1:
                commandePrix += bouquetsPrix[1];
                break;
            case 2:
                commandePrix += bouquetsPrix[2];
                break;
            case 3:
                commandePrix += bouquetsPrix[3];
                break;
            case 4:
                commandePrix += bouquetsPrix[4];
                break;
            case -1:
                goto Delai;
            default:
                break;
        }
        #endregion

        Fleurs:

        #region Séléction Fleurs
        List<string> fleursNoms = new List<string>();
        reader = Query($"SELECT nom FROM Fleur WHERE id_Boutique = {numBoutique};");
        while (reader.Read())
            fleursNoms.Add(reader.GetString(0));
        reader.Close();
        fleursNoms.Add("Aucune");
        List<int> fleursPrix = new List<int>();
        reader = Query($"SELECT prix FROM Fleur WHERE id_Boutique = {numBoutique};");
        while (reader.Read())
            fleursPrix.Add(reader.GetInt32(0));
        reader.Close();
        int numFleur = ScrollingMenu("Veuillez choisir la fleur à commander", fleursNoms.ToArray());
        if (numFleur == -1)
            goto Bouquets;
        else if (numFleur != 8)
        {
            int quantite = Convert.ToInt32(WritePrompt("Veuillez saisir la quantité de fleurs à commander : "));
            commandePrix += fleursPrix[numFleur] * quantite;
        } 
        #endregion

        #region Séléction Accessoires
        List<string> accessoiresNoms = new List<string>();
        reader = Query($"SELECT nom FROM Accessoire WHERE id_Boutique = {numBoutique};");
        while (reader.Read())
            accessoiresNoms.Add(reader.GetString(0));
        reader.Close();
        accessoiresNoms.Add("Aucun");
        List<int> accessoiresPrix = new List<int>();
        reader = Query($"SELECT prix FROM Accessoire WHERE id_Boutique = {numBoutique};");
        while (reader.Read())
            accessoiresPrix.Add(reader.GetInt32(0));
        reader.Close();
        int numAccessoire = ScrollingMenu("Veuillez choisir l'accessoire à commander", accessoiresNoms.ToArray());
        if (numAccessoire is -1)
            goto Fleurs;
        else if (numAccessoire != 3)
        {
            int quantite = Convert.ToInt32(WritePrompt("Veuillez saisir la quantité d'accessoires à commander : "));
            commandePrix += accessoiresPrix[numAccessoire] * quantite;
        }
        #endregion

        #region finalisation
        if (commandePrix > 0)
        {
            string message = WritePrompt("Veuillez saisir un message à joindre au bouquet : ");
            reader = Query($"SELECT id FROM Client WHERE email = '{identifiant}';");
            reader.Read();
            int ident = reader.GetInt32(0);
            reader.Close();
            reader = Query($"SELECT Count(*) FROM Commande WHERE id_Client = {ident};");
            reader.Read();
            int moyenneCommande = reader.GetInt32(0) / 12;
            if (moyenneCommande >= 5)
                commandePrix  = (int)(commandePrix * 0.85);
            else if (moyenneCommande >= 1)
                commandePrix  = (int)(commandePrix * 0.95);
            reader.Close();
            if (dateLivraison < DateTime.Now.AddDays(3))
            {
                reader = Query($"INSERT INTO Commande (statut, date_creation, date_livraison, message, prix, id_Boutique, id_Client) VALUES ('VINV', '{DateTime.Now.ToString("dd-MM-yyyy")}', '{dateLivraison.ToString("dd-MM-yyyy")}', '{message}', {commandePrix}, {numBoutique}, {ident});");
                reader.Close();
                WriteParagraph(new string[]{ $" Commande en cours de vérification. "," Les délais étant inférieurs à 3 jours, ","un agent doit vérifier les stocks",$" Total de la commande : {commandePrix} €. "}, true);
                ReadKey(true);
            
            }
            else
                reader = Query($"INSERT INTO Commande (statut, date_creation, date_livraison, message, prix, id_Boutique, id_Client) VALUES ('CC', '{DateTime.Now.ToString("dd-MM-yyyy")}', '{dateLivraison.ToString("dd-MM-yyyy")}', '{message}', {commandePrix}, {numBoutique}, {ident});");
            reader.Close();
            WriteParagraph(new string[]{ $" Commande effectuée avec succès !", $" Total de la commande : {commandePrix} €. "}, true);
            ReadKey(true);
        }
        else 
        {
            WriteParagraph(new string[]{ "Aucune transaction n'a pu être effectuée."}, true);
            ReadKey(true);
        }
        goto MainMenu;
        #endregion

        #endregion

        #region Staff

        Actions:

        switch(ScrollingMenu("Veuillez choisir une action", new string[] { "Statistiques", "État stocks","État commandes", "Retour" }))
        {
            case 0:
                goto Statistiques;
            case 1:
                goto Stocks;
            case 2:
                goto GestionCommandes;
            default:
                goto MainMenu;
        }

        Statistiques:

        #region Statistiques
        ClearContent();
        int pos = ScrollingMenu("Veuillez choisir quelle statistique consulter.", new string[] { "Prix moyen par commande", "Meilleur client", "Magasin le plus fréquenté", "Magasin le plus rentable", "Fleur la moins populaire", "Fleur la plus populaire", "Retour"}, Placement.Center, 9, false, 0, positionDefaut);
        switch(pos)
        {
            case 0:
                reader = Query("SELECT AVG(prix) FROM Commande;");
                reader.Read();
                WriteParagraph(new string[] { $" Prix moyen par commande : {reader.GetInt32(0)} €." }, true, CursorTop + 2);
                reader.Close();
                ReadKey(true);
                break;
            case 1:
                reader = Query("SELECT nom, prenom, email FROM Client WHERE id = (SELECT id_Client FROM Commande GROUP BY id_Client ORDER BY SUM(prix) DESC LIMIT 1);");
                reader.Read();
                WriteParagraph(new string[] { $" Le meilleur client de nos boutiques est {reader.GetString(0)} {reader.GetString(1)} ({reader.GetString(2)})" }, true, CursorTop + 2);
                reader.Close();
                ReadKey(true);
                break;
            case 2:
                reader = Query("SELECT nom FROM Boutique WHERE id = (SELECT id_Boutique FROM Commande GROUP BY id_Boutique ORDER BY COUNT(*) DESC LIMIT 1);");
                reader.Read();
                WriteParagraph(new string[] { $" Le magasin le plus fréquenté est {reader.GetString(0)}" }, true, CursorTop + 2);
                reader.Close();
                ReadKey(true);
                break;
            case 3:
                reader = Query("SELECT nom FROM Boutique WHERE id = (SELECT id_Boutique FROM Commande GROUP BY id_Boutique ORDER BY SUM(prix) DESC LIMIT 1);");
                reader.Read();
                WriteParagraph(new string[] { $" Le magasin le plus rentable est {reader.GetString(0)}" }, true, CursorTop + 2);
                reader.Close();
                ReadKey(true);
                break;
            case 4:
                reader = Query("SELECT nom FROM Fleur WHERE id = (SELECT id FROM Fleur GROUP BY id ORDER BY SUM(disponibilite) DESC LIMIT 1);");
                reader.Read();
                WriteParagraph(new string[] { $" La fleur la plus populaire est {reader.GetString(0)}" }, true, CursorTop + 2);
                reader.Close();
                ReadKey(true);
                break;
            case 5:
                reader = Query("SELECT nom FROM Fleur WHERE id = (SELECT id FROM Fleur GROUP BY id ORDER BY SUM(disponibilite) ASC LIMIT 1);");
                reader.Read();
                WriteParagraph(new string[] { $" La fleur la moins populaire est {reader.GetString(0)}" }, true, CursorTop + 2);
                reader.Close();
                ReadKey(true);
                break;
            default:
                positionDefaut = 0;
                goto Actions;
        }
        positionDefaut = pos;
        goto Statistiques;
        #endregion

        Stocks:

        #region Choisir Boutique
        string boutiqueChoisie;
        reader = Query("SELECT nom FROM Boutique;");
        List<string> choixBoutique = new List<string>();
        while (reader.Read())
            choixBoutique.Add(reader.GetString(0));
        reader.Close();
        int idBoutique = ScrollingMenu("Veuillez choisir la boutique dont vous souhaiter consulter le stock.", choixBoutique.ToArray());
        if (idBoutique == -1)
            goto Actions;
        boutiqueChoisie = choixBoutique[idBoutique];
        idBoutique++;
        #endregion

        #region Afficher Stocks
        reader = Query($"SELECT nom, disponibilite FROM Fleur WHERE id_Boutique = {idBoutique};");
        SetCursorPosition(0, 9);
        WriteParagraph(new string[] { $" Stocks de fleurs de la boutique {boutiqueChoisie} : " }, true, CursorTop, Placement.Left);
        WriteLine("\n");
        for (int i = 0; i < reader.FieldCount; i++)
        {
            Console.Write ($"{reader.GetName(i),-20}");
        }
        Console.WriteLine();
        while (reader.Read())
        {
            ForegroundColor = ConsoleColor.Green;
            string str = "";
            str += $"{reader.GetString(0),-20}";
            str += $"{reader.GetInt32(1),-20}";
            if(reader.GetInt32(1) < 7)
                ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
        }
        ForegroundColor = ConsoleColor.White;
        reader.Close();
        Console.WriteLine();
        WriteParagraph(new string[]{ $" Stocks d'accessoires : "}, true, CursorTop, Placement.Left);
        WriteLine("\n");

        reader = Query($"SELECT nom, disponibilite FROM Accessoire WHERE id_Boutique = {idBoutique};");
        for (int i = 0; i < reader.FieldCount; i++)
        {
            Console.Write ($"{reader.GetName(i),-20}");
        }
        Console.WriteLine();
        while (reader.Read())
        {
            ForegroundColor = ConsoleColor.Green;
            string str = "";
            str += $"{reader.GetString(0),-20}";
            str += $"{reader.GetInt32(1),-20}";
            if(reader.GetInt32(1) < 7)
                ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
        }
        ForegroundColor = ConsoleColor.White;
        reader.Close();
        WriteLine("\n");
        WriteParagraph(new string[]{ " Appuyez sur une touche pour continuer. "}, true, CursorTop, Placement.Left);
        ReadKey(true);
        ClearContent();
        goto Stocks;
        #endregion
        
        GestionCommandes:

        #region Afficher Table
        reader = Query($"SELECT id, statut, date_creation, date_livraison, message, prix FROM Commande;");
        List<string> commandes = new List<string>();
        string header = "";
        for (int i = 0; i < reader.FieldCount; i++)
        {
            header += $"{reader.GetName(i),-20}";
        }
        while (reader.Read())
        {
            string str = "";
            for (int i = 0; i < reader.FieldCount; i++)
            {
                str += $" {reader.GetValue(i),-18} ";
            }
            commandes.Add(str);
        }
        reader.Close();
        #endregion
        
        #region Séléctionner Commande
        WriteBanner((" Projet BDD", "Veuillez sélectionner la commande", "Réalisé par Yann et Sherylann "), true, true);
        ClearContent();
        int numCommande = ScrollingMenu(header, commandes.ToArray(), Placement.Center);
        WriteBanner((" Projet BDD", "Exécution...", "Réalisé par Yann et Sherylann "), true, true);
        if (numCommande == -1)
            goto Actions;
        numCommande++;
        #endregion

        ChangementChamp:

        #region Afficher Commande
        ClearContent();
        Console.SetCursorPosition(0, 9);
        reader = Query($"SELECT statut, date_creation, date_livraison, message, prix FROM Commande WHERE id = {numCommande} ;");
        string header2 = "";
        for (int i = 0; i < reader.FieldCount; i++)
        {
            header2 += $"{reader.GetName(i),-20}";
        }
        Console.WriteLine(header2.BuildString(WindowWidth, Placement.Center));
        Console.WriteLine("");
        while (reader.Read())
        {
            string str = "";
            for (int i = 0; i < reader.FieldCount; i++)
            {
                str += $" {reader.GetValue(i),-18} ";
            }
            Console.WriteLine(str.BuildString(WindowWidth, Placement.Center));
        }
        reader.Close();
        int posTemp = CursorTop ;
        #endregion

        #region Changer Champ
        switch(ScrollingMenu("Veuillez choisir le champ à modifier", new string[] { "Statut","Date de création", "Date de livraison","Message", "Prix", "Retour" }, Placement.Center, posTemp))
        {
            case 0:
                string statut = ScrollingMenuString("Veuillez choisir le nouveau statut", new string[] { "CC", "CL", "VINV", "CPAV", "CAL", "Retour" }, Placement.Center, posTemp);
                if (statut == "Retour")
                    goto GestionCommandes;
                reader = Query($"UPDATE Commande SET statut = '{statut}' WHERE id = {numCommande};");
                reader.Close();
                break;
            case 1:
                DateTime date_Creation = Convert.ToDateTime(WritePrompt("Veuillez entrer la nouvelle date de création (jj/mm/aaaa)", posTemp));
                reader = Query($"UPDATE Commande SET date_creation = '{date_Creation.ToString("dd-MM-yyyy")}' WHERE id = {numCommande};");
                reader.Close();
                break;
            case 2:
                DateTime date_Livraison = Convert.ToDateTime(WritePrompt("Veuillez entrer la nouvelle date de livraison (jj/mm/aaaa)", posTemp));
                reader = Query($"UPDATE Commande SET date_livraison = '{date_Livraison.ToString("dd-MM-yyyy")}' WHERE id = {numCommande};");
                reader.Close();
                break;
            case 3:
                string message = WritePrompt("Veuillez entrer le nouveau message", posTemp);
                reader = Query($"UPDATE Commande SET message = '{message}' WHERE id = {numCommande};");
                reader.Close();
                break;
            case 4:
                int prix = int.Parse(WritePrompt("Veuillez entrer le nouveau prix", posTemp));
                reader = Query($"UPDATE Commande SET prix = {prix} WHERE id = {numCommande};");
                reader.Close();
                break;
            default:
                goto GestionCommandes;
        }
        WriteParagraph(new string[] { " Changement effectué avec succès ! " }, true, posTemp);
            ReadKey(true);
        goto ChangementChamp;
        #endregion
        
        #endregion

        #region Utility
        Exit:
        if (connection is not null)
            connection.Close();
        ProgramExit();

        Options:
        Options();
        switch(execution)
        {
            case Move.MainMenu:
                goto MainMenu;
            case Move.Couleur:
                goto Couleur;
            case Move.Options:
                goto Options;
            default:
                break;
        }

        Couleur:
        ChangeColor();
        goto Options;
        #endregion
    }
    public static void MainMenu()
    {
        ClearContent();
        if (user is Profil.NonDefini)
            switch (ScrollingMenu("Bienvenue dans les boutiques de M.BelleFleur, veuillez vous identifier.", new string[]{"Authentifier", "Options", "Quitter"}))
            {
                case 0:
                    user = Authentification();
                    if (user is Profil.NonDefini)
                        execution = Move.MainMenu;
                    else 
                        execution = Move.MainMenu;
                    break;
                case 1:
                    execution = Move.Options;
                    break;
                case 2:
                    execution = Move.Exit;
                    break;
                default:
                    break;
            }
        else if (user is Profil.Client)
        {
            MySqlDataReader reader = Query($"SELECT prenom FROM Client WHERE email = '{identifiant}';");
            reader.Read();
            string name = reader.GetString(0);
            reader.Close();
            switch (ScrollingMenu($"Bienvenue dans votre espace personnel {name}.", new string[]{"Commande", "Options", "Déconnexion"}))
            {
                case 0:
                    execution = Move.Commande;
                    break;
                case 1:
                    execution = Move.Options;
                    break;
                case 2:
                    identifiant = "";
                    user = Profil.NonDefini;
                    execution = Move.MainMenu;
                    break;
                default:
                    break;
            }
        }
        else if (user is Profil.Staff)
            switch (ScrollingMenu("Bienvenue dans l'espace administrateur.", new string[]{"Gestion", "Options", "Déconnexion"}))
            {
                case 0:
                    execution = Move.Actions;
                    break;
                case 1:
                    execution = Move.Options;
                    break;
                case 2:
                    identifiant = "";
                    user = Profil.NonDefini;
                    execution = Move.MainMenu;
                    break;
                default:
                    break;
            }
    }
    public static void Options()
    {
        ClearContent();
        switch(user)
        {
            case Profil.Client:
                switch(ScrollingMenu("Veuillez sélectionner une option", new string[]{
                    "Changer de couleur", 
                    "Changer mot de passe",
                    "Hisorique des commandes",
                    "Retour"}))
                {
                    case 0:
                        execution = Move.Couleur;
                        break;
                    case 1:
                        string mdp = WritePrompt("Veuillez saisir le nouveau mot de passe : ");
                        string query = $"UPDATE Client SET mdp = '{mdp}' WHERE email = '{identifiant}';";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.ExecuteNonQuery();
                        execution = Move.MainMenu;
                        break;
                    case 2:
                        Console.SetCursorPosition(0, 9);
                        Console.WriteLine("Historique des commandes : \n");
                        string query2 = $"SELECT id, date_creation, message, prix FROM Commande WHERE id_Client = (SELECT id FROM Client WHERE email = '{identifiant}');";
                        MySqlDataReader reader = Query(query2);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader.GetName(i),-20}");
                        }
                        Console.WriteLine();
                        while (reader.Read())
                        {
                            Console.WriteLine();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader.GetValue(i),-20}");
                            }
                        }
                        reader.Close();
                        Console.WriteLine();
                        WriteParagraph(new string[] { " Appuyez sur une touche pour continuer... " }, true, CursorTop + 1, Placement.Left);
                        ReadKey(true);
                        ClearContent();
                        execution = Move.Options;
                        break;
                    default:
                        execution = Move.MainMenu;
                        break;
                }
                break;
            case Profil.Staff:
                switch(ScrollingMenu("veuillez choisir une option", new string[]{
                    "Changer de couleur", 
                    "Changer mot de passe",
                    "Retour"}))
                {
                    case 0:
                        execution = Move.Couleur;
                        break;
                    case 1:
                        if (user is Profil.Staff)
                            mdpStaff = WritePrompt("Veuillez saisir le nouveau mot de passe : ");
                        execution = Move.MainMenu;
                        break;
                    default:
                        execution = Move.MainMenu;
                        break;
                }
                break;
            default:
                switch(ScrollingMenu("veuillez choisir une option", new string[]{
                    "Changer de couleur",
                    "Changer utilisateur BDD",
                    "Retour"}))
                {
                    case 0:
                        execution = Move.Couleur;
                        break;
                    case 1:
                        BDDname = WritePrompt("Veuillez saisir le nouveau nom d'utilisateur : ");
                        BDDmdp = WritePrompt("Veuillez saisir le nouveau mot de passe : ");
                        execution = Move.MainMenu;
                        break;
                    default:
                        execution = Move.MainMenu;
                        break;
                }
                break;
        }
        
    }
    public static void ChangeColor()
    {
        switch(ScrollingMenu("Veuillez choisir la nouvelle couleur de l'interface", new string[]{
            "Blanc", 
            "Rouge",
            "Magenta",
            "Jaune",
            "Vert",
            "Bleu",
            "Cyan"}))
            {
                case 0:
                    ChangeFont(White);
                    break;
                case 1:
                    ChangeFont(Red);
                    break;
                case 2:
                    ChangeFont(Magenta);
                    break;
                case 3:
                    ChangeFont(Yellow);
                    break;
                case 4:
                    ChangeFont(Green);
                    break;
                case 5:
                    ChangeFont(Blue);
                    break;
                case 6:
                    ChangeFont(Cyan);
                    break;
                case -1:
                    execution = Move.Options;
                    return;
            }
            execution = Move.Couleur;
    }
    public static Profil Authentification()
    {
        ClearContent();
        if (connection is null)
        {
            connection = new MySqlConnection($"server=localhost;user id={BDDname};password={BDDmdp};database=PROJECT");
            connection.Open();
        }
        identifiant = WritePrompt("Veuillez saisir votre email ou identifiant : ");
        if (identifiant == idStaff)
        {
            MdpGerant:
            string mdp1 = WritePrompt("Veuillez saisir le mot de passe pour s'authentifier : ");
            if (mdp1 == mdpStaff)
                return Profil.Staff;
            else
                switch(ScrollingMenu("Mot de passe incorrect, réessayer ?", new string[] { "Oui", "Non" }))
                {
                    case 0:
                        goto MdpGerant;
                    default:
                        return Profil.NonDefini;
                }
        }
        else 
        {
            string query = $"SELECT email FROM Client WHERE email = '{identifiant}'";
            MySqlCommand command = new MySqlCommand(query, connection);
            if (command.ExecuteScalar() == null)
                switch (ScrollingMenu("Aucune correspondance dans la base de données, créer nouveau profil ?", new string[] { "Oui", "Non" }))
                {
                    case 0:
                        string nom = WritePrompt("Veuillez saisir votre nom : ");
                        string prenom = WritePrompt("Veuillez saisir votre prénom : ");
                        string adresse = WritePrompt("Veuillez saisir votre adresse : ");
                        string telephone = WritePrompt("Veuillez saisir votre numéro de téléphone : ");
                        string mdp = WritePrompt("Veuillez saisir votre mot de passe : ");
                        string carte = WritePrompt("Veuillez saisir votre numéro de carte bancaire : ");
                        query = $"INSERT INTO Client (nom, prenom, adresse, telephone, email, mdp, carte_credit) VALUES ('{nom}', '{prenom}', '{adresse}', '{telephone}', '{identifiant}', '{mdp}', '{carte}')";
                        command = new MySqlCommand(query, connection);
                        command.ExecuteNonQuery();
                        return Profil.Client;
                    case 1:
                        return Profil.NonDefini;
                    default:
                        return Profil.NonDefini;
                }
            else
            {
                MdpClient:
                string mdp2 = WritePrompt("Veuillez saisir votre mot de passe : ");
                query = $"SELECT mdp FROM Client WHERE email = '{identifiant}'";
                command = new MySqlCommand(query, connection);
                if (command.ExecuteScalar().ToString() == mdp2)
                {
                    return Profil.Client;
                }
                    
                else
                    switch (ScrollingMenu("Mot de passe incorrect, réessayer ?", new string[] { "Oui", "Non" }))
                    {
                        case 0:
                            goto MdpClient;
                        case 1:
                            return Profil.NonDefini;
                        default:
                            return Profil.NonDefini;
                    }
            }
        }
    }
    public static MySqlDataReader Query(string query)
    {
        MySqlCommand command = new MySqlCommand(query, connection);
        return command.ExecuteReader();
    }
    #endregion
}