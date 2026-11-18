using System;
using System.Reflection;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Sets a work item field that the TFS object model has made read-only on the client (#3270).
    /// In a revision that changes state, the object model's own copy of the process rules makes Activated, Resolved and
    /// Closed Date read-only and throws TF26194 from <c>Field.Value</c> before anything is sent. The service itself
    /// accepts the value with BypassRules: it was proven on the test organisations to keep a source date set this way in
    /// the save that changes the state. This calls the object model's internal setter, which skips only that client-side
    /// check; the value is still sent in the same save and still validated by the service.
    /// </summary>
    public static class ObjectModelFieldWriter
    {
        private static readonly Lazy<MethodInfo> SetFieldValueMethod = new Lazy<MethodInfo>(FindSetFieldValue);

        /// <summary>True when the internal setter exists in the loaded object model. Checked before migrating.</summary>
        public static bool IsAvailable => SetFieldValueMethod.Value != null;

        /// <summary>Sets <paramref name="fieldReferenceName"/> to <paramref name="value"/> without the client-side read-only check.</summary>
        public static void SetBelowClientRules(WorkItem workItem, string fieldReferenceName, object value)
        {
            var method = SetFieldValueMethod.Value
                ?? throw new InvalidOperationException("The TFS object model in use has no internal WorkItem.SetFieldValue(int, object, FieldFlags), so state-stamped fields cannot be set from the source.");
            var field = workItem.Fields[fieldReferenceName];
            var flags = Enum.ToObject(method.GetParameters()[2].ParameterType, 0);
            // The internal store holds dates in UTC; the public setter converts, this one does not.
            if (value is DateTime date) value = ToInternalDate(date);
            try
            {
                method.Invoke(workItem, new[] { field.FieldDefinition.Id, value, flags });
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }

        /// <summary>Converts a date to the UTC value the object model stores internally (an unspecified kind is read as local, as the public setter does).</summary>
        public static DateTime ToInternalDate(DateTime value)
        {
            return DateTime.SpecifyKind(value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime(), DateTimeKind.Utc);
        }

        private static MethodInfo FindSetFieldValue()
        {
            var flagsType = typeof(WorkItem).Assembly.GetType("Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemFieldData+FieldFlags");
            if (flagsType == null) return null;
            return typeof(WorkItem).GetMethod("SetFieldValue", BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] { typeof(int), typeof(object), flagsType }, null);
        }
    }
}
