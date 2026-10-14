using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts.Process;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MigrationTools.DataContracts.Tests
{
    [TestClass()]
    public class ProcessDefinitionContractTests
    {
        // Same settings the AzureDevOpsEndpoint uses to send definitions.
        private static JObject Body(RestApiDefinition definition)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            };
            return JObject.Parse(JsonConvert.SerializeObject(definition.ToJson(), settings));
        }

        private static WorkItemControl Control(string id, string type) => new WorkItemControl { Id = id, ControlType = type };

        [TestMethod(), TestCategory("L0")]
        public void GroupCloneAsNew_KeepsOnlyHtmlControls()
        {
            var group = new WorkItemGroup
            {
                Label = "G",
                Controls = new List<WorkItemControl> { Control("Custom.F", "FieldControl"), Control("Custom.Html", "HtmlFieldControl") }
            };

            var clone = group.CloneAsNew();

            CollectionAssert.AreEqual(new[] { "Custom.Html" }, clone.Controls.Select(c => c.Id).ToArray());
        }

        [TestMethod(), TestCategory("L0")]
        public void GroupCloneAsNew_WithoutHtmlControls_SendsNoControls()
        {
            var group = new WorkItemGroup { Label = "G", Controls = new List<WorkItemControl> { Control("Custom.F", "FieldControl") } };

            Assert.IsNull(group.CloneAsNew().Controls);
        }

        [TestMethod(), TestCategory("L0")]
        public void GroupCloneAsNew_WithNullControls_DoesNotThrow()
        {
            Assert.IsNull(new WorkItemGroup { Label = "G" }.CloneAsNew().Controls);
        }

        [TestMethod(), TestCategory("L0")]
        [DataRow("custom", true)]
        [DataRow("system", false)]
        [DataRow("inherited", false)]
        [DataRow(null, false)]
        public void Rule_OnlyCustomRulesAreWritable(string customizationType, bool expected)
        {
            Assert.AreEqual(expected, new WorkItemRule { CustomizationType = customizationType }.IsCustom);
        }

        [TestMethod(), TestCategory("L0")]
        public void Rule_Body_CarriesNameConditionsAndActions()
        {
            var rule = new WorkItemRule
            {
                Id = "r1",
                Name = "amtpd rule",
                CustomizationType = "custom",
                Conditions = new List<WorkItemRule.RuleCondition> { new WorkItemRule.RuleCondition { ConditionType = "when", Field = "System.State", Value = "Active" } },
                Actions = new List<WorkItemRule.RuleAction> { new WorkItemRule.RuleAction { ActionType = "makeRequired", TargetField = "Custom.F" } },
            };

            var body = Body(rule);

            Assert.AreEqual("amtpd rule", (string)body["name"]);
            Assert.AreEqual("when", (string)body["conditions"][0]["conditionType"]);
            Assert.AreEqual("makeRequired", (string)body["actions"][0]["actionType"]);
            Assert.IsNull(body["customizationType"]);
        }

        [TestMethod(), TestCategory("L0")]
        public void WorkItemType_Create_InheritsFromParent()
        {
            var source = new WorkItemType { ReferenceName = "src.Bug", Name = "Bug", Inherits = "Microsoft.VSTS.WorkItemTypes.Bug", Color = "CC293D", Icon = "icon_insect", Customization = "inherited" };
            var target = source.CloneAsNew();
            target.ResetObject();

            var body = Body(target);

            Assert.AreEqual("Microsoft.VSTS.WorkItemTypes.Bug", (string)body["inheritsFrom"]);
            Assert.AreEqual("icon_insect", (string)body["icon"]);
            Assert.AreEqual("CC293D", (string)body["color"]);
            Assert.IsNull(body["name"]);
            Assert.IsNull(body["referenceName"]);
        }

        [TestMethod(), TestCategory("L0")]
        public void WorkItemType_CreateCustom_SendsName()
        {
            var target = new WorkItemType { ReferenceName = "src.Risk", Name = "Risk", Color = "000000", Icon = "icon_list" }.CloneAsNew();
            target.ResetObject();

            var body = Body(target);

            Assert.AreEqual("Risk", (string)body["name"]);
            Assert.IsNull(body["inheritsFrom"]);
        }

        [TestMethod(), TestCategory("L0")]
        public void WorkItemType_Update_SendsOnlyUpdatableFields()
        {
            var body = Body(new WorkItemType { ReferenceName = "tgt.Bug", Name = "Bug", Inherits = "Microsoft.VSTS.WorkItemTypes.Bug", Color = "CC293D", Icon = "icon_insect", Customization = "inherited" });

            CollectionAssert.AreEquivalent(new[] { "color", "icon", "isDisabled" }, body.Properties().Select(p => p.Name).ToArray());
        }

        [TestMethod(), TestCategory("L0")]
        [DataRow("system", true)]
        [DataRow("inherited", false)]
        [DataRow("custom", false)]
        public void WorkItemType_IsSystem(string customization, bool expected)
        {
            Assert.AreEqual(expected, new WorkItemType { Customization = customization }.IsSystem);
        }

        [TestMethod(), TestCategory("L0")]
        public void Behavior_Create_SendsInheritsAsReferenceName()
        {
            var target = new WorkItemBehavior
            {
                ReferenceName = "Custom.abc",
                Name = "Initiatives",
                Color = "AA00AA",
                Customization = "custom",
                Inherits = new WorkItemBehavior.WorkItemBehaviorRef { BehaviorRefName = "System.PortfolioBacklogBehavior" }
            }.CloneAsNew();
            target.ResetObject();

            var body = Body(target);

            Assert.AreEqual("System.PortfolioBacklogBehavior", (string)body["inherits"]);
            Assert.AreEqual("Initiatives", (string)body["name"]);
            Assert.IsNull(body["customization"]);
        }

        [TestMethod(), TestCategory("L0")]
        public void Behavior_Update_KeepsTargetReferenceName()
        {
            var target = new WorkItemBehavior { ReferenceName = "Custom.target", Name = "Initiatives", Customization = "custom" };

            target.UpdateWithExisting(new WorkItemBehavior { ReferenceName = "Custom.source", Name = "Initiatives", Color = "BB00BB", Customization = "custom" });

            Assert.AreEqual("Custom.target", target.Id);
            CollectionAssert.AreEquivalent(new[] { "name", "color" }, Body(target).Properties().Select(p => p.Name).ToArray());
        }

        [TestMethod(), TestCategory("L0")]
        [DataRow("inherited", true, false)]
        [DataRow("system", false, true)]
        [DataRow("custom", false, false)]
        public void Behavior_Customization(string customization, bool inherited, bool system)
        {
            var behavior = new WorkItemBehavior { Customization = customization };

            Assert.AreEqual(inherited, behavior.IsInherited);
            Assert.AreEqual(system, behavior.IsSystem);
        }

        [TestMethod(), TestCategory("L0")]
        public void WorkItemTypeBehavior_Create_KeepsBehaviorId()
        {
            var target = new WorkItemTypeBehavior { Id = "System.RequirementBacklogBehavior", IsDefault = true }.CloneAsNew();
            target.ResetObject();

            var body = Body(target);

            Assert.AreEqual("System.RequirementBacklogBehavior", (string)body["behavior"]["id"]);
            Assert.IsTrue((bool)body["isDefault"]);
        }

        [TestMethod(), TestCategory("L0")]
        [DataRow("system", true)]
        [DataRow("inherited", false)]
        [DataRow("custom", false)]
        public void WorkItemTypeField_IsSystem(string customization, bool expected)
        {
            Assert.AreEqual(expected, new WorkItemTypeField { Customization = customization }.IsSystem);
        }
    }
}
