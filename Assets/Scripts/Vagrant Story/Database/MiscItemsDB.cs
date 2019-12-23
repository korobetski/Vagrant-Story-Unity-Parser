using System.Collections.Generic;
using UnityEngine;
using VagrantStory.Items;

namespace VagrantStory.Database
{
    public class MiscItemsDB : MonoBehaviour
    {
        public static Misc Racine_HP = new Misc("Racine HP", "Racine d'une herbe revigorante. Rétablit 50 HP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.HP, +50) });
        public static Misc Bulbe_HP = new Misc("Bulbe HP", "Bulbe d'une plante vivifiante. Restaure 100 HP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.HP, +100) });
        public static Misc Algue_HP = new Misc("Algue HP", "Extrait d'une plante rajeunissante. Rétablit 150 HP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.HP, +150) });
        public static Misc Potion_HP = new Misc("Potion HP", "Potion végétale reconstituante. Restaure tous les HP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.HP, +short.MaxValue) });
        public static Misc Racine_MP = new Misc("Racine MP", "Racine d'une herbe stimulant l'intelligence. Rétablit 25 MP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.MP, +25) });
        public static Misc Bulbe_MP = new Misc("Bulbe MP", "Bulbe d'une plante stimulant l'intelligence. Recharge 50 MP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.MP, +50) });
        public static Misc Algue_MP = new Misc("Algue MP", "Extrait de plante vivifiant l'intelligence. Rétablit 100 MP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.MP, +100) });
        public static Misc Potion_MP = new Misc("Potion MP", "Potion végétale stimulant l'intelligence. Recharge tous les MP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.MP, +short.MaxValue) });
        public static Misc Racine_Risk = new Misc("Racine Risk", "Racine stimulant le système nerveux. Réduit les Risks de 25 points.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.RISK, -25) });
        public static Misc Bulbe_Risk = new Misc("Bulbe Risk", "Bulbe qui stimule le système nerveux. Réduit les Risks de 50 points.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.RISK, -50) });
        public static Misc Algue_Risk = new Misc("Algue Risk", "Extrait végétal stimulant le système nerveux. Réduit les Risks de 75 points.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.RISK, -75) });
        public static Misc Potion_Risk = new Misc("Potion Risk", "Potion végétale vivifiant le système nerveux. Réduit les Risks à 0.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.RISK, -short.MaxValue) });
        public static Misc Liqueur = new Misc("Liqueur", "Antidote des seigneurs de LéaMundis. Restaure 100 HP et MP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.HP, +100),
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.MP, +100) });
        public static Misc Fine_dalcool = new Misc("Fine d'alcool", "Antidote céleste. Rétablit tous les HP et MP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.HP, +short.MaxValue),
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.MP, +short.MaxValue) });
        public static Misc Eau_de_vie = new Misc("Eau-de-vie", "Potion expérimentale des alchimistes royaux. Restaure 25 HP, réduit les Risks de 25 points.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.HP, +25),
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.RISK, -25) });
        public static Misc Digestif = new Misc("Digestif", "Réactif d'anciens sorciers bafoués. Restaure 50 HP, réduit les Risks de 50 points.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.HP, +50),
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.MODIFIER, ItemEffect.Mod.RISK, -50) });
        public static Misc Incito_Fluide = new Misc("Incito Fluide", "Extrait d'une plante réputée pour guérir lesaltérations d'état. Guérit \"Paralysie\".", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.REMOVE_STATUS, ItemEffect.Status.Paralysis) });
        public static Misc AntiVenin = new Misc("AntiVenin", "Fabriqué à l'époque de la grande guerre des serpents. Guérit \"Poison\".", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.REMOVE_STATUS, ItemEffect.Status.Poison) });
        public static Misc Stimulant = new Misc("Stimulant", "Mixture de pétales de fleurs flétries. Guérit \"Torpeur\".", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.REMOVE_STATUS, ItemEffect.Status.Numbness) });
        public static Misc Grâce = new Misc("Grâce", "Talisman portant le sceau de Saint Iokus.Guérit \"Malédiction\".", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.REMOVE_STATUS, ItemEffect.Status.Curse) });
        public static Misc Panacée = new Misc("Panacée", " Potion composée de pousses végétales.Guérit \"Paralysie\", \"Poison\" et \"Torpeur\".", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.REMOVE_STATUS, ItemEffect.Status.Paralysis),
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.REMOVE_STATUS, ItemEffect.Status.Poison),
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.REMOVE_STATUS, ItemEffect.Status.Numbness)});
        public static Misc Esuna = new Misc("Esuna", "Mixture d'ailes de lucioles séchées. Annule tous les sortilèges.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.REMOVE_STATUS, ItemEffect.Status.ALL_DEBUFF) });
        public static Misc Speed = new Misc("Speed", "Poudre des fées accélérant les mouvements et rallongeant temporairement les sauts.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.ADD_STATUS, ItemEffect.Status.Speed, 60) });
        public static Misc Nectar_Frc = new Misc("Nectar Force", "Elixir utilisé à l'époque de l'Inquisition. Augmente niveau total: FRC (Force).", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.STR, ItemEffect.Roll(4)) });
        public static Misc Nectar_Int = new Misc("Nectar Inteligence", "Elixir des Inquisiteurs Royaux. Augmente niveau total: INT (Intelligence).", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.INT, ItemEffect.Roll(4)) });
        public static Misc Nectar_Agl = new Misc("Nectar Agilité", "Elixir des adorateurs de Saint Iokus. Augmente niveau total: AGL (Agilité).", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.AGI, ItemEffect.Roll(4)) });
        public static Misc Nectar_HP = new Misc(" Nectar HP", "Elixir des vierges de LéaMundis. Augmente le niveau total de vos HP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.HP, ItemEffect.Roll(4)) });
        public static Misc Nectar_MP = new Misc("Nectar MP", "Elixir jadis utilisé par les mages. Augmente le niveau total de vos MP.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.MP, ItemEffect.Roll(4)) });
        public static Misc Cru_Vaillance = new Misc("Cru Vaillance", "Vin rouge de LéaMundis, délicat mais fort en bouche.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.STR, ItemEffect.Roll(4)) });
        public static Misc Cru_Prudence = new Misc("Cru Prudence", "Le plus noble des vins rouges, de l'arôme et du bouquet.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.INT, ItemEffect.Roll(4)) });
        public static Misc Manoir_Vif = new Misc("Manoir Vif", "Vin blanc des vignes de LéaMundis, réputé pour son odeur de miel.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.AGI, ItemEffect.Roll(4)) });
        public static Misc Château_Audace = new Misc("Château Audace", "Vin doux de grande qualité.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.HP, ItemEffect.Roll(4)) });
        public static Misc Saint_Virtux = new Misc(" Saint Virtux", "Vin pétillant, tiré d'un mélange de trois célèbres cépages.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.PERMA_MOD, ItemEffect.Mod.MP, ItemEffect.Roll(4)) });
        public static Misc Oeil_dArgon = new Misc("Oeil d'Argon", "Révèle tous les pièges d'une pièce, pendant un bref instant.", new List<ItemEffect>() {
                            new ItemEffect(ItemEffect.Target.SELF, ItemEffect.Type.ADD_STATUS, ItemEffect.Status.Argon, 60) });
    }
}

