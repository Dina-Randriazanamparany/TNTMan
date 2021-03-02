﻿namespace TNTMan.map.blocs
{
    internal class Bloc
    {
        public static readonly int TAILLE_BLOC = 32;

        int id;
        int durabilite;
        bool solide;

        // Bloc est une classe parente de tout les types de bloc que l'on peut placer
        protected Bloc() { }

        protected Bloc(int _id, int _durabilite, bool _solide)
        {
            id = _id;
            durabilite = _durabilite;
            solide = _solide;
        }

        public int getId()
        {
            return id;
        }

        public int getDurabilite()
        {
            return durabilite;
        }

        public bool estSolide()
        {
            return solide;
        }
    }
}