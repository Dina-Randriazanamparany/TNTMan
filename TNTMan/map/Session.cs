﻿using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using TNTMan.ecrans;
using TNTMan.entitees;
using TNTMan.map.blocs;

namespace TNTMan.map
{
    class Session
    {
        String musique;
        List<Joueur> joueurs;
        Map map;
        int mancheActuelle;
        int nbManches;
        long tempsImparti;
        DateTime tempsDebutManche;
        DateTime tempsFinManche = DateTime.MinValue;
        long tempsMortSubite;
        long tempsRestant;

        public Session(int nb_joueurs, int id_map, int nb_manches, long temps_imparti, long temps_mort_subite)
        {
            PointApparition pointApparition = null;

            musique = @"jeu_"+Program.random.Next(1, 2);
            joueurs = new List<Joueur>();
            map = new Map();
            map.chargerMap(id_map);
            for (int i = 0; i < nb_joueurs; i++)
            {
                pointApparition = map.pointApparitions[i];
                switch (i)
                {
                    case 0:
                        joueurs.Add(new Joueur(1, 0.5f + pointApparition.X, 0.5f + pointApparition.Y, map, SDL.SDL_Scancode.SDL_SCANCODE_W, SDL.SDL_Scancode.SDL_SCANCODE_S, SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D, SDL.SDL_Scancode.SDL_SCANCODE_E));
                        break;
                    case 1:
                        joueurs.Add(new Joueur(2, 0.5f + pointApparition.X, 0.5f + pointApparition.Y, map, SDL.SDL_Scancode.SDL_SCANCODE_UP, SDL.SDL_Scancode.SDL_SCANCODE_DOWN, SDL.SDL_Scancode.SDL_SCANCODE_LEFT, SDL.SDL_Scancode.SDL_SCANCODE_RIGHT, SDL.SDL_Scancode.SDL_SCANCODE_RETURN));
                        break;
                    case 2:
                        joueurs.Add(new Joueur(3, 0.5f + pointApparition.X, 0.5f + pointApparition.Y, map, SDL.SDL_Scancode.SDL_SCANCODE_U, SDL.SDL_Scancode.SDL_SCANCODE_J, SDL.SDL_Scancode.SDL_SCANCODE_H, SDL.SDL_Scancode.SDL_SCANCODE_K, SDL.SDL_Scancode.SDL_SCANCODE_I));
                        break;
                    case 3:
                        joueurs.Add(new Joueur(4, 0.5f + pointApparition.X, 0.5f + pointApparition.Y, map, SDL.SDL_Scancode.SDL_SCANCODE_KP_5, SDL.SDL_Scancode.SDL_SCANCODE_KP_2, SDL.SDL_Scancode.SDL_SCANCODE_KP_1, SDL.SDL_Scancode.SDL_SCANCODE_KP_3, SDL.SDL_Scancode.SDL_SCANCODE_KP_6));
                        break;
                    default:
                        Program.MessageErr("Nombre de joueurs incorrect!");
                        break;
                }
            }
            foreach(var joueur in joueurs)
            {
                map.ajoutEntite(joueur);
            }
            mancheActuelle = 1;
            nbManches = nb_manches;
            tempsDebutManche = DateTime.Now;
            tempsImparti = temps_imparti;
            tempsMortSubite = temps_mort_subite;
            Sfx.ArreterJouerMusique();
            Sfx.JouerMusique(musique);
        }

        public void dessiner(IntPtr rendu)
        {
            DateTime temps_actuel = DateTime.Now;
            Size resolution = Gfx.getResolution();
            Size cadre_temps_restant = Gfx.getTailleRectangleTexte(18, "Temps Restant : {0}:{1:D2}", tempsRestant / 60, tempsRestant % 60);
            for (int x = 0; x < Map.LARGEUR_GRILLE; x++)
                for (int y = 0; y < Map.LONGUEUR_GRILLE; y++)
                {
                    Bloc bloc = map.getBlocA(x, y);
                    if (bloc != null)
                    {
                        bloc.dessiner(rendu, x, y);
                    }
                }

            map.dessiner(rendu);

            for (int i = 0; i < joueurs.Count; i++)
            {
                if (i >= 2)
                {
                    Gfx.dessinerTexte(5, resolution.Height - i * 20, 18, Color.Black, "J{0} - B:{1}/{4}|P:{2}|V:{3}", i + 1, joueurs[i].getNbBombes(), joueurs[i].getPortee(), (int)((joueurs[i].getVitesse() - 0.05) / 0.01f) + 1, joueurs[i].getMaxNbBombes());
                }
                else
                {
                    Gfx.dessinerTexte(5, 5 + i * 20, 18, Color.Black, "J{0} - B:{1}/{4}|P:{2}|V:{3}", i + 1, joueurs[i].getNbBombes(), joueurs[i].getPortee(), (int)((joueurs[i].getVitesse() - 0.05) / 0.01f) + 1, joueurs[i].getMaxNbBombes());
                }
            }

            Gfx.dessinerTexte(resolution.Width / 2 - cadre_temps_restant.Width / 2, 5, 18, Color.Black, "Temps Restant : {0}:{1:D2}", tempsRestant / 60, tempsRestant % 60);

            // Affichage du message de transition entre les manches
            if ((tempsFinManche > DateTime.MinValue) && (temps_actuel - tempsFinManche).TotalSeconds < 10)
            {
                if (mancheActuelle < nbManches)
                    afficherTransition(resolution, temps_actuel);
                else
                    afficherScores(resolution);
            }
        }

        public void gererTouches(byte[] etats)
        {
            map.gererTouches(etats);
        }

        void finDeLaManche(int raison)
        {
            PointApparition pointApparition = null;

            // si un joueur gagne
            if (raison > 0)
            {
                joueurs[raison - 1].incrementerVictoire();
            }

            if (mancheActuelle < nbManches)
            {
                map.dechargerMap();
                // On charge la map pour la prochaine manche
                map.chargerMap(map.getId());
                foreach (var joueur in joueurs)
                {
                    pointApparition = map.pointApparitions[joueur.getId() - 1];
                    joueur.reapparaitre(0.5f + pointApparition.X, 0.5f + pointApparition.Y);
                    map.ajoutEntite(joueur);
                }

                // On réinitialise le temps
                tempsDebutManche = DateTime.Now;
                // On passe à la manche suivante
                mancheActuelle++;
                // Relancer la musique
                Sfx.JouerMusique(musique);
            }
            else
            {
                // fin de la partie
                Gfx.changerEcran(new Ecran_Titre());
            }
        }

        public void afficherTransition(Size resolution, DateTime temps_actuel)
        {
            // On crée un message de transition
            Gfx.remplirRectangle(resolution.Width / 2 - 225, resolution.Height / 2 - 75, 450, 150, 1, Color.White, Color.Red);
            Gfx.dessinerTexte(resolution.Width / 2 - 150, resolution.Height / 2 - 25, 30, Color.Red, "Fin de la manche {0} !", mancheActuelle);
            Gfx.dessinerTexte(resolution.Width / 2 - 150, resolution.Height / 2 + 10, 15, Color.Black, "Début de la prochaine manche dans ...");
            Gfx.dessinerTexte(resolution.Width / 2 - 40, resolution.Height / 2 + 30, 15, Color.Black, "{0} secondes", (int)(10 - (temps_actuel - tempsFinManche).TotalSeconds));
        }

        public void afficherScores(Size resolution)
        {
            int ecart = 30;
            // On crée un panneau des scores
            Gfx.remplirRectangle(resolution.Width / 2 - 235, 10, 500, resolution.Height - 20, 1, Color.White, Color.Red);
            Gfx.dessinerTexte(resolution.Width / 2 - 120, 30, 30, Color.Red, "FIN DE LA PARTIE !");
            // On affiche l'en-tête du tableau des scores
            Gfx.dessinerTexte(resolution.Width / 2 - 210, 70, 20, Color.DarkGreen, "Joueur");
            Gfx.dessinerTexte(resolution.Width / 2 - 130, 70, 20, Color.DarkGreen, "Nombre de victoires");
            Gfx.dessinerTexte(resolution.Width / 2 + 80, 70, 20, Color.DarkGreen, "Nombre de tués");
            // On affiche les scores pour tous les joueurs
            foreach(Joueur j in joueurs)
            {
                Gfx.dessinerTexte(resolution.Width / 2 - 210, 70 + ecart, 20, Color.Black, "{0}", j.getId());
                Gfx.dessinerTexte(resolution.Width / 2 - 130, 70 + ecart, 20, Color.Black, "{0}", j.getScore().getNbVictoires());
                Gfx.dessinerTexte(resolution.Width / 2 + 80, 70 + ecart, 20, Color.Black, "{0}", j.getScore().getNbTues());
                ecart += 20;
            }
            // On affiche le numéro du gagnant
            Gfx.dessinerTexte(resolution.Width / 2 - 150, (resolution.Height - 20) * 3 / 4, 20, Color.DarkGreen, "Le gagnant est ...");
            Gfx.dessinerTexte(resolution.Width / 2 - 70, (resolution.Height - 20) * 3 / 4 + 20, 30, Color.DarkGreen, "Joueur n°{0} !", getGagnantID());
        }

        public int getGagnantID()
        {
            Joueur gagnant = joueurs[0];
            for(int i = 1; i<joueurs.Count; i++)
            {
                if (gagnant.getScore().getNbVictoires() < joueurs[i].getScore().getNbVictoires())
                    gagnant = joueurs[i];
                else if (gagnant.getScore().getNbVictoires() == joueurs[i].getScore().getNbVictoires() && gagnant.getScore().getNbTues() < joueurs[i].getScore().getNbTues())
                    gagnant = joueurs[i];
            }
            return gagnant.getId();
        }

        public void mettreAJour()
        {
            DateTime temps_actuel = DateTime.Now;
            List<Joueur> joueurs_en_vie = joueurs.FindAll((j) => !j.estMort());
            // Variables locales utilisées pour la fin de la manche
            int raison_fin_manche = 0;

            // Si il ne reste aucun joueur en vie c'est la fin de la manche
            if (joueurs_en_vie.Count < 1)
            {
                if (tempsFinManche == DateTime.MinValue)
                {
                    tempsFinManche = temps_actuel;
                    // Arrêter la musique
                    Sfx.ArreterJouerMusique();
                }
            }
            // Si le temps est écoulé c'est la fin de la manche
            if (tempsImparti - (temps_actuel - tempsDebutManche).TotalSeconds < -1)
            {
                if (tempsFinManche == DateTime.MinValue)
                {
                    tempsFinManche = temps_actuel;
                    // Arrêter la musique
                    Sfx.ArreterJouerMusique();
                    // On joue le son temps_ecoule en cas de fin de manche par temps écoulé
                    Sfx.JouerSon("temps_ecoule");
                }
            }
            // Si il reste un joueur en vie il est déclaré vainqueur
            if (joueurs_en_vie.Count == 1)
            {
                if (tempsFinManche == DateTime.MinValue)
                {
                    tempsFinManche = temps_actuel;
                    // Arrêter la musique
                    Sfx.ArreterJouerMusique();
                    // Un joueur a gagné
                    raison_fin_manche = joueurs_en_vie[0].getId();
                }
            }

            // On met à jour la map si la fin de manche n'est pas signalé
            if (tempsFinManche == DateTime.MinValue)
            {
                tempsRestant = tempsImparti - (long)(temps_actuel - tempsDebutManche).TotalSeconds;
                map.mettreAJour();
            }
            else
            {
                // On passe à la manche suivante au bout de 10 secondes
                if ((temps_actuel - tempsFinManche).TotalSeconds >= 10)
                {
                    tempsFinManche = DateTime.MinValue;
                    finDeLaManche(raison_fin_manche);
                }
            }
        }
    }
}
