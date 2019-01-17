using Xunit;
using Controllers;
using KnowledgeSources;
using CSACore;
using System.Collections.Generic;
using KnowledgeUnits;
using System.Linq;

namespace CSATests
{
    public class TestControllers
    {
        class KS_Dummy1 : KS_IDSelector
        {
            public KS_Dummy1(IBlackboard blackboard) : base(blackboard) { }
        }

        class KS_Dummy2 : KS_IDSelector
        {
            public KS_Dummy2(IBlackboard blackboard) : base(blackboard) { }
        }

        class Controller_PublicUpdateAgenda : Controller
        {
            public new void UpdateAgenda() => base.UpdateAgenda();

            protected override KnowledgeSource SelectKSForExecution() => throw new System.NotImplementedException();
        }

        class PriorityController_PublicMethods : PriorityController
        {
            public new void UpdateAgenda() => base.UpdateAgenda();

            public new KnowledgeSource SelectKSForExecution() => base.SelectKSForExecution();
        }

        [Fact]
        public void TestAddingKS()
        {
            // Maintaining the activeKSs is done through the abstract class Controller. Using the concrete class LexemeController for testing. 
            IController controller = new PriorityController();
            IBlackboard blackboard = new Blackboard();
            KnowledgeSource ks = new KS_IDSelector(blackboard);
            Assert.Equal(0, controller.ActiveKSs.Count);
            controller.AddKnowledgeSource(ks);
            Assert.Equal(1, controller.ActiveKSs.Count);
        }

        [Fact]
        public void TestRemovingKS()
        {
            IController controller = new PriorityController();
            IBlackboard blackboard = new Blackboard();
            KnowledgeSource ks = new KS_IDSelector(blackboard);
            controller.AddKnowledgeSource(ks);
            Assert.Equal(1, controller.ActiveKSs.Count);
            controller.RemoveKnowledgeSource(ks);
            Assert.Equal(0, controller.ActiveKSs.Count);
        }

        [Fact]
        public void TestUpdateAgenda_Controller()
        {
            Controller_PublicUpdateAgenda controller = new Controller_PublicUpdateAgenda();
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks = new KS_IDSelector(blackboard);
            controller.AddKnowledgeSource(ks);
            IUnit u = new U_IDQuery("foo");

            blackboard.AddUnit(u);
            Assert.Equal(0, controller.Agenda.Count);

            controller.UpdateAgenda();
            Assert.Equal(1, controller.Agenda.Count);

            blackboard.DeleteUnit(u);
            controller.UpdateAgenda();
            Assert.Equal(0, controller.Agenda.Count);
        }

        [Fact]
        public void TestSelectKSForExecution_PriorityController()
        {
            PriorityController_PublicMethods controller = new PriorityController_PublicMethods();
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks1 = new KS_IDSelector(blackboard);
            KS_IDSelector ks2 = new KS_IDSelector(blackboard);
            KS_IDSelector ks3 = new KS_IDSelector(blackboard);
            ks1.Properties[KS_PropertyNames.Priority] = 10;
            ks2.Properties[KS_PropertyNames.Priority] = 30;
            ks3.Properties[KS_PropertyNames.Priority] = 20;
            controller.AddKnowledgeSource(ks1);
            controller.AddKnowledgeSource(ks2);
            controller.AddKnowledgeSource(ks3);
            blackboard.AddUnit(new U_IDQuery("foo"));
            
            controller.UpdateAgenda();
            Assert.Equal(3, controller.Agenda.Count);

            KnowledgeSource ks = controller.SelectKSForExecution();
            Assert.Equal(30, ks.Properties[KS_PropertyNames.Priority]);
        }


        // fixme: finish this test after writing tests for agenda updating and SelectKSForExecution 
        [Fact]
        public void TestExecute_PriorityController()
        {
            PriorityController_PublicMethods controller = new PriorityController_PublicMethods();
            IBlackboard blackboard = new Blackboard();
            KS_IDSelector ks1 = new KS_IDSelector(blackboard);
            ks1.Properties[KS_PropertyNames.Priority] = 10;
            controller.AddKnowledgeSource(ks1);
            blackboard.AddUnit(new U_IDQuery("foo"));       
  
            List<ContentUnit> cuList = new List<ContentUnit>
            {
                new ContentUnit(),
                new ContentUnit(),
                new ContentUnit()
            };
            cuList[0].Metadata[CU_SlotNames.ContentUnitID] = "foo";
            cuList[1].Metadata[CU_SlotNames.ContentUnitID] = "bar";
            cuList[2].Metadata[CU_SlotNames.ContentUnitID] = "baz";

            foreach (IUnit u in cuList)
            {
                blackboard.AddUnit(u);
            }

            controller.Execute();

            // Four content units total (the original three plus a new selected one)
            ISet<IUnit> cuSet = blackboard.LookupUnits(ContentUnit.CU_Name);
            Assert.Equal(4, cuSet.Count);

            var selectedUnit = from unit in cuSet
                               where ((ContentUnit)unit).HasMetadataSlot(CU_SlotNames.SelectedContentUnit)
                               select unit;

            // Exactly 1 selected content unit
            int count = selectedUnit.Count();
            Assert.Equal(1, count);

            // The selected content unit is "foo" (matches the query request)
            Assert.True(((ContentUnit)selectedUnit.ElementAt(0)).Metadata[CU_SlotNames.ContentUnitID].Equals("foo"));
        }
    }

}
