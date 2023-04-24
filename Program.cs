using MySql.Data.MySqlClient;
using static System.Console;
using static MDD_FlowerStore.ConsoleVisuals;
using static System.ConsoleColor;

namespace MDD_FlowerStore;

public class Program
{
    #region Champs
    public static MySqlConnection connection = new MySqlConnection("server=localhost;user id=mdd;password=mdd;database=PROJECT");
    public static string mdpGerant = "securite";
    public static Move execution = Move.MainMenu;
    public static Profil user = Profil.NonDefini;
    public static string identifiant = "";

    #endregion

    #region Enum
    public enum Profil
    {
        Gerant,
        Client,
        NonDefini
    }
    public enum Move
    {
        Continue,
        MainMenu,
        Options,
        Couleur,
        Back,
        Exit
    }
    #endregion

    #region Méthodes
    public static void Main(string[] args)
    {
        connection.Open();
        WriteFullScreen(default);

        MainMenu:

        MainMenu();
        switch(execution)
        {
            case Move.MainMenu:
                goto MainMenu;
            case Move.Options:
                goto Options;
            case Move.Exit:
                goto Exit;
            default:
                break;
        }


        
        Exit:

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
            default:
                break;
        }

        Couleur:
        ChangeColor();
        goto Options;
    }
    public static void MainMenu()
    {
        if (user is Profil.NonDefini)
            switch (ScrollingMenu("Bienvenue dans l'interface utilisateur des boutiques de M.BelleFleur, veuillez choisir votre prochaine action.", new string[]{"Authentifier", "Options", "Quitter"}))
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
        else    
            switch (ScrollingMenu("Bienvenue dans l'interface utilisateur des boutiques de M.BelleFleur, veuillez choisir votre prochaine action.", new string[]{"Actions", "Options", "Déconnexion"}))
            {
                case 0:
                    // ! Actions à définir
                    execution = Move.MainMenu;
                    // ! Temp
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
        switch(user)
        {
            case Profil.Gerant: case Profil.Client:
                switch(ScrollingMenu("veuillez choisir une option", new string[]{
                    "Changer de couleur", 
                    "Changer mot de passe",
                    "Retour"}))
                {
                    case 0:
                        execution = Move.Couleur;
                        break;
                    case 1:
                        if (user is Profil.Gerant)
                            mdpGerant = WritePrompt("Veuillez saisir le nouveau mot de passe : ");
                        else
                        {
                            string mdp = WritePrompt("Veuillez saisir le nouveau mot de passe : ");
                            string query = $"UPDATE Client SET mdp = '{mdp}' WHERE email = '{identifiant}'";
                            MySqlCommand command = new MySqlCommand(query, connection);
                            command.ExecuteNonQuery();
                        }
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
                    "Retour"}))
                {
                    case 0:
                        execution = Move.Couleur;
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
        ID:
        string identifiant = WritePrompt("Veuillez saisir votre email ou identifiant : ");
        if (identifiant == "BelleFleur")
        {
            MdpGerant:
            string mdp1 = WritePrompt("Veuillez saisir le mot de passe pour s'authentifier : ");
            if (mdp1 == mdpGerant)
            {
                identifiant = "BelleFleur";
                return Profil.Gerant;
            }  
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
                switch (ScrollingMenu("Email incorrect, réessayer ?", new string[] { "Oui", "Non" }))
                {
                    case 0:
                        goto ID;
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
    #endregion
    
    
    /*
        string query = "SELECT * FROM Boutique";
        MySqlCommand command = new MySqlCommand(query, connection);

        MySqlDataReader reader = command.ExecuteReader();
        WriteLine($"{reader.GetName(0),-4} {reader.GetName(1),-10} {reader.GetName(2),10}");
        while (reader.Read())
        {
            WriteLine($"{reader.GetInt32(0),-4} {reader.GetString(1),-10} {reader.GetString(2),10}");
        }
    */
}