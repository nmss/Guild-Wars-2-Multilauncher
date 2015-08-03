using System;
using System.IO;

namespace gw2_launcher
{
    class Options
    {
        public static string jeu = "";
        public static string arguments = "";
        public static string chemin = "";

        public static bool compteCourant = true;
        public static string utilisateur = "";
        public static string pass = "";

        public static void parse()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 2)
            { // les 2 premiers parametres sont le nom et l'emplacement du jeu
                jeu = args[1];
                arguments = args[2];
                chemin = Path.GetFullPath(args[3]);

                if (args.Length > 4)
                { // les 2 paramètres suivants sont le nom et le mot de passe du compte windows
                    compteCourant = false;
                    utilisateur = args[4];
                    pass = args[5];
                }
            }
        }
    }
}
