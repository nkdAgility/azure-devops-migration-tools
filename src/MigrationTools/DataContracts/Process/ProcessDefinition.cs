using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MigrationTools.DataContracts.Process
{
    [ApiPath("work/processes", IncludeProject: false, IncludeTrailingSlash: false, ApiVersion: "6.0-preview.2", UpdateVerb = HttpVerbs.Patch)]
    [ApiName("Process Definitions")]
    public partial class ProcessDefinition : ProcessDefBase, ISynchronizeable<ProcessDefinition>
    {
        public override string Id
        {
            get => TypeId;
            set => TypeId = value;
        }
        public string TypeId { get; set; }
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; }
        public string ReferenceName { get; set; }
        public string CustomizationType { get; set; }
        public string Description { get; set; }
        public string ParentProcessTypeId { get; set; }

        public ProcessDefinition CloneAsNew()
        {
            var proc = new ProcessDefinition();
            proc.ParentProcessTypeId = this.ParentProcessTypeId;
            proc.IsDefault = this.IsDefault;
            proc.IsEnabled = this.IsEnabled;
            proc.Description = this.Description;
            proc.CustomizationType = this.CustomizationType;
            proc.Name = this.Name;
            proc.ReferenceName = this.ReferenceName;
            return proc;
        }

        public void UpdateWithExisting(ProcessDefinition existing)
        {
            this.IsDefault = existing.IsDefault;
            this.IsEnabled = existing.IsEnabled;
            this.Description = existing.Description;
        }
    }

    [ApiPath("work/processes/{0}/workitemtypes", IncludeProject: false, IncludeTrailingSlash: false, UpdateVerb = HttpVerbs.Patch)]
    [ApiName("Work Item Type")]
    public partial class WorkItemType : ProcessDefBase, ISynchronizeable<WorkItemType>
    {
        public override string Id
        {
            get => ReferenceName;
            set => ReferenceName = value;
        }
        public bool IsDisabled { get; set; }
        public string ReferenceName { get; set; }
        public string Customization { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public List<WorkItemState> States { get; set; }

        public WorkItemType CloneAsNew()
        {
            var wit = new WorkItemType();
            wit.IsDisabled = this.IsDisabled;
            wit.Name = this.Name;
            wit.ReferenceName = this.ReferenceName;
            wit.Description = this.Description;
            wit.Color = this.Color;
            wit.Name = this.Name;
            wit.Icon = this.Icon;
            return wit;
        }

        public void UpdateWithExisting(WorkItemType existing)
        {
            this.IsDisabled = existing.IsDisabled;
            this.Name = existing.Name;
            // Keep the old ProcModel name prefix but change the name (after the period)
            this.ReferenceName = (ReferenceName??"").Split('.').FirstOrDefault() + "." + (existing.ReferenceName??"").Split('.').Last();
            this.Description = existing.Description;
            this.Color = existing.Color;
            this.Name = existing.Name;
            this.Icon = existing.Icon;
        }
    }

    [ApiPath("work/processes/{0}/workitemtypes/{1}/states",
        IncludeProject: false, IncludeTrailingSlash: false,
        IncludeIdOnUpdate: true, UpdateVerb = HttpVerbs.Patch)]
    [ApiName("Work Item State")]
    public partial class WorkItemState : ProcessDefBase, ISynchronizeable<WorkItemState>
    {
        public override string Id { get; set; }
        public string StateId { get; set; }
        public string Color { get; set; }
        public string StateCategory { get; set; }
        public int Order { get; set; }
        public string CustomizationType { get; set; }

        public WorkItemState CloneAsNew()
        {
            return new WorkItemState()
            {
                Name = Name,
                Color = Color,
                StateCategory = StateCategory,
                Order = Order,
                CustomizationType = CustomizationType
            };
        }

        public void UpdateWithExisting(WorkItemState existing)
        {
            Color = existing.Color;
            StateCategory = existing.StateCategory;
            Order = existing.Order;
            CustomizationType = existing.CustomizationType;
        }
    }
    [ApiPath("work/processes/{0}/workitemtypes/{1}/rules", IncludeProject: false, IncludeTrailingSlash: false)]
    [ApiName("Work Item State")]
    public partial class WorkItemRule : ProcessDefBase, ISynchronizeable<WorkItemRule>
    {
        public class RuleCondition { public string ConditionType { get; set; } public string Field { get; set; } public string Value { get; set; } }
        public class RuleAction { public string ActionType { get; set; } public string TargetField { get; set; } public string Value { get; set; } }
        public List<RuleCondition> Conditions { get; set; }
        public List<RuleAction> Actions { get; set; }
        public string CustomizationType { get; set; }

        public WorkItemRule CloneAsNew()
        {
            return new WorkItemRule()
            {
                Name = Name,
                CustomizationType = CustomizationType,
                Actions = Actions,
                Conditions = Conditions
            };
        }

        public void UpdateWithExisting(WorkItemRule existing)
        {
            Name = existing.Name;
            CustomizationType = existing.CustomizationType;
            Actions = existing.Actions;
            Conditions = existing.Conditions;
        }
    }
    [ApiPath("wit/fields", IncludeProject: false, IncludeTrailingSlash: false, UpdateVerb = HttpVerbs.Patch)]
    [ApiName("Work Item Field")]
    public partial class WorkItemField : ProcessDefBase, ISynchronizeable<WorkItemField>
    {
        [JsonIgnore]
        public override string Id
        {
            get => ReferenceName;
        }
        public class SupportedOperation
        {
            public string ReferenceName { get; set; }
            public string Name { get; set; }
        }
        public string ReferenceName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Usage { get; set; }
        public bool ReadOnly { get; set; }
        public bool CanSortBy { get; set; }
        public bool IsQueryable { get; set; }
        public List<SupportedOperation> SupportedOperations { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPicklist { get; set; }
        public bool IsPicklistSuggested { get; set; }

        public WorkItemField CloneAsNew()
        {
            return new WorkItemField()
            {
                Name = Name,
                ReferenceName = ReferenceName,
                Description = Description,
                Type = Type,
                Usage = Usage,
                ReadOnly = ReadOnly,
                CanSortBy = CanSortBy,
                IsQueryable = IsQueryable,
                IsIdentity = IsIdentity,
                IsPicklist = IsPicklist,
                IsPicklistSuggested = IsPicklistSuggested,
                SupportedOperations = SupportedOperations
            };
        }

        public void UpdateWithExisting(WorkItemField existing)
        {
            Name = existing.Name;
            ReferenceName = existing.ReferenceName;
            Description = existing.Description;
            Type = existing.Type;
            Usage = existing.Usage;
            ReadOnly = existing.ReadOnly;
            CanSortBy = existing.CanSortBy;
            IsQueryable = existing.IsQueryable;
            IsIdentity = existing.IsIdentity;
            IsPicklist = existing.IsPicklist;
            IsPicklistSuggested = existing.IsPicklistSuggested;
            SupportedOperations = existing.SupportedOperations;
        }
    }

    [ApiPath("work/processes/{0}/workitemtypes/{1}/fields?$expand=All", IncludeProject: false, IncludeTrailingSlash: false, UpdateVerb = HttpVerbs.Patch)]
    [ApiName("Work Item Type Field")]
    public partial class WorkItemTypeField : ProcessDefBase, ISynchronizeable<WorkItemTypeField>
    {
        [JsonIgnore]
        public override string Id
        {
            get => ReferenceName;
        }
        public string ReferenceName { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public bool AllowGroups { get; set; }
        public bool Required { get; set; }
        public bool ReadOnly { get; set; }
        public string Customization { get; set; }
        public string[] AllowedValues { get; set; }

        public override dynamic ToJson()
        {
            switch (ReferenceName)
            {

                case "System.AreaId":
                case "System.IterationId":
                    return new
                    {
                        ReferenceName,
                        Description,
                        DefaultValue,
                        AllowedValues,
                        AllowGroups,
                        ReadOnly
                    };
                default:
                    return new
                    {
                        ReferenceName,
                        Type,
                        Description,
                        DefaultValue,
                        AllowedValues,
                        AllowGroups,
                        Required,
                        ReadOnly
                    };
            }
        }

        public WorkItemTypeField CloneAsNew()
        {
            return new WorkItemTypeField()
            {
                Name = Name,
                ReferenceName = ReferenceName,
                Type = Type,
                Description = Description,
                DefaultValue = DefaultValue,
                AllowGroups = AllowGroups,
                Required = Required,
                ReadOnly = ReadOnly,
                Customization = Customization,
                AllowedValues = AllowedValues
            };
        }

        public void UpdateWithExisting(WorkItemTypeField existing)
        {
            Name = existing.Name;
            ReferenceName = existing.ReferenceName;
            Type = existing.Type;
            Description = existing.Description;
            DefaultValue = existing.DefaultValue;
            AllowGroups = existing.AllowGroups;
            Required = existing.Required;
            ReadOnly = existing.ReadOnly;
            Customization = existing.Customization;
            AllowedValues = existing.AllowedValues;
        }
    }


    [ApiPath("work/processes/{0}/workitemtypesbehaviors/{1}/behaviors", IncludeProject: false, IncludeTrailingSlash: false)]
    [ApiName("Work Item Type Behaviors")]
    public partial class WorkItemTypeBehavior : ProcessDefBase, ISynchronizeable<WorkItemTypeBehavior>
    {
        public class WorkItemTypeBehaviorRef
        {
            public string Id { get; set; }
        }
        public override string Id
        {
            get => Behavior?.Id;
            set {
                Behavior = new WorkItemTypeBehaviorRef() { Id = value };
                Behavior.Id = value;
            }
        }
        public WorkItemTypeBehaviorRef Behavior { get; set; }
        public bool IsDefault { get; set; }

        public WorkItemTypeBehavior CloneAsNew()
        {
            return new WorkItemTypeBehavior()
            {
                Behavior = new WorkItemTypeBehaviorRef()
            };
        }

        public void UpdateWithExisting(WorkItemTypeBehavior existing)
        {
            IsDefault = existing.IsDefault;
            Behavior = existing.Behavior;
        }
    }
    [ApiPath("work/processes/{0}/behaviors", IncludeProject: false, IncludeTrailingSlash: false)]
    [ApiName("Process Behaviors")]
    public partial class WorkItemBehavior : ProcessDefBase, ISynchronizeable<WorkItemBehavior>
    {
        public class WorkItemBehaviorRef
        {
            public string BehaviorRefName { get; set; }
        }
        public override string Id
        {
            get => ReferenceName;
            set => ReferenceName = value;
        }
        public string ReferenceName { get; set; }
        public string Color { get; set; }
        public int Rank { get; set; }
        public string Description { get; set; }
        public string Customization { get; set; }
        public WorkItemBehaviorRef Inherits { get; set; }

        public WorkItemBehavior CloneAsNew()
        {
            return new WorkItemBehavior()
            {
                Name = Name,
                ReferenceName = ReferenceName,
                Color = Color,
                Description = Description,
                Customization = Customization,
                Inherits = Inherits,
                Rank = Rank
            };
        }

        public void UpdateWithExisting(WorkItemBehavior existing)
        {
            Name = existing.Name;
            ReferenceName = existing.ReferenceName;
            Color = existing.Color;
            Description = existing.Description;
            Customization = existing.Customization;
            Inherits = new WorkItemBehaviorRef() { BehaviorRefName = existing.Inherits?.BehaviorRefName };
            Rank = existing.Rank;
        }
    }

    public interface ISynchronizeable<T>
    {
        T CloneAsNew();
        void UpdateWithExisting(T existing);
    }

    public abstract class ProcessDefBase : RestApiDefinition
    {
        public string Url { get; set; }
        public override void ResetObject()
        {
            SetSourceId(Id);
            Url = null;
            Id = null;
        }

        public override bool HasTaskGroups()
        {
            return false;
        }

        public override bool HasVariableGroups()
        {
            return false;
        }
    }


    [ApiPath("work/processes/{0}/workitemtypes/{1}/layout/groups/{2}/controls", IncludeProject: false, IncludeTrailingSlash: false)]
    [ApiName("WIT Control")]
    public class WorkItemControl : ProcessDefBase, ISynchronizeable<WorkItemControl>
    {
        public bool Inherited { get; set; }
        public string Label { get; set; }
        public string ControlType { get; set; }
        public bool ReadOnly { get; set; }
        public string Watermark { get; set; }
        public string Metadata { get; set; }
        public bool Visible { get; set; }
        public bool IsContribution { get; set; }

        public WorkItemControl CloneAsNew()
        {
            return new WorkItemControl()
            {
                Inherited = Inherited,
                Label = Label,
                ControlType = ControlType,
                ReadOnly = ReadOnly,
                Watermark = Watermark,
                Metadata = Metadata,
                Visible = Visible,
                IsContribution = IsContribution,
            };
        }

        public void UpdateWithExisting(WorkItemControl existing)
        {
            Inherited = existing.Inherited;
            Label = existing.Label;
            ControlType = existing.ControlType;
            ReadOnly = existing.ReadOnly;
            Watermark = existing.Watermark;
            Metadata = existing.Metadata;
            Visible = existing.Visible;
            IsContribution = existing.IsContribution;
        }
    }

    public class Contribution
    {
        public string ContributionId { get; set; }
    }

    [ApiPath("work/processes/{0}/workitemtypes/{1}/layout/pages/{2}/sections/{3}/groups", IncludeProject: false, IncludeTrailingSlash: false)]
    [ApiName("WIT Layout Group")]
    public class WorkItemGroup : ProcessDefBase, ISynchronizeable<WorkItemGroup>
    {
        [JsonIgnore]
        public override string Name { get => Label; }
        public bool Inherited { get; set; }
        public string Label { get; set; }
        public bool IsContribution { get; set; }
        public bool Visible { get; set; }
        public List<WorkItemControl> Controls { get; set; }
        public Contribution Contribution { get; set; }

        public WorkItemGroup CloneAsNew()
        {
            // When cloning a group, you need to ensure that the HTML controls stick with the group.. you cannot add an HTML control to a group later
            var htmlControls = Controls != null ? Controls.Where(c => c.ControlType.Equals("HtmlFieldControl", StringComparison.OrdinalIgnoreCase)) : null;
            return new()
            {
                Controls = htmlControls != null ? Controls : null,
                Label = Label,
                IsContribution = IsContribution,
                Visible = Visible,
            };
        }

        public void UpdateWithExisting(WorkItemGroup existing)
        {
            Label = existing.Label;
            IsContribution = existing.IsContribution;
            Visible = existing.Visible;
        }
    }

    public class WorkItemSection
    {
        public string Id { get; set; }
        public List<WorkItemGroup> Groups { get; set; }
    }

    [ApiPath("work/processes/{0}/workitemtypes/{1}/layout/pages", IncludeProject: false, IncludeTrailingSlash: false, IncludeIdOnUpdate = false, UpdateVerb = HttpVerbs.Patch)]
    [ApiName("WIT Layout Page")]
    public class WorkItemPage : ProcessDefBase, ISynchronizeable<WorkItemPage>
    {
        [JsonIgnore]
        public override string Name {
            get => Label;
        }
        public bool Inherited { get; set; }
        public bool Overridden { get; set; }
        public string Label { get; set; }
        public string PageType { get; set; }
        public bool Locked { get; set; }
        public bool Visible { get; set; }
        public bool IsContribution { get; set; }
        public List<WorkItemSection> Sections { get; set; }

        public WorkItemPage CloneAsNew()
        {
            return new WorkItemPage()
            {
                Inherited = Inherited,
                Overridden = Overridden,
                Label = Label,
                PageType = PageType,
                Locked = Locked,
                Visible = Visible,
                IsContribution = IsContribution,
                Sections = Sections,
            };
        }

        public void UpdateWithExisting(WorkItemPage existing)
        {
            Inherited = existing.Inherited;
            Overridden = existing.Overridden;
            Label = existing.Label;
            PageType = existing.PageType;
            Locked = existing.Locked;
            Visible = existing.Visible;
            IsContribution = existing.IsContribution;
            Sections = null;// existing.Sections;  // You will see 'The value specified for the following variable must be null: Sections.' If you try to update Sections
        }
    }

    public class SystemControl
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string ControlType { get; set; }
        public bool ReadOnly { get; set; }
        public string Watermark { get; set; }
        public bool Visible { get; set; }
        public bool IsContribution { get; set; }
    }

    [ApiPath("work/processes/{0}/workitemtypes/{1}/layout", IncludeProject: false, IncludeTrailingSlash: false)]
    [ApiName("WIT Layout")]
    public class WorkItemLayout : ProcessDefBase, ISynchronizeable<WorkItemLayout>
    {
        public List<WorkItemPage> Pages { get; set; }
        public List<SystemControl> SystemControls { get; set; }

        public WorkItemLayout CloneAsNew()
        {
            throw new NotImplementedException();
        }

        public void UpdateWithExisting(WorkItemLayout existing)
        {
            throw new NotImplementedException();
        }
    }



}
