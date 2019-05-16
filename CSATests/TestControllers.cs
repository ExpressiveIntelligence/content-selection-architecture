using System;
using Xunit;
using CSA.Controllers;
using CSA.KnowledgeSources;
using CSA.Core;
using System.Collections.Generic;
using CSA.KnowledgeUnits;
using System.Linq;
using static CSA.KnowledgeSources.KSProps;

#pragma warning disable CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.CUSlots;
#pragma warning restore CS0618 // Type or member is obsolete

namespace CSA.Tests
{
    public class TestControllers
    {
        [Obsolete("Reactive controller infrastructure is obsolete.")]
        class KS_Dummy1 : KS_ReactiveIDSelector
        {
            public KS_Dummy1(IBlackboard blackboard) : base(blackboard) { }
        }

        [Obsolete("Reactive controller infrastructure is obsolete.")]
        class KS_Dummy2 : KS_ReactiveIDSelector
        {
            public KS_Dummy2(IBlackboard blackboard) : base(blackboard) { }
        }

        [Obsolete("Reactive controller infrastructure is obsolete.")]
        class Controller_PublicUpdateAgenda : ReactiveController
        {
            public new void UpdateAgenda() => base.UpdateAgenda();

            protected override IKnowledgeSourceActivation SelectKSForExecution() => throw new System.NotImplementedException();
        }

        [Obsolete("Reactive controller infrastructure is obsolete.")]
        class PriorityController_PublicMethods : ReactivePriorityController
        {
            public new void UpdateAgenda() => base.UpdateAgenda();

            public new IKnowledgeSourceActivation SelectKSForExecution() => base.SelectKSForExecution();
        }

        // [Fact]
        [Obsolete("Not performing this unit test. Reactive controller infrastructure is obsolete.")]
        /*public*/
        void TestAddingKS()
        {
            // Maintaining the activeKSs is done through the abstract class Controller. Using the concrete class LexemeController for testing. 
            IReactiveController controller = new ReactivePriorityController();
            IBlackboard blackboard = new Blackboard();
            ReactiveKnowledgeSource ks = new KS_ReactiveIDSelector(blackboard);
            Assert.Equal(0, controller.ActiveKSs.Count);
            controller.AddKnowledgeSource(ks);
            Assert.Equal(1, controller.ActiveKSs.Count);
        }

        // [Fact]
        [Obsolete("Not performing this unit test. Reactive controller infrastructure is obsolete.")]
        /*public*/
        void TestRemovingKS()
        {
            IReactiveController controller = new ReactivePriorityController();
            IBlackboard blackboard = new Blackboard();
            ReactiveKnowledgeSource ks = new KS_ReactiveIDSelector(blackboard);
            controller.AddKnowledgeSource(ks);
            Assert.Equal(1, controller.ActiveKSs.Count);
            controller.RemoveKnowledgeSource(ks);
            Assert.Equal(0, controller.ActiveKSs.Count);
        }

        // [Fact]
        [Obsolete("Not performing this unit test. Reactive controller infrastructure is obsolete.")]
        /*public*/
        void TestUpdateAgenda_Controller()
        {
            Controller_PublicUpdateAgenda controller = new Controller_PublicUpdateAgenda();
            IBlackboard blackboard = new Blackboard();
            KS_ReactiveIDSelector ks = new KS_ReactiveIDSelector(blackboard);
            controller.AddKnowledgeSource(ks);
            IUnit u = new U_IDSelectRequest("foo");

            blackboard.AddUnit(u);
            Assert.Equal(0, controller.Agenda.Count);

            controller.UpdateAgenda();
            Assert.Equal(1, controller.Agenda.Count);

            blackboard.RemoveUnit(u);
            controller.UpdateAgenda();
            Assert.Equal(0, controller.Agenda.Count);
        }

        // [Fact]
        [Obsolete("Not performing this unit test. Reactive controller infrastructure is obsolete.")]
        /*public*/
        void TestSelectKSForExecution_PriorityController()
        {
            PriorityController_PublicMethods controller = new PriorityController_PublicMethods();
            IBlackboard blackboard = new Blackboard();
            KS_ReactiveIDSelector ks1 = new KS_ReactiveIDSelector(blackboard);
            KS_ReactiveIDSelector ks2 = new KS_ReactiveIDSelector(blackboard);
            KS_ReactiveIDSelector ks3 = new KS_ReactiveIDSelector(blackboard);
            ks1.Properties[Priority] = 10;
            ks2.Properties[Priority] = 30;
            ks3.Properties[Priority] = 20;
            controller.AddKnowledgeSource(ks1);
            controller.AddKnowledgeSource(ks2);
            controller.AddKnowledgeSource(ks3);
            blackboard.AddUnit(new U_IDSelectRequest("foo"));

            controller.UpdateAgenda();
            Assert.Equal(3, controller.Agenda.Count);

            IKnowledgeSourceActivation KSA = controller.SelectKSForExecution();
            Assert.Equal(30, KSA.Properties[KSProps.Priority]);
        }

        // [Fact]
        [Obsolete("Not performing this unit test. Reactive controller infrastructure is obsolete.")]
        /*public*/ 
        void TestExecute_PriorityController()
        {
            PriorityController_PublicMethods controller = new PriorityController_PublicMethods();
            IBlackboard blackboard = new Blackboard();
            KS_ReactiveIDSelector ks1 = new KS_ReactiveIDSelector(blackboard);
            ks1.Properties[Priority] = 10;
            controller.AddKnowledgeSource(ks1);
            blackboard.AddUnit(new U_IDSelectRequest("foo"));       
  
            List<ContentUnit> cuList = new List<ContentUnit>
            {
                new ContentUnit(),
                new ContentUnit(),
                new ContentUnit()
            };
            cuList[0].Metadata[ContentUnitID] = "foo";
            cuList[1].Metadata[ContentUnitID] = "bar";
            cuList[2].Metadata[ContentUnitID] = "baz";

            foreach (IUnit u in cuList)
            {
                blackboard.AddUnit(u);
            }

            controller.Execute();

            // Four content units total (the original three plus a new selected one)
            ISet<ContentUnit> cuSet = blackboard.LookupUnits<ContentUnit>();
            Assert.Equal(4, cuSet.Count);

            var selectedUnit = from unit in cuSet
                               where unit.HasMetadataSlot(SelectedContentUnit)
                               select unit;

            // Exactly 1 selected content unit
            int count = selectedUnit.Count();
            Assert.Equal(1, count);

            // The selected content unit is "foo" (matches the query request)
            Assert.True(selectedUnit.ElementAt(0).Metadata[ContentUnitID].Equals("foo"));

            // Since we only added one KS to controller, and there was only one matching blackboard pattern, after execution the agenda should be empty
            Assert.Empty(controller.Agenda);
        }

        // fixme: eventually make this a theory 
        [Fact]
        public void TestAddKnowledgeSource_ScheduledSequenceController()
        {
            IBlackboard blackboard = new Blackboard();
            IScheduledKnowledgeSource ks1 = new KS_KC_ScheduledFilterSelector(blackboard, "pool1");
            IScheduledKnowledgeSource ks2 = new KS_KC_ScheduledIDSelector(blackboard, "pool1", "pool2");
            IScheduledKnowledgeSource ks3 = new KS_KC_ScheduledUniformDistributionSelector(blackboard, "pool2", "pool3", 1);
            ScheduledSequenceController controller = new ScheduledSequenceController();

            controller.AddKnowledgeSource(ks1);
            controller.AddKnowledgeSource(ks2);
            controller.AddKnowledgeSource(ks3);

            Assert.True(controller.RegisteredKnowledgeSource(ks1));
            Assert.True(controller.RegisteredKnowledgeSource(ks2));
            Assert.True(controller.RegisteredKnowledgeSource(ks3));
        }

        // fixme: eventually make this a theory 
        [Fact]
        public void TestRemoveKnowledgeSource_ScheduledSequenceController()
        {
            IBlackboard blackboard = new Blackboard();
            IScheduledKnowledgeSource ks1 = new KS_KC_ScheduledFilterSelector(blackboard, "pool1");
            IScheduledKnowledgeSource ks2 = new KS_KC_ScheduledIDSelector(blackboard, "pool1", "pool2");
            IScheduledKnowledgeSource ks3 = new KS_KC_ScheduledUniformDistributionSelector(blackboard, "pool2", "pool3", 1);
            ScheduledSequenceController controller = new ScheduledSequenceController();

            controller.AddKnowledgeSource(ks1);
            controller.AddKnowledgeSource(ks2);
            controller.AddKnowledgeSource(ks3);

            Assert.True(controller.RegisteredKnowledgeSource(ks1));
            Assert.True(controller.RegisteredKnowledgeSource(ks2));
            Assert.True(controller.RegisteredKnowledgeSource(ks3));

            controller.RemoveKnowledgeSource(ks3);
            Assert.True(controller.RegisteredKnowledgeSource(ks1));
            Assert.True(controller.RegisteredKnowledgeSource(ks2));
            Assert.False(controller.RegisteredKnowledgeSource(ks3));

            controller.RemoveKnowledgeSource(ks2);
            Assert.True(controller.RegisteredKnowledgeSource(ks1));
            Assert.False(controller.RegisteredKnowledgeSource(ks2));
            Assert.False(controller.RegisteredKnowledgeSource(ks3));

            controller.RemoveKnowledgeSource(ks1);
            Assert.False(controller.RegisteredKnowledgeSource(ks1));
            Assert.False(controller.RegisteredKnowledgeSource(ks2));
            Assert.False(controller.RegisteredKnowledgeSource(ks3));

        }

        // fixme: eventually make this a theory 
        [Fact]
        public void TestExecute_ScheduledSequenceController()
        {
            string id = "id1";
            string pool1 = "pool1";
            string pool2 = "pool2";

            IBlackboard blackboard = new Blackboard();
            IScheduledKnowledgeSource ks1 = new KS_KC_ScheduledFilterSelector(blackboard, pool1, (Unit u) => u.HasComponent<KC_UnitID>());
            IScheduledKnowledgeSource ks2 = new KS_KC_ScheduledIDSelector(blackboard, pool1, pool2);
            ScheduledSequenceController controller = new ScheduledSequenceController();

            controller.AddKnowledgeSource(ks1);
            controller.AddKnowledgeSource(ks2);

            Unit unit = new Unit();
            unit.AddComponent(new KC_UnitID(id));
            blackboard.AddUnit(unit);

            Unit req = new Unit();
            req.AddComponent(new KC_IDSelectionRequest(id));
            blackboard.AddUnit(req);
            req.SetActiveRequest(true);

            controller.Execute();

            var pool1Units = from u in blackboard.LookupUnits<Unit>()
                             where u.HasComponent<KC_ContentPool>()
                             where u.ContentPoolEquals(pool1)
                             select u;

            var pool2Units = from u in blackboard.LookupUnits<Unit>()
                             where u.HasComponent<KC_ContentPool>()
                             where u.ContentPoolEquals(pool2)
                             select u;

            int pool1Count = pool1Units.Count();
            int pool2Count = pool2Units.Count();
            Assert.Equal(1, pool1Count);
            Assert.Equal(1, pool2Count);

            Assert.True(pool1Units.First().UnitIDEquals(id));
            Assert.True(pool2Units.First().UnitIDEquals(id));
        }
    }
}
