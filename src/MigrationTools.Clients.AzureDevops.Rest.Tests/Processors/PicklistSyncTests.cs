using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts.Process;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class PicklistSyncTests
    {
        private static WorkItemPicklist List(string type, bool suggested, params string[] items) =>
            new WorkItemPicklist { Id = "list-1", Name = "picklist_src", Type = type, IsSuggested = suggested, Items = new List<string>(items) };

        private static WorkItemField PicklistField() => new WorkItemField { ReferenceName = "Custom.Colour", Type = "string", IsPicklist = true, PicklistId = "tgt-list" };

        [TestMethod(), TestCategory("L0")]
        public void TargetFieldMissing_ListAndFieldAreCreated()
        {
            Assert.AreEqual(PicklistFieldAction.CreateListAndField, PicklistSync.Plan(List("String", false, "Red"), null, null).Action);
        }

        [TestMethod(), TestCategory("L0")]
        public void TargetFieldIsText_IsLeftUnchanged()
        {
            var textField = new WorkItemField { ReferenceName = "Custom.Colour", Type = "string" };
            Assert.AreEqual(PicklistFieldAction.TargetFieldIsNotPicklist, PicklistSync.Plan(List("String", false, "Red"), textField, null).Action);
        }

        [TestMethod(), TestCategory("L0")]
        public void TargetListOfOtherType_IsLeftUnchanged()
        {
            Assert.AreEqual(PicklistFieldAction.TypeMismatch, PicklistSync.Plan(List("Integer", false, "1"), PicklistField(), List("String", false, "1")).Action);
        }

        [TestMethod(), TestCategory("L0")]
        public void SameValues_NoChange()
        {
            var plan = PicklistSync.Plan(List("String", false, "Red", "Green"), PicklistField(), List("string", false, "green", "Red"));
            Assert.AreEqual(PicklistFieldAction.NoChange, plan.Action);
            Assert.IsFalse(plan.IsSuggestedDiffers);
        }

        [TestMethod(), TestCategory("L0")]
        public void DifferentList_OnlyMissingValuesAreAdded_TargetValuesKept()
        {
            var plan = PicklistSync.Plan(List("String", true, "Red", "Blue"), PicklistField(), List("String", false, "Red", "Amber"));
            Assert.AreEqual(PicklistFieldAction.AddMissingItems, plan.Action);
            CollectionAssert.AreEqual(new[] { "Blue" }, plan.MissingItems);
            CollectionAssert.AreEqual(new[] { "Amber" }, plan.TargetOnlyItems);
            Assert.IsTrue(plan.IsSuggestedDiffers);

            var merged = PicklistSync.WithItemsAdded(List("String", false, "Red", "Amber"), plan.MissingItems);
            CollectionAssert.AreEqual(new[] { "Red", "Amber", "Blue" }, merged.Items);
            Assert.IsFalse(merged.IsSuggested, "The target's suggested setting is kept.");
            Assert.AreEqual("list-1", merged.Id);
        }

        [TestMethod(), TestCategory("L0")]
        public void NewTargetList_KeepsValuesTypeAndSuggested_WithFreshName()
        {
            var source = List("Integer", true, "1", "2", "3");
            var created = PicklistSync.NewTargetList(source);
            Assert.IsNull(created.Id);
            Assert.AreNotEqual(source.Name, created.Name);
            StringAssert.StartsWith(created.Name, "picklist_");
            Assert.AreEqual("Integer", created.Type);
            Assert.IsTrue(created.IsSuggested);
            CollectionAssert.AreEqual(source.Items, created.Items);
            Assert.AreNotSame(source.Items, created.Items);
        }

        [TestMethod(), TestCategory("L0")]
        public void WorkItemTypePicklistField_IsSentWithoutAllowedValues_KeepingItsDefault()
        {
            var source = new WorkItemTypeField { ReferenceName = "Custom.Colour", DefaultValue = "Green", AllowedValues = new[] { "Red", "Green" } };
            var sent = PicklistSync.WithoutListValues(source, isPicklist: true);
            Assert.IsNull(sent.AllowedValues);
            Assert.AreEqual("Green", sent.DefaultValue);
            Assert.IsNotNull(source.AllowedValues, "The source model is not changed.");
            Assert.AreSame(source, PicklistSync.WithoutListValues(source, isPicklist: false));
        }

        [TestMethod(), TestCategory("L0")]
        public void TargetFieldOfOtherType_DefaultIsNotSent()
        {
            var source = new WorkItemTypeField { ReferenceName = "Custom.Size", DefaultValue = "M", AllowedValues = new[] { "S", "M" } };
            var sent = PicklistSync.WithoutListValues(source, isPicklist: true, targetTypeDiffers: true);
            Assert.IsNull(sent.DefaultValue);
            Assert.IsNull(sent.AllowedValues);
            Assert.AreEqual("M", source.DefaultValue);
        }

        [TestMethod(), TestCategory("L0")]
        public void TypesDiffer_ComparesOrgFieldTypes()
        {
            Assert.IsTrue(PicklistSync.TypesDiffer(new WorkItemField { Type = "string" }, new WorkItemField { Type = "integer" }));
            Assert.IsFalse(PicklistSync.TypesDiffer(new WorkItemField { Type = "string" }, new WorkItemField { Type = "String" }));
            Assert.IsFalse(PicklistSync.TypesDiffer(new WorkItemField { Type = "string" }, null));
        }

        [TestMethod(), TestCategory("L0")]
        public void SharedList_StaleReadAfterWrite_KeepsTheWrittenValues()
        {
            var written = new[] { "X", "Y" };
            var staleRead = new[] { "X" };
            var merged = PicklistSync.Union(written, staleRead);
            CollectionAssert.AreEqual(new[] { "X", "Y" }, merged);

            var plan = PicklistSync.Plan(List("String", false, "X", "Z"), PicklistField(), List("String", false, merged.ToArray()));
            CollectionAssert.AreEqual(new[] { "Z" }, plan.MissingItems);
            CollectionAssert.AreEqual(new[] { "X", "Y", "Z" }, PicklistSync.WithItemsAdded(List("String", false, merged.ToArray()), plan.MissingItems).Items);
        }

        [TestMethod(), TestCategory("L0")]
        public void SharedList_RejectedWrite_IsNotRecorded_AndTheNextFieldStillAddsItsValues()
        {
            var written = new Dictionary<string, WorkItemPicklist>();
            PicklistSync.RecordWrite(written, List("String", false, "X", "Y"), accepted: true);
            PicklistSync.RecordWrite(written, List("String", false, "X", "Y", "Z"), accepted: false);
            CollectionAssert.AreEqual(new[] { "X", "Y" }, written["list-1"].Items, "The earlier accepted write stays.");

            var read = PicklistSync.Union(written["list-1"].Items, new[] { "X" });
            var plan = PicklistSync.Plan(List("String", false, "X", "Z"), PicklistField(), List("String", false, read.ToArray()));
            CollectionAssert.AreEqual(new[] { "Z" }, plan.MissingItems, "Z was never saved, so the next field adds it again.");
        }

        [TestMethod(), TestCategory("L0")]
        public void PicklistField_IsCreatedLinkedToItsList()
        {
            var field = new WorkItemField { Name = "Colour", ReferenceName = "Custom.Colour", Type = "string", IsPicklist = true, PicklistId = "src-list" };
            var clone = field.CloneAsNew();
            clone.PicklistId = "tgt-list";
            var json = JsonConvert.SerializeObject(clone.ToJson(), new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
            });
            StringAssert.Contains(json, "\"picklistId\":\"tgt-list\"");
            StringAssert.Contains(json, "\"isPicklist\":true");
        }
    }
}
