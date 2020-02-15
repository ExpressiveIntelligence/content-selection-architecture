using CSA.Controllers;
using CSA.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static CSA.Demo.ContentUnitSetupForDemos;
using static CSA.Core.KCNames;
using System.Linq;

namespace Demo
{
    class TavernTalk
    {
        public IBlackboard Blackboard { get; }
        public ScheduledSequenceController Controller { get; }

        public const string AbstractSocialMoves = "Abstract Social Moves";
        public const string CharacterKnowledgeBase = "Character Knowledge Base";
        public const string Characters = "Characters";
        public const string SocialRules = "Social Rules";

        public const string PotentialSocialMoves = "Potential Social Moves";
        public const string BoundSocialMoves = "Bound Social Moves";
        public const string WeightedSocialMoves = "Weighted Social Moves";
        public const string SelectedSocialMove = "Selected Social Move";


        public TavernTalk()
        {
            Blackboard = new Blackboard();
            TavernTalk_DefineUnits(Blackboard);

            /* 
             * This does not account for picking who is acting. 
             * For the moment this is probably always the same person, 
             * as the referenced KB does not change, while it probably should if 
             * this is turn taking
             */

            // Get all ASMs that need a social fact
                //combine with all facts in KB 
                // -> Put in Potential Social Moves Pool
            //Move over ASMs that don't need a fact
                // -> Put in Potential Social Moves Pool

            //Get all PSMs that need a target
                // combine with all characters in the room
                // -> put in Bound Social Moves Pool
            //Move over PSMs that dont' need a target
                // -> put in Bound Social Moves Pool

            //For each BSM, determine which rules apply and of those, which are in effect
                // Do a UtilitySum for each
                // -> put in Weighted Social Moves Pool

            //Pick the highest weighted one

        }


        public static void TavernTalk_DefineUnits(IBlackboard blackboard)
        {
            //Abstract Social Moves:
            Unit move = new Unit();
            move.AddComponent(new KC_UnitID("tell", true));
            move.AddComponent(new KC_ContentPool(AbstractSocialMoves));
            blackboard.AddUnit(move);

            move = new Unit();
            move.AddComponent(new KC_UnitID("ask", true));
            move.AddComponent(new KC_ContentPool(AbstractSocialMoves));
            blackboard.AddUnit(move);




            //Character Knowledge Base Facts:
            /* 
             * Structure should contain at least:
             *      id: string
             *      subject: string
             *      verb: string
             *      object: string
             *      opinion: int
             *      
             * Will likely also want things like: 
             *      time
             *      location
             *      state (as opposed to verb)
             *      
             */

            Unit fact = new Unit();
            fact.AddComponent(new KC_UnitID("Alicia dating Jasper", true));
            move.AddComponent(new KC_ContentPool(CharacterKnowledgeBase));
            blackboard.AddUnit(fact);

            fact = new Unit();
            fact.AddComponent(new KC_UnitID("Alicia is busy", true));
            move.AddComponent(new KC_ContentPool(CharacterKnowledgeBase));
            blackboard.AddUnit(fact);




            //Characters list:
            // I think this should just be the characters currently in the bar
            // may each eventaully have their own knowledge pool?
            // where is social relations stored? In Prologue?
            Unit character = new Unit();
            character.AddComponent(new KC_UnitID("Barkeep", true));
            move.AddComponent(new KC_ContentPool(Characters));
            blackboard.AddUnit(character);

            character = new Unit();
            character.AddComponent(new KC_UnitID("Marco", true));
            move.AddComponent(new KC_ContentPool(Characters));
            blackboard.AddUnit(character);




            //Social Rules:
            Unit rule = new Unit();
            rule.AddComponent(new KC_UnitID("Ask friends stuff", true));
            rule.AddComponent(new KC_PrologExpression(KCNames.ApplTest_Prolog, "ask(X).", true));
            rule.AddComponent(new KC_PrologExpression(KCNames.SalienceTest_Prolog, "friends(X).", true));
            rule.AddComponent(new KC_Utility(5, true));
            rule.AddComponent(new KC_ContentPool(SocialRules, true));
            blackboard.AddUnit(rule);



            //Prolog Database:

            Unit prologKBUnit = new Unit();
            KC_PrologKB prologKB = new KC_PrologKB("Global", true);
            prologKBUnit.AddComponent(prologKB);

            /*
             * fixme: is there a better way to define a predicate for prolog than asserting and retracting it?
             */
            prologKB.Assert("ask(a).");
            prologKB.Retract("ask(a).");
            prologKB.Assert("friends(Marco).");
            prologKB.Retract("friends(Marco).");
            
            blackboard.AddUnit(prologKBUnit);
        }
    }
}
