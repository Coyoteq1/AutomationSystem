using System.Collections.Generic;
using System.Reflection;

namespace CrowbaneArena.Data
{
    /// <summary>
    /// V Blood boss GUIDs - Based on ICB.core implementation
    /// </summary>
    public static class VBloodGUIDs
    {
        // V Blood Boss GUIDs (all bosses from V Rising 1.1+)
        public const int AlphaWolf = -1905691330;
        public const int KeelyTheFrostArcher = -2124095595;
        public const int RufusTheForeman = -1390464837;
        public const int ErrolTheStonebreaker = -1899496294;
        public const int GraysonTheArmorer = 1124728805;
        public const int GrethelTheGlassblower = 2109459277;
        public const int Putrid_Rat = 1106458752;
        public const int GoreswineTheSavage = -1347412392;
        public const int Lidia_TheChaosArcher = 763273073;
        public const int Clive_TheFirestarter = 1896428751;
        public const int Polora_TheFeywalker = 1945956671;
        public const int Beatrice_TheTailor = -1942352521;
        public const int Vincent_TheFrostbringer = 1124739990;
        public const int Tristan_TheVampireHunter = 1391546313;
        public const int Mairwyn_TheElementalist = -2025101517;
        public const int Leandra_TheShadowPriestess = -1936217296;
        public const int Jade_TheVampireHunter = -1968372384;
        public const int Meredith_TheBrightArcher = -1605403740;
        public const int Octavian_TheMilkEye = 1688478381;
        public const int Raziel_TheShepherd = -680319732;
        public const int Ungora_TheSpiderQueen = -548489519;
        public const int Terrorclaw_TheOgre = -1347412392;
        public const int Foulrot_TheSoultaker = 1362814124;
        public const int Christina_TheSunPriestess = -99012450;
        public const int Quincey_TheBanditKing = -1659822956;
        public const int Voltatia_TheStormHarbinger = 2054432370;
        public const int Frostmaw_TheMountainTerror = 24378719;
        public const int Azariel_TheSunbringer = 1112948473;
        public const int Cyril_TheCursedSmith = -1347412392;
        public const int Willfred_TheWerewolfChief = -1137611264;
        public const int Morian_TheStormwingMatriarch = 685266977;
        public const int Angram_ThePurifier = 106480588;
        public const int Solarus_TheImmaculate = 272705418;
        public const int Nightmarshal_Styx = 1688478381;
        public const int Maja_TheDarkSavant = 914043867;
        public const int Gorecrusher_TheBehemoth = -1347412392;
        public const int Domina_TheBlademaster = -1659822956;
        public const int TheWingedHorror = -1347412392;
        public const int TheDuke_OfBalaton = 577478542;

        /// <summary>
        /// Get all V Blood boss GUIDs using reflection
        /// </summary>
        public static List<int> GetAll()
        {
            var guids = new List<int>();
            var fields = typeof(VBloodGUIDs).GetFields(
                BindingFlags.Public | 
                BindingFlags.Static | 
                BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(int))
                {
                    guids.Add((int)field.GetValue(null));
                }
            }
            
            return guids;
        }

        /// <summary>
        /// Get count of V Blood bosses
        /// </summary>
        public static int Count => GetAll().Count;
    }
}
