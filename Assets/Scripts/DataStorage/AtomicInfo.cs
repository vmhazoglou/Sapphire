using UnityEngine;

namespace Data.Elements
{
    public static class AtomicInfo
    {

        // Element symbols
        public static string[] symbols = { "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne",
                                            "Na", "Mg", "Al", "Si", "P", "S", "Cl", "Ar", "K", "Ca",
                                            "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn"};

        // Covalent radii from http://www.crystalmaker.com/support/tutorials/crystalmaker/atomic-radii/index.html
        public static float[] radii = { 0.37f, 0.32f, 1.34f, 0.90f, 0.82f, 0.77f, 0.75f, 0.73f, 0.71f, 0.69f,
                                        1.54f, 1.30f, 1.18f, 1.11f, 1.06f, 1.02f, 0.99f, 0.97f, 1.96f, 1.74f,
                                        1.44f, 1.36f, 1.25f, 1.27f, 1.39f, 1.25f, 1.26f, 1.21f, 1.38f, 1.31f};

    }
}