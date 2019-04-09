using System;
using System.Dynamic;
using System.Collections.Generic;
using Prolog;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;
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

        /*
         * Test using object composition to provide typed access to metadata and content slots.
         */
        /* public interface IMetadata_ID
        {
            string ContentUnitID { get; set; }
        }

        public interface IMetadata_TargetID
        {
            string TargetContentUnitID { get; set; }
        }

        public interface IMetadata_ApplicabilityTestResult
        {
            bool ApplTestResult { get; set; }
        }

        public interface IMetadata_UnityPrologApplTest
        {
            string ApplTest_UnityProlog { get; set; }
            object ApplTestBindings_UnityProlog { get; set; } 
        }

        public class Metadata_ID : IMetadata_ID
        {
            public string ContentUnitID { get; set; }
        }

        public class Metatdata_TargetID : IMetadata_TargetID
        {
            public string TargetContentUnitID { get; set; }
        }

        public class Metadata_ApplicabilityTestResult : IMetadata_ApplicabilityTestResult
        {
            public bool ApplTestResult { get; set; }
        }

        // Boo. This approach is going to end up with the problem of defining a combinatorial collection of classes which I want to avoid. 
        public class ContentUnit_ID_ApplTestResult : IMetadata_ID, IMetadata_ApplicabilityTestResult
        {
            private readonly Metadata_ID m_id = new Metadata_ID();
            private readonly Metadata_ApplicabilityTestResult m_applResult = new Metadata_ApplicabilityTestResult();

            public string ContentUnitID { get => m_id.ContentUnitID; set => m_id.ContentUnitID = value; }
            public bool ApplTestResult { get => m_applResult.ApplTestResult; set => m_applResult.ApplTestResult = value; }
        } */

        /*
         * Let's explore now whether extension methods combined with using a dictionary will do what we need
         */

        /*
         * IMetadata must have a Metadata dictionary. This is implemented by ContentUnit
         */
        public interface IMetadata
        {
            IDictionary<string, object> Metadata { get; }
        }

        /*
         * An IMetadata_ID has a string ID. Interface implemented by extension methods (trick for multiple inheritance).
         */
        public interface IMetadata_ID : IMetadata
        {
        }

        public static string GetID(this IMetadata_ID contentUnit)
        {
            return (string)contentUnit.Metadata[ContentUnitID];
        }

        public static void SetID(this IMetadata_ID contentUnit, string id)
        {
            contentUnit.Metadata[ContentUnitID] = id;
        }

        /*
         * An IMetatadata_TargetID has a string target ID. Interface implemented by extension methods (trick for multiple inheritance).
         */
        public interface IMetadata_TargetID : IMetadata 
        {
        }

        public static string GetTargetID(this IMetadata_TargetID contentUnit)
        {
            return (string)contentUnit.Metadata[TargetContentUnitID];
        }

        public static void SetTargetID(this IMetadata_TargetID contentUnit, string id)
        {
            contentUnit.Metadata[TargetContentUnitID] = id;
        }

        /*
         * An IMetadata_ApplicabilityTestResult has a boolean applicability test result (appl test might be prolog, or simpler if test). 
         * Interface implemented by extension methods (trick for multiple inheritance).       
         */
        public interface IMetadata_ApplicabilityTestResult : IMetadata
        {
        }

        public static bool GetApplTestResult(this IMetadata_ApplicabilityTestResult contentUnit)
        {
            return (bool)contentUnit.Metadata[ApplTestResult];
        }

        public static void SetApplTestResult(this IMetadata_ApplicabilityTestResult contentUnit, bool result)
        {
            contentUnit.Metadata[ApplTestResult] = result;
        }

        /*
         * An IMetadata_UnityPrologApplTest has a string applicability test (will be parsed into a query by UnityProlog) and a object representing a variable binding.
         * Interface implemented by extension methods (trick for multiple inheritance).        
         */
        public interface IMetadata_UnityPrologApplTest : IMetadata
        {
        }

        public static string GetApplTest_UnityProlog(this IMetadata_UnityPrologApplTest contentUnit)
        {
            return (string)contentUnit.Metadata[ApplTest_Prolog];
        }

        public static void SetApplTest_UnityProlog(this IMetadata_UnityPrologApplTest contentUnit, string test)
        {
            contentUnit.Metadata[ApplTest_Prolog] = test;
        }

        public static object GetApplTestBindings_UnityProlog(this IMetadata_UnityPrologApplTest contentUnit)
        {
            return contentUnit.Metadata[ApplTestBindings_Prolog];
        }

        public static void SetApplTestBindings_UnityProlog(this IMetadata_UnityPrologApplTest contentUnit, object binding)
        {
            contentUnit.Metadata[ApplTestBindings_Prolog] = binding;
        }

        /*
         * Defining a ContentUnit with an ID, applicability test result, and prolog applicability test. 
         */
        public class ContentUnit_ID_Prolog : ContentUnit, IMetadata_ID, IMetadata_ApplicabilityTestResult, IMetadata_UnityPrologApplTest
        {
            public ContentUnit_ID_Prolog() : base()
            {
            }

            public ContentUnit_ID_Prolog(ContentUnit_ID_Prolog contentUnit) : base(contentUnit)
            { 
            }
        }

        /*
         * Defining a ContentUnit with a target ID
         */
        public class ContentUnit_Choice_TargetID : ContentUnit, IMetadata_TargetID
        {
            public ContentUnit_Choice_TargetID() : base()
            {
            }

            public ContentUnit_Choice_TargetID(ContentUnit_Choice_TargetID contentUnit) : base(contentUnit)
            {
            }
        }

        /*
         * Yay, it looks like this approach to faking multiple inheritance for content units is going to work! 
         * fixme: for now let's get the simple CFG demo working, then move code base over to supporting this approach to multiple inheritance.        
         */
        public static void MultipleInheritanceTest()
        {
            ContentUnit_ID_Prolog contentUnit = new ContentUnit_ID_Prolog();

            contentUnit.SetID("start");
            contentUnit.SetApplTest_UnityProlog("available.");

            string id = contentUnit.GetID();

            IMetadata_ID contentUnit2 = new ContentUnit_ID_Prolog();
            contentUnit2.SetID("foo");

            ContentUnit_ID_Prolog contentUnit3 = new ContentUnit_ID_Prolog(contentUnit);
        }

    }
}
