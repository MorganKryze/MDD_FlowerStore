using MySql.Data.MySqlClient;
using static System.Console;
using static MDD_FlowerStore.ConsoleVisuals;

namespace MDD_FlowerStore;

public class Program
{
    public static MySqlConnection connection = new MySqlConnection("server=localhost;user id=mdd;password=mdd;database=PROJECT");
    public static string mdpGerant = "securite";
    public static Move execution = Move.MainMenu;
    public static Profil user;
    public static void Main(string[] args)
    {
        connection.Open();
        WriteFullScreen(default);

        MainMenu:

        MainMenu();
        if (execution == Move.Exit)
            goto Exit;

        user = Authentification(LogIn());
        WriteParagraph(new string[]{ "Vous êtes connecté en tant que " + user});
        ReadKey(true);


        Options:

        
        Exit:

        connection.Close();
        ProgramExit();
    }
    public enum Move
    {
        MainMenu,
        Options,
        Back,
        Exit
    }
    public static void MainMenu()
    {
        switch (ScrollingMenu("Bienvenue dans l'interface utilisateur de la boutique de M.BelleFleur, veuillez choisir votre prochaine action.", new string[]{"Authentification", "Options", "Quitter"}))
        {
            case 0:
                user = Authentification(LogIn());
                break;
            case 1:
                //Options();
                break;
            case 2:
                execution = Move.Exit;
                break;
            default:
                break;
        }
    }
    public enum Profil
    {
        Gerant,
        Client,
        NonDefini
    }
    public static Profil LogIn()
    {
        switch(ScrollingMenu("Veuillez choisir votre statut :", new string[] { "Gerant", "Client" }))
        {
            case 0:
                return Profil.Gerant;
            case 1:
                return Profil.Client;
            default:
                return Profil.NonDefini;
        }
    }
    public static Profil Authentification(Profil utilisateur)
    {
        switch (utilisateur)
        {
            case Profil.Gerant:
                MdpGerant:
                string mdp1 = WritePrompt("Veuillez saisir le mot de passe pour s'authentifier : ");
                if (mdp1 == mdpGerant)
                    return Profil.Gerant;
                else
                    switch(ScrollingMenu("Mot de passe incorrect, réessayer ?", new string[] { "Oui", "Non" }))
                    {
                        case 0:
                            goto MdpGerant;
                        case 1:
                            return Profil.NonDefini;
                        default:
                            return Profil.NonDefini;
                    }
            case Profil.Client:
                Email:
                string email = WritePrompt("Veuillez saisir votre email : ");
                string query = $"SELECT email FROM Client WHERE email = '{email}'";
                MySqlCommand command = new MySqlCommand(query, connection);
                if (command.ExecuteScalar() == null)
                    switch (ScrollingMenu("Email incorrect, réessayer ?", new string[] { "Oui", "Non" }))
                    {
                        case 0:
                            goto Email;
                        case 1:
                            return Profil.NonDefini;
                        default:
                            return Profil.NonDefini;
                    }
                else
                {
                    MdpClient:
                    string mdp2 = WritePrompt("Veuillez saisir votre mot de passe : ");
                    query = $"SELECT mdp FROM Client WHERE email = '{email}'";
                    command = new MySqlCommand(query, connection);
                    if (command.ExecuteScalar().ToString() == mdp2)
                        return Profil.Client;
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
            default:
                return Profil.NonDefini;
        }
    }
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