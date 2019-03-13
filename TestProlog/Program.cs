using System;
using System.Dynamic;
using Prolog;
using CSA.Core;


/* 
 * A test rig for testing integration of UnityProlog (https://github.com/ianhorswill/UnityProlog) with CSA.
 * */
namespace TestProlog
{
    static class Program
    {
        public static void Main(string[] args)
        {
            //CreatePrologKBAFromFilAndQuery();
            //AssertingAFact();
            // TestExpandoObjectWithInterface();

        }

        private static void CreatePrologKBAFromFilAndQuery()
        {
            // fixme: KnowledgeBase wants a reference to a game object. This is because GameObjects can form a tree of knowledge bases. 
            // But I don't need this. Ideally we'd refactor prolog so that there is a lower abstract level that makes no reference to Unity classes.
            // Alternatively, a KB could be created on the Unity side and passed into the CSA. 
            KnowledgeBase kb = new KnowledgeBase("Global", null);
            kb.Consult("PrologTest.prolog");
            var query = ISOPrologReader.Read("mortal(socratese).");
            if (kb.IsTrue(query))
            {
                Console.WriteLine("Socratese is mortal.");
            }
            else
            {
                Console.WriteLine("Something is wrong with logic!");
            }

        }

        /*
         * Demonstrates assertions, and that queries are correctly solved after appropriate assertion of facts and rules.
         */       
        private static void AssertingAFact()
        {
            KnowledgeBase kb = new KnowledgeBase("global", null);
            kb.IsTrueWrite("assert(person(plato)).");
            kb.IsTrueWrite("person(socratese).");
            kb.IsTrueWrite("assert((mortal(X) :- person(X))).");
            kb.IsTrueWrite("mortal(socratese).");
            kb.IsTrueWrite("assert(person(socratese)).");
            kb.IsTrueWrite("person(socratese).");
            kb.IsTrueWrite("mortal(socratese).");
            kb.IsTrueWrite("mortal(plato), person(socratese).");
         }

        /*
         * Experiments with the following configuration:
         * Global Prolog KB stored on a knowledge unit.
         * Prolog query applicability tests associated with content units.
         */
        private static void AddPrologQueriesToContentUnits()
        {
            IBlackboard blackboard = new Blackboard();
            ContentUnit cu = new ContentUnit();
            // cu.Metadata[] = 
        }

        private static void IsTrueWrite(this KnowledgeBase kb, string query)
        {
            if (kb.IsTrueParsed(query))
            {
                Console.WriteLine($"{query} is true");

            }
            else
            {
                Console.WriteLine($"{query} is false");
            }
        }

        public static bool IsTrueParsed(this KnowledgeBase kb, string query)
        {
            return kb.IsTrue(ISOPrologReader.Read(query));
        }

        /*
         * Test whether an interface can be used to make intellisence work with an ExpandoObject. 
         */
        public static void TestExpandoObjectWithInterface()
        {
            dynamic test = new ExpandoObject();
            // fixme: can't cast test to ITestInterface, even though it's dynamic. At runtime sees that test is ExpandObject and can't find conversion
            ITestInterface testCast = (ITestInterface)test;


            test.Foo = 3;
            test.Bar = new object();
            Console.WriteLine(test.Foo);
        }

         public interface ITestInterface {
            int Foo { get; set; }
            object Bar { get; set; }
        }
    }
}
