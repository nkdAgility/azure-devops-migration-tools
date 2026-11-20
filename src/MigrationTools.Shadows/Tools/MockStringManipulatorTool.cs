using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools.Shadows
{
    public class MockStringManipulatorTool : IStringManipulatorTool
    {
        public string? ProcessString(string? value)
        {
            throw new NotImplementedException();
        }

        public string? ProcessString(string? value, string? fieldName)
        {
            throw new NotImplementedException();
        }
    }
}
