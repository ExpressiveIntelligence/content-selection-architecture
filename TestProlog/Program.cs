using System;
using System.Dynamic;
using Prolog;
using CSA.Core;
using UnityEngine;

/* 
 * A test rig for testing integration of UnityProlog (https://github.com/ianhorswill/UnityProlog) with CSA.
 * */
namespace TestProlog
{
    static class Program
    {
        public static void Main(string[] args)
        {
            //CreatePrologKBAFromFileAndQuery();
            //AssertingAFact();
            // TestExpandoObjectWithInterface();
            // TestVariableBinding();
            // QueryWithNoArgument();
            //KnowledgeBase kb = new KnowledgeBase("global", null);
            //PrintTypeVariable<KnowledgeBase>(kb);
            // TestIncrement();   
            TestBoolConstant();
        }

        //Testing whether true and false are handled as prolog queries
        private static void TestBoolConstant()
        {
            KnowledgeBase kb = new KnowledgeBase("Global", null);
            kb.Consult("PrologTest.prolog");
            kb.IsTrueWrite("true.");
            kb.IsTrueWrite("false.");
        }

        /*
         * fixme: Queries which involve chaining bindings generate an error. It looks like in the guts of SolveFor() there 
         * are recursive calls which call GameObject.SolveFor(). So using this basic feature of prolog requires 
         * the UnityProlog KB to be embeded in a game object. To debug this I should just develop a TextChoices unity 
         * demo with the KB stored on a GameObject.        
         */
        private static void TestIncrement()
        {
            KnowledgeBase kb = new KnowledgeBase("Global", null);
            kb.Consult("PrologTestWithError.prolog");
            kb.IsTrueWrite("triesGreaterThan(1).");
            //kb.IsTrueWrite("tries(X), X > 1.");
            //kb.IsTrueWrite("assert(triedDoor).");
            //kb.IsTrueWrite("incrementTries.");
            //kb.IsTrueWrite("tries(X), X > 1.");
            //kb.IsTrueWrite("assert(triedDoor).");
            //kb.IsTrueWrite("tries(x), X > 1.");
        }


        // Not related to test prolog. Scratch test of getting the name of the type referenced by a type variable. 
        private static void PrintTypeVariable<T>(T t)
        {
            Console.WriteLine(typeof(T));
            Console.WriteLine(t.GetType().FullName);

        }
        private static void CreatePrologKBAFromFileAndQuery()
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
         * Test that SolveFor() can be used to bind variables.
         * Findings:
         * If the variable is bound to a simple value it's a Symbol.
         * If the variable is bound to a composite value (like a functor) it's a Structure.
         * If the query doesn't succeed the returned variable is null.
         * If the query succeeds but the variable isn't bound in the query, the returned variable is a gensym.
         * If you need to return multiple values with a query, you can create a predicate on the prolog side which binds multiple values in a functor and unify that with the return variable. 
         */
        private static void TestVariableBinding()
        {
            KnowledgeBase kb = new KnowledgeBase("Global", null);
            kb.Consult("PrologTest.prolog");
            var x = kb.SolveForParsed("X:test(X).");
            Console.WriteLine("The value of x is " + x);
            Console.WriteLine("The type of x is " + x.GetType());
            //Console.WriteLine("The type of x is " + ((Structure)x).Argument(1));
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
         * Testing queries that have no arguments. 
         */
        private static void QueryWithNoArgument()
        {
            KnowledgeBase kb = new KnowledgeBase("global", null);
            kb.IsTrueWrite("assert((test :- foo)).");
            kb.IsTrueWrite("assert(foo).");
            kb.IsTrueWrite("retract(foo).");
            kb.IsTrueWrite("test.");
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

        public static object SolveForParsed(this KnowledgeBase kb, string variableAndConstraint)
        {
            if (!(ISOPrologReader.Read(variableAndConstraint) is Structure colonExpression) || !colonExpression.IsFunctor(Symbol.Colon, 2))
                throw new ArgumentException("Argument to SolveFor(string) must be of the form Var:Goal.");
            return kb.SolveFor((LogicVariable)colonExpression.Argument(0), colonExpression.Argument(1), null, false);

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

        public interface ITestInterface
        {
            int Foo { get; set; }
            object Bar { get; set; }
        }
    }
}
