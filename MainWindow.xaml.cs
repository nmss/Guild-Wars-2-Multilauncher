using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace gw2_launcher
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                quitter();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            run();
        }

        private void quitter()
        {
            App.Current.Shutdown();
        }
        private void quitter(object o)
        {
            quitter();
        }
        private void quitter(object o, EventArgs e)
        {
            quitter();
        }

        private void run()
        {
            Options.parse();

            bool ok = false;
            bool oktmp;

            List<System.Diagnostics.Process> processes = new List<System.Diagnostics.Process>();
            processes.AddRange(CustomApi.getProcesses("gw2"));
            processes.AddRange(CustomApi.getProcesses("gw"));

            if (processes.Count == 0)
            {
                contenu.Items.Add("Aucun Guild Wars n'a été trouvé.");
            }
            foreach (System.Diagnostics.Process process in processes)
            {
                contenu.Items.Add("[" + process.Id + "] " + process.ProcessName + " ...");
                oktmp = CustomApi.CloseHandle(process, @"\\Sessions\\[0-9]\\BaseNamedObjects\\AN-Mutex-Window-Guild Wars");
                if (oktmp)
                {
                    contenu.Items.Add("\tOK");
                }
                else
                {
                    contenu.Items.Add("\tErreur");
                }
                ok = ok || oktmp;
            }

            if (Options.jeu.Equals("gw2") || Options.jeu.Equals("gw"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = Options.chemin;
                startInfo.FileName = System.IO.Path.Combine(Options.chemin, Options.jeu + ".exe");
                startInfo.Arguments = Options.arguments;
                if (!Options.compteCourant)
                {
                    startInfo.LoadUserProfile = true;
                    startInfo.UseShellExecute = false;
                    startInfo.UserName = Options.utilisateur;
                    SecureString pass = new SecureString();
                    foreach (char c in Options.pass) { pass.AppendChar(c); }
                    startInfo.Password = pass;
                }
                contenu.Items.Add("Lancement de " + Options.jeu);
                Process.Start(startInfo);
            }
            if (ok)
            {
                quitter();
            }
            else
            {
                Timer.delay(quitter, new TimeSpan(0, 0, 2));
            }

        }
    }
}
