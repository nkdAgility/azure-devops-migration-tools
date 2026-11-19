namespace MigrationTools.Tools.Interfaces
{
    public interface IStringManipulatorTool
    {
        string? ProcessString(string? value);

        /// <summary>
        /// Processes the value of <paramref name="fieldName"/>; the field name is used in the review log entry
        /// when the value is truncated.
        /// </summary>
        string? ProcessString(string? value, string? fieldName);
    }
}
